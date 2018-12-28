using BEL.CommonDataContract;
using BEL.DataAccessLayer;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEL.DailyReminder
{
    class Program
    {
        /// <summary>
        /// Start Execution form here
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                string artworkURL = BELDataAccessLayer.Instance.GetSiteURL(SiteURLs.ARTWORKSITEURL);
                using (ClientContext clientContext = BELDataAccessLayer.Instance.CreateClientContext(artworkURL))
                {
                    Web web = BELDataAccessLayer.Instance.CreateWeb(clientContext);
                    List configList = web.Lists.GetByTitle("AutoApprovalConfiguration");
                    CamlQuery configCaml = new CamlQuery();
                    configCaml.ViewXml = @"<View>
                                                        <Query>
                                                            <Where>
                                                                <Eq>
                                                                   <FieldRef Name='DailyReminderIsActive' />
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

                                DataTable tblReminderData = GetEscalationTable();
                                tblReminderData = GetDailyReminder(clientContext, web, mainListName, approverMatrixListName, activityLogListName, tblReminderData, 2);
                                if (tblReminderData != null && tblReminderData.Rows.Count != 0)
                                {
                                    //   SendEscalationEmail(tblReminderData, clientContext, web);
                                    SendReminderEmail(tblReminderData, clientContext, web);
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
            catch (Exception ex)
            {
                Console.WriteLine("Error While Execute Main Method");
                Console.Write(ex.StackTrace + "==>" + ex.Message);
                Logger.Error("Error While Execute Main Method");
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Prepare DataTable Schema
        /// </summary>
        /// <returns>DataTable Schema</returns>
        public static DataTable GetEscalationTable()
        {
            DataTable tblEscalation = new DataTable();
            tblEscalation.Columns.Add(new DataColumn("UserFullName", typeof(string)));
            tblEscalation.Columns.Add(new DataColumn("UserEmail", typeof(string)));
            tblEscalation.Columns.Add(new DataColumn("FromEmail", typeof(string)));
            tblEscalation.Columns.Add(new DataColumn("ToEmail", typeof(string)));
            tblEscalation.Columns.Add(new DataColumn("CCEmail", typeof(string)));
            tblEscalation.Columns.Add(new DataColumn("Subject", typeof(string)));
            tblEscalation.Columns.Add(new DataColumn("Body", typeof(string)));
            tblEscalation.Columns.Add(new DataColumn("ApplicationName", typeof(string)));
            tblEscalation.Columns.Add(new DataColumn("FormName", typeof(string)));
            tblEscalation.Columns.Add(new DataColumn("ReferenceNo", typeof(string)));
            tblEscalation.Columns.Add(new DataColumn("ItemCode", typeof(string)));
            tblEscalation.Columns.Add(new DataColumn("BU", typeof(string)));
            tblEscalation.Columns.Add(new DataColumn("ModelName", typeof(string)));
            tblEscalation.Columns.Add(new DataColumn("RequestDate", typeof(string)));
            tblEscalation.Columns.Add(new DataColumn("AssignDate", typeof(string)));
            tblEscalation.Columns.Add(new DataColumn("Link", typeof(string)));
            tblEscalation.Columns.Add(new DataColumn("PendingSince", typeof(string)));
            tblEscalation.Columns.Add(new DataColumn("ActionStatus", typeof(string)));
            tblEscalation.Columns.Add(new DataColumn("ID", typeof(string)));
            tblEscalation.Columns.Add(new DataColumn("ListName", typeof(string)));
            tblEscalation.Columns.Add(new DataColumn("Role", typeof(string)));

            return tblEscalation;
        }

        /// <summary>
        /// Esclates the requests.
        /// </summary>
        /// <param name="siteContext">The site context.</param>
        /// <param name="siteWeb">The site web.</param>
        /// <param name="mainListName">Name of the main list.</param>
        /// <param name="approverMatrixListName">Name of the approver matrix list.</param>
        /// <param name="activityLogListName">Name of the activity log list.</param>
        public static DataTable GetDailyReminder(ClientContext siteContext, Web siteWeb, string mainListName, string approverMatrixListName, string activityLogListName, DataTable tblEscalation, int actionStatus)
        {
            List approverList = siteWeb.Lists.GetByTitle(approverMatrixListName);
            List mainList = siteWeb.Lists.GetByTitle(mainListName);
            List activityLogList = siteWeb.Lists.GetByTitle(activityLogListName);
            CamlQuery query = new CamlQuery();
            string qry = string.Empty;
            string dueDate = string.Empty;
            dueDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
            //            qry = @"<And>
            //                                <Leq>
            //                                    <FieldRef Name='DueDate' />
            //                                    <Value Type='DateTime' IncludeTimeValue='FALSE'>" + dueDate + @"</Value>
            //                                </Leq>
            //                                <And>
            //                                    <Eq>
            //                                        <FieldRef Name='Status' />
            //                                        <Value Type='Text'>" + ApproverStatus.PENDING + @"</Value>
            //                                    </Eq>
            //                                    <Eq>
            //                                        <FieldRef Name='IsReminder' />
            //                                        <Value Type='Boolean'>1</Value>
            //                                    </Eq>
            //                                </And>
            //                        </And>";
            qry = @"<And><And><And><Eq><FieldRef Name='Status' /><Value Type='Choice'>Pending</Value></Eq><IsNotNull><FieldRef Name='Approver' /></IsNotNull></And><Eq><FieldRef Name='IsReminder' /><Value Type='Boolean'>1</Value></Eq></And><Geq><FieldRef Name='DueDate' /><Value Type='DateTime'><Today /></Value></Geq></And>";


            query.ViewXml = @"<View>
                                    <Query>
                                        <Where>
                                            " + qry + @"
                                        </Where>
                                    </Query>
                                </View>";
            ListItemCollection approverDetails = approverList.GetItems(query);
            siteContext.Load(approverDetails);
            siteContext.ExecuteQuery();
            if (approverDetails != null)
            {
                for (int i = 0; i < approverDetails.Count; i++)
                {

                    string esclationTo = string.Empty;   ////not required
                    tblEscalation = DailyReminder(siteContext, siteWeb, approverDetails[i], mainListName, approverMatrixListName, tblEscalation, ref esclationTo, actionStatus);
                }
            }
            return tblEscalation;
        }

        /// <summary>
        /// Esclates the request.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="web">The web.</param>
        /// <param name="item">The item.</param>
        /// <param name="mainListName">Name of the main list.</param>
        /// <param name="approverListName">Name of the approver list.</param>
        /// <returns>to users</returns>
        public static DataTable DailyReminder(ClientContext context, Web web, ListItem item, string mainListName, string approverListName, DataTable tblEscalation, ref string toUser, int actionStatus)
        {
            try
            {
                DataRow dr = tblEscalation.NewRow();
                string userIDs = BELDataAccessLayer.GetEmailsFromPersonField(context, web, item["Approver"] as FieldUserValue[]);
                dr["UserEmail"] = dr["ToEmail"] = BELDataAccessLayer.GetEmailUsingUserID(context, web, userIDs);
                dr["UserFullName"] = BELDataAccessLayer.GetNameUsingUserID(context, web, userIDs);
                EmailHelper eHelper = new EmailHelper();
                //List<ListItemDetail> listDetails = new List<ListItemDetail>();
                int reqId = (item["RequestID"] as FieldLookupValue).LookupId;
                List mainList = web.Lists.GetByTitle(mainListName);
                ListItem mainItem = mainList.GetItemById(reqId);
                context.Load(mainItem);
                context.Load(mainList, m => m.DefaultDisplayFormUrl);
                context.ExecuteQuery();

                dr["ApplicationName"] = Convert.ToString(item["ApplicationName"]);
                dr["Role"] = Convert.ToString(item["Role"]);
                dr["FormName"] = Convert.ToString(item["FormName"]);
                dr["RequestDate"] = (mainItem["RequestDate"] != null ? Convert.ToDateTime(mainItem["RequestDate"]).ToString("dd-MM-yyyy") : string.Empty);
                dr["AssignDate"] = (item["AssignDate"] != null ? Convert.ToDateTime(item["AssignDate"]).ToString("dd-MM-yyyy") : string.Empty);
                string link = string.Empty;
                string referenceNo = string.Empty;
                link = "#URL" + mainList.DefaultDisplayFormUrl + "?ID=" + mainItem["ID"];
                dr["ListName"] = mainListName;
                dr["ID"] = mainItem.Id;
                dr["Link"] = link;
                dr["ReferenceNo"] = Convert.ToString(mainItem["ReferenceNo"]);////referenceNo;
                dr["ItemCode"] = Convert.ToString(mainItem["ItemCode"]);////referenceNo;
                dr["BU"] = Convert.ToString(mainItem["BusinessUnit"]);////referenceNo;
                dr["ModelName"] = Convert.ToString(mainItem["Model"]);////referenceNo;
                dr["ActionStatus"] = actionStatus;
                dr["PendingSince"] = item["DueDate"] != null ? Convert.ToDateTime(item["DueDate"]).ToString("dd/MM/yyyy") : string.Empty;
                ////if (item["DueDate"] != null)
                ////{
                ////    dr["PendingSince"] = GetPendingDays(Convert.ToDateTime(item["DueDate"]));
                ////}
                ////else
                ////{
                ////    dr["PendingSince"] = "0";
                ////}
                if (!Convert.ToString(item["Role"]).Equals("ABSQ Team"))
                {
                    tblEscalation.Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Request Escalation: " + ex.Message);
                Console.Write(ex.StackTrace + "==>" + ex.Message);
                Logger.Error("Error Request Escalation: " + ex.Message);
            }
            return tblEscalation;
        }

        /// <summary>
        /// NO More Use
        /// </summary>
        /// <param name="tblEscalation"></param>
        /// <param name="clientContext"></param>
        /// <param name="web"></param>
        public static void SendEscalationEmail(DataTable tblEscalation, ClientContext clientContext, Web web)
        {
            string appName = string.Empty;
            string formName = string.Empty;
            string template = string.Empty;

            var distinctUsers = (from row in tblEscalation.AsEnumerable()
                                 select row.Field<string>("UserEmail")).Distinct();

            foreach (DataRow dr in tblEscalation.Rows)
            {
                appName = dr["ApplicationName"].ToString();
                formName = dr["FormName"].ToString();
                EmailHelper eHelper = new EmailHelper();
                template = dr["ActionStatus"].ToString();

                Dictionary<string, string> custom = new Dictionary<string, string>();
                custom["ReferenceNo"] = dr["ReferenceNo"].ToString();
                custom["Link"] = dr["Link"].ToString(); ;
                custom["AssignDate"] = dr["AssignDate"].ToString();
                custom["UserName"] = dr["UserFullName"].ToString();
                ListItemDetail listDetail = new ListItemDetail();
                listDetail.ItemId = Convert.ToInt32(dr["ID"].ToString());
                listDetail.ListName = dr["ListName"].ToString();
                List<ListItemDetail> listDetails = new List<ListItemDetail>();
                listDetails.Add(listDetail);
                Dictionary<string, string> email = new Dictionary<string, string>();

                email = eHelper.GetEmailBody(clientContext, web, EmailTemplateName.REMINDEREMAILTEMPLATE, listDetails, custom, null, appName, formName);
                eHelper.SendMail(appName, formName, EmailTemplateName.REMINDEREMAILTEMPLATE, email["Subject"], email["Body"], BELDataAccessLayer.Instance.GetConfigVariable("FromEmailAddress"), dr["ToEmail"].ToString(), dr["CCEmail"].ToString(), false);
            }
        }

        /// <summary>
        /// Prepare Email Body and Send to User
        /// </summary>
        /// <param name="tblEscalation">Data Table</param>
        /// <param name="clientContext">Client Context</param>
        /// <param name="web">Web Object</param>
        public static void SendReminderEmail(DataTable tblEscalation, ClientContext clientContext, Web web)
        {
            var tempdistinctUsers = (from row in tblEscalation.AsEnumerable()
                                     select row.Field<string>("UserEmail")).Distinct();

            var distinctUsers = tempdistinctUsers.SelectMany(l => l.Split(',')).Distinct().ToList();

            foreach (var userEmail in distinctUsers)
            {
                DataRow[] filteredDataTable = tblEscalation.Select(@"UserEmail Like '%" + userEmail + "%'");

                string htmlTbl = @"<p>Dear User,<p>
                                    <p>Below requests are pending for your action. Kindly take corrective action to avoid any further reminders. 
                                            <a href ='#URL" + BELDataAccessLayer.Instance.GetConfigVariable("ArtworkSiteURL") + @"/'> Click Here</a> to view all pending artworks.            
                                            <p>   
                                                         
                                    <table width='100%' cellspacing='2' cellpadding='2' border='2'>
                                    <tr>
                                    <th width='5%'>Sr No</th>
                                    <th width='15%'>Artwork</th>
                                    <th>Reference No</th>
                                    <th>Item Code</th>
                                    <th>Business Unit</th>
                                    <th>Model Name</th>
                                    <th>Assigned Date</th>
                                    <th>Due Date</th>
                                    </tr>";
                int i = 1;
                foreach (DataRow dr in filteredDataTable)
                {
                    try
                    {

                        string frmName = dr["FormName"].ToString();
                        frmName = frmName.Replace("Form", string.Empty);
                        htmlTbl += @"<tr>
                                        <td>" + i.ToString() + @"</td>
                                        <td>" + frmName + @"</td> 
                                        <td>" + dr["ReferenceNo"] + @"</td>
                                        <td>" + dr["ItemCode"] + @"</td>
                                        <td>" + dr["BU"] + @"</td>
                                        <td>" + dr["ModelName"] + @"</td>
                                        <td>" + dr["AssignDate"] + @"</td>
                                        <td>" + dr["PendingSince"] + @"</td>
                                    </tr>";
                        i++;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
                htmlTbl += "</table><p><i>This is an auto generated email, please do not reply.</i></p><p>Thanks & Regards,<br/>Artwrok Team</p>";
                EmailHelper eHelper = new EmailHelper();
                eHelper.SendMail("All", "All", EmailTemplateName.REMINDEREMAILTEMPLATE, "Daily Reminder - Requests pending for your action", htmlTbl, BELDataAccessLayer.Instance.GetConfigVariable("FromEmailAddress"), userEmail, string.Empty, false);
            }
        }

        private static string GetPendingDays(DateTime dueDate)
        {
            int totaldays = Convert.ToInt32((DateTime.Now.Date - dueDate).TotalDays);
            return Convert.ToString(totaldays);
        }
    }
}
