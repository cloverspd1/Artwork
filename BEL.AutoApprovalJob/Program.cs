namespace BEL.AutoApprovalJob
{
    using BEL.ArtworkWorkflow.BusinessLayer;
    using BEL.ArtworkWorkflow.Models.Common;
    using BEL.ArtworkWorkflow.Models.ExistingArtwork;
    using BEL.ArtworkWorkflow.Models.NewArtwork;
    using BEL.CommonDataContract;
    using BEL.DataAccessLayer;
    using Microsoft.SharePoint.Client;
    using Microsoft.SharePoint.Client.Taxonomy;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class Program
    {
        /// <summary>
        /// Main Method where Code execuation start
        /// </summary>
        /// <param name="args">Default argument.</param>
        static void Main(string[] args)
        {
            try
            {
                string artworkURL = BELDataAccessLayer.Instance.GetSiteURL(SiteURLs.ARTWORKSITEURL);
                using (ClientContext clientContext = BELDataAccessLayer.Instance.CreateClientContext(artworkURL))
                {
                    Web web = BELDataAccessLayer.Instance.CreateWeb(clientContext);
                    List configList = web.Lists.GetByTitle(ArtworkListNames.AUTOAPPROVALCONFIGURATIONLIST);
                    CamlQuery configCaml = new CamlQuery();
                    configCaml.ViewXml = @"<View>
                                    <Query>
                                        <Where>
                                            <Eq>
                                               <FieldRef Name='IsActive' />
                                                <Value Type='Boolean'>1</Value>
                                            </Eq>
                                        </Where>
                                    </Query>
                                </View>";
                    ListItemCollection items = configList.GetItems(configCaml);
                    clientContext.Load(items);
                    clientContext.ExecuteQuery();

                    if (items != null)
                    {
                        string applicationName = string.Empty, formName = string.Empty;

                        foreach (ListItem item in items)
                        {
                            try
                            {
                                if (item["ApplicationName"] != null)
                                {
                                    applicationName = (item["ApplicationName"] as TaxonomyFieldValue).Label;
                                }
                                if (item["FormName"] != null)
                                {
                                    formName = (item["FormName"] as TaxonomyFieldValue).Label;
                                }
                                string mainListName = Convert.ToString(item["Title"]);
                                string approverMatrixListName = Convert.ToString(item["ApproverMatrixListName"]);
                                string activityLogListName = Convert.ToString(item["ActivityLogListName"]);

                                AutoApproveRequests(clientContext, web, mainListName, approverMatrixListName, activityLogListName, applicationName, formName);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex);
                            }
                        }

                    }

                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("Error While Execute Main Method");
                Console.Write(ex.StackTrace + "==>" + ex.Message);
                Logger.Error("Error While Execute Main Method");
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Auto Approve/Reject Request
        /// </summary>
        /// <param name="context">SP context</param>
        /// <param name="web">SP web</param>
        /// <param name="mainListName">List Name</param>
        /// <param name="approverMatrixListName">Approval Matrix list name</param>
        /// <param name="activityLogListName">ACtivity log name</param>
        /// <param name="appName">Application Name</param>
        /// <param name="formName">Form Name</param>
        public static void AutoApproveRequests(ClientContext context, Web web, string mainListName, string approverMatrixListName, string activityLogListName, string applicationName, string formName)
        {
            List<ApprovalData> approvalData = new List<ApprovalData>();
            List approverList = web.Lists.GetByTitle(approverMatrixListName);
            CamlQuery query = new CamlQuery();
            string yesterDayDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ssZ");
            string qry = @"
                            <And>
                                    <IsNotNull><FieldRef Name='Approver'></FieldRef></IsNotNull>
                                 <And>
                                    <Eq>
                                        <FieldRef Name='Status' />
                                        <Value Type='Text'>" + ApproverStatus.PENDING + @"</Value>
                                    </Eq>
                                    <Eq>
                                        <FieldRef Name='DueDate' />
                                        <Value Type='DateTime' IncludeTimeValue='FALSE'>" + yesterDayDate + @"</Value>
                                    </Eq>
                           </And></And>";
            query.ViewXml = @"<View>
                                    <Query>
                                        <Where>
                                            " + qry + @"
                                        </Where>
                                    </Query>
                                </View>";
            ListItemCollection approverDetails = approverList.GetItems(query);
            context.Load(approverDetails);
            context.ExecuteQuery();
            if (approverDetails != null)
            {
                List<string> processedRequest = new List<string>();
                foreach (ListItem item in approverDetails)
                {
                    try
                    {
                        ApprovalData obj = new ApprovalData();
                        obj.RequestID = item["RequestID"] != null ? (item["RequestID"] as FieldLookupValue).LookupId : 0;
                        obj.Approver = BELDataAccessLayer.GetEmailsFromPersonField(context, web, item["Approver"] as FieldUserValue[]);
                        obj.ApproverName = BELDataAccessLayer.GetNameFromPersonField(context, web, item["Approver"] as FieldUserValue[]);
                        obj.IsAutoApproval = item["IsAutoApproval"] != null ? (bool)item["IsAutoApproval"] : false;
                        obj.IsAutoRejection = item["IsAutoRejection"] != null ? (bool)item["IsAutoRejection"] : false;
                        obj.SectionName = Convert.ToString(item["SectionName"]);
                        obj.Levels = Convert.ToString(item["Levels"]);
                        if (!string.IsNullOrEmpty(obj.Approver))
                        {
                            approvalData.Add(obj);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
                ApproveRequest(context, web, mainListName, approverMatrixListName, activityLogListName, applicationName, formName, approvalData);
            }
        }

        /// <summary>
        /// Auto Approve/Reject Request
        /// </summary>
        /// <param name="context">SP context</param>
        /// <param name="web">SP web</param>
        /// <param name="mainListName">List Name</param>
        /// <param name="approverMatrixListName">Approval Matrix list name</param>
        /// <param name="activityLogListName">ACtivity log name</param>
        /// <param name="appName">Application Name</param>
        /// <param name="formName">Form Name</param>
        /// <param name="approvalData">Approval Data.</param>
        public static void ApproveRequest(ClientContext context, Web web, string mainListName, string approverMatrixListName, string activityLogListName, string appName, string formName, List<ApprovalData> approvalData)
        {
            if (approvalData != null && approvalData.Count != 0)
            {
                foreach (ApprovalData obj in approvalData)
                {
                    if (obj.IsAutoApproval || obj.IsAutoRejection)
                    {
                        try
                        {
                            ActionStatus status = new ActionStatus();
                            string userid = obj.Approver.Split(',').FirstOrDefault();
                            string username = obj.ApproverName.Split(',').FirstOrDefault();
                            string adminuserid = BELDataAccessLayer.Instance.GetConfigVariable("AdminUserID");
                            string adminUserName = BELDataAccessLayer.Instance.GetConfigVariable("AdminUserName");
                            bool saveRequired = false;
                            Dictionary<string, string> objDict = new Dictionary<string, string>();
                            objDict.Add(Parameter.FROMNAME, formName);
                            objDict.Add(Parameter.ITEMID, obj.RequestID.ToString());
                            objDict.Add(Parameter.USEREID, userid);
                            objDict.Add("UserName", username);
                            var formObj = getFormobj(formName);
                            if (formName == ArtworkFormNames.NEWARTWORKFORM)
                            {
                                NewArtworkContract contract = NewArtworkBusinessLayer.Instance.GetNewArtworkDetails(objDict);
                                formObj = contract.Forms.FirstOrDefault();
                            }
                            else if (formName == ArtworkFormNames.EXISTINGARTWORKFORM)
                            {
                                ExistingArtworkContract contract = ExistingArtworkBusinessLayer.Instance.GetExistingArtworkDetails(objDict);
                                formObj = contract.Forms.FirstOrDefault();
                            }

                            if (formObj != null && formObj.SectionsList != null && formObj.SectionsList.Count > 0)
                            {
                                var sectionDetails = formObj.SectionsList.FirstOrDefault(f => f.SectionName == obj.SectionName);
                                dynamic actualSection = Convert.ChangeType(sectionDetails, sectionDetails.GetType());
                                if (sectionDetails != null)
                                {
                                    if (obj.IsAutoApproval)
                                    {
                                        saveRequired = true;
                                        sectionDetails.ButtonCaption = "Auto Approved By Admin";
                                        sectionDetails.ActionStatus = ButtonActionStatus.NextApproval;
                                        actualSection.CurrentApprover.Comments = "Request is Auto Approved by " + adminUserName + " on behalf of " + username;
                                        actualSection.Files = new List<FileDetails>();
                                    }
                                    else if (obj.IsAutoRejection)
                                    {
                                        saveRequired = true;
                                        sectionDetails.ButtonCaption = "Auto Rejected By Admin";
                                        sectionDetails.ActionStatus = ButtonActionStatus.Rejected;
                                        actualSection.OldArtworkRejectedDate = DateTime.Now;
                                        actualSection.OldArtworkRejectedComment = actualSection.CurrentApprover.Comments = "Request is Auto Rejected by " + adminUserName + " on behalf of " + username;
                                        ////actualSection.Files = new List<FileDetails>();
                                    }
                                    if (saveRequired)
                                    {
                                        objDict[Parameter.ACTIONPER] = sectionDetails.ButtonCaption + "|" + sectionDetails.ActionStatus;
                                        if (formName == ArtworkFormNames.NEWARTWORKFORM)
                                        {
                                            status = NewArtworkBusinessLayer.Instance.SaveBySection(sectionDetails, objDict);
                                        }
                                        else
                                        {
                                            status = ExistingArtworkBusinessLayer.Instance.SaveBySection(sectionDetails, objDict);
                                        }
                                        Logger.Info(status);
                                    }

                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get IForm Object
        /// </summary>
        /// <param name="formname">Form Name in String</param>
        /// <returns>New object of Iform</returns>
        public static IForm getFormobj(string formname)
        {
            if (formname == ArtworkFormNames.NEWARTWORKFORM)
            {
                return new NewArtworkForm(true);
            }
            else
            {
                return new ExistingArtworkForm(true);
            }
        }
    }
}
