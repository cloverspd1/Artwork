namespace BEL.ArtworkWorkflow.BusinessLayer
{
    using BEL.CommonDataContract;
    using BEL.ArtworkWorkflow.Models.NewArtwork;
    using BEL.ArtworkWorkflow.Models.Common;
    using BEL.DataAccessLayer;
    using Microsoft.SharePoint.Client;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using ArtworkWorkflow.Models;
    using Newtonsoft.Json;
    using BEL.ArtworkWorkflow.Models.Master;
    using System.IO;

    /// <summary>
    /// New Artwork Business Layer
    /// </summary>
    public class NewArtworkBusinessLayer
    {
        /// <summary>
        /// The New Artwork Business Layer
        /// </summary>
        private static readonly Lazy<NewArtworkBusinessLayer> Lazy =
         new Lazy<NewArtworkBusinessLayer>(() => new NewArtworkBusinessLayer());

        /// <summary>
        /// New Artwork Business Layer Instance
        /// </summary>
        public static NewArtworkBusinessLayer Instance
        {
            get
            {
                return Lazy.Value;
            }
        }

        /// <summary>
        /// The padlock
        /// </summary>
        private static readonly object Padlock = new object();

        /// <summary>
        /// The context
        /// </summary>
        private ClientContext context = null;

        /// <summary>
        /// The web
        /// </summary>
        private Web web = null;

        /// <summary>
        /// New Artwork BusinessLayer
        /// </summary>
        private NewArtworkBusinessLayer()
        {
            string siteURL = BELDataAccessLayer.Instance.GetSiteURL(SiteURLs.ARTWORKSITEURL);
            if (!string.IsNullOrEmpty(siteURL))
            {
                if (this.context == null)
                {
                    this.context = BELDataAccessLayer.Instance.CreateClientContext(siteURL);
                }
                if (this.web == null)
                {
                    this.web = BELDataAccessLayer.Instance.CreateWeb(this.context);
                }
            }
        }

        #region "GET DATA"
        /// <summary>
        /// Gets the NPD details.
        /// </summary>
        /// <param name="objDict">The object dictionary.</param>
        /// <returns>byte array</returns>
        public NewArtworkContract GetNewArtworkDetails(IDictionary<string, string> objDict)
        {
            NewArtworkContract contract = new NewArtworkContract();
            if (objDict != null && objDict.ContainsKey(Parameter.FROMNAME) && objDict.ContainsKey(Parameter.ITEMID) && objDict.ContainsKey(Parameter.USEREID))
            {
                string formName = objDict[Parameter.FROMNAME];
                int itemId = Convert.ToInt32(objDict[Parameter.ITEMID]);
                string userID = objDict[Parameter.USEREID];
                string userName = objDict["UserName"];
                IForm newArtworkForm = new NewArtworkForm(true);
                newArtworkForm = BELDataAccessLayer.Instance.GetFormData(this.context, this.web, ArtworkApplicationNameConstants.ARTWORKAPP, formName, itemId, userID, newArtworkForm);
                if (newArtworkForm != null && newArtworkForm.SectionsList != null && newArtworkForm.SectionsList.Count > 0)
                {
                    var sectionDetails = newArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ARTWORKDETAILSECTION) as NewArtworkDetailSection;
                    if (sectionDetails != null)
                    {
                        List<IMasterItem> rolewiseapprovers = sectionDetails.MasterData.FirstOrDefault(p => p.NameOfMaster == ArtworkMasters.ROLEWISEAPPROVERMASTER).Items;
                        List<RoleWiseApproverMasterListItem> rolewiseapproversList = rolewiseapprovers.ConvertAll(p => (RoleWiseApproverMasterListItem)p);
                        if (itemId == 0)
                        {
                            sectionDetails.Product = "Local";
                            List<IMasterItem> artworkTypeList = sectionDetails.MasterData.FirstOrDefault(p => p.NameOfMaster == ArtworkMasters.ARTWORKTYEMASTER).Items;
                            List<ArtworkTypeMasterListItem> lstDefaultSelectedArtworkType = artworkTypeList.ConvertAll(p => (ArtworkTypeMasterListItem)p).FindAll(x => x.IsDefaultSelected == true);
                            string strDefaultArtworkType = string.Empty;

                            foreach (ArtworkTypeMasterListItem item in lstDefaultSelectedArtworkType)
                            {
                                strDefaultArtworkType = strDefaultArtworkType + item.Title + ",";
                            }
                            if (!string.IsNullOrWhiteSpace(strDefaultArtworkType))
                            {
                                strDefaultArtworkType = strDefaultArtworkType.Remove(strDefaultArtworkType.Length - 1);
                            }
                            sectionDetails.ArtworkType = strDefaultArtworkType;
                            List<IMasterItem> approvers = sectionDetails.MasterData.FirstOrDefault(p => p.NameOfMaster == ArtworkMasters.APPROVERMASTER).Items.ToList();
                            List<ApproverMasterListItem> approverList = approvers.ConvertAll(p => (ApproverMasterListItem)p);
                            ApproverMasterListItem obj = null;
                            bool isValidRequester = false;
                            foreach (ApproverMasterListItem item in approverList)
                            {
                                string[] strRequester = item.Requester.Split(',');
                                foreach (string str in strRequester)
                                {
                                    ////if (str == userID && (item.BusinessUnit == ArtworkCommonConstant.CPBUSINESSUNITCODE || item.BusinessUnit == ArtworkCommonConstant.LUMBUSINESSUNITCODE || item.BusinessUnit == ArtworkCommonConstant.MRBUSINESSUNITCODE || item.BusinessUnit == ArtworkCommonConstant.EXPBUSINESSUNITCODE))
                                    if (str == userID && (item.DomesticOrImported == ArtworkCommonConstant.IMPORTED || item.DomesticOrImported == ArtworkCommonConstant.LOCAL) && (item.BusinessUnit == ArtworkCommonConstant.CPBUSINESSUNITCODE || item.BusinessUnit == ArtworkCommonConstant.KAPBUSINESSUNITCODE || item.BusinessUnit == ArtworkCommonConstant.FANSBUSINESSUNITCODE || item.BusinessUnit == ArtworkCommonConstant.DAPBUSINESSUNITCODE || item.BusinessUnit == ArtworkCommonConstant.LIGHTBUSINESSUNITCODE || item.BusinessUnit == ArtworkCommonConstant.LUMBUSINESSUNITCODE || item.BusinessUnit == ArtworkCommonConstant.MRBUSINESSUNITCODE || item.BusinessUnit == ArtworkCommonConstant.EXPBUSINESSUNITCODE))
                                    {
                                        isValidRequester = true;
                                    }
                                }
                                if (isValidRequester)
                                {
                                    obj = item;
                                    
                                    break;
                                }
                            }
                            if (obj != null)
                            {
                                ////sectionDetails.DomesticOrImported = obj.DomesticOrImported;
                                ////Get Department User
                                sectionDetails.ApproversList.ForEach(p =>
                                {
                                    if (p.Role == ArtworkRoles.CREATOR || p.Role == ArtworkRoles.MARKETINGLEVEL1APPROVER)
                                    {
                                        p.Approver = userID;
                                        p.ApproverName = userName;
                                    }
                                    ////else if (p.Role == ArtworkRoles.BUAPPROVER || p.Role == ArtworkRoles.MARKETINGLEVEL2APPROVER)
                                    ////{
                                    ////    p.Approver = obj.BUHead;
                                    ////    p.ApproverName = obj.BUHeadName;
                                    ////}
                                    ////else if (p.Role == ArtworkRoles.SCMLEVEL1APPROVER)
                                    ////{
                                    ////    p.Approver = obj.SCMLevel1;
                                    ////    p.ApproverName = obj.SCMLevel1Name;
                                    ////}
                                    ////else if (p.Role == ArtworkRoles.SCMLEVEL2APPROVER)
                                    ////{
                                    ////    p.Approver = obj.SCMLevel2;
                                    ////    p.ApproverName = obj.SCMLevel2Name;
                                    ////}
                                    ////else if (p.Role == ArtworkRoles.ADBLEVEL1APPROVER)
                                    ////{
                                    ////    p.Approver = obj.ADBLevel1;
                                    ////    p.ApproverName = obj.ADBLevel1Name;
                                    ////}
                                    ////else if (p.Role == ArtworkRoles.ADBLEVEL2APPROVER)
                                    ////{
                                    ////    p.Approver = obj.ADBLevel2;
                                    ////    p.ApproverName = obj.ADBLevel2Name;
                                    ////}
                                    ////else if (p.Role == ArtworkRoles.QALEVEL1APPROVER)
                                    ////{
                                    ////    p.Approver = obj.QALevel1;
                                    ////    p.ApproverName = obj.QALevel1Name;
                                    ////}
                                    ////else if (p.Role == ArtworkRoles.QALEVEL2APPROVER)
                                    ////{
                                    ////    p.Approver = obj.QALevel2;
                                    ////    p.ApproverName = obj.QALevel2Name;
                                    ////}
                                });
                            }
                            if (rolewiseapproversList != null)
                            {
                                ////Get Department User
                                sectionDetails.ApproversList.ForEach(p =>
                                {
                                    if (p.Role == ArtworkRoles.ABSQTEAM)
                                    {
                                        p.Approver = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.ABSQTEAM).Approvers;
                                        p.ApproverName = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.ABSQTEAM).ApproversName;
                                    }
                                    else if (p.Role == ArtworkRoles.ABSQAPPROVER)
                                    {
                                        p.Approver = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.ABSQAPPROVER).Approvers;
                                        p.ApproverName = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.ABSQAPPROVER).ApproversName;
                                    }

                                    sectionDetails.ABSAdmin = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.ABSADMIN).Approvers;
                                    sectionDetails.ABSAdminName = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.ABSADMIN).ApproversName;

                                    ////sectionDetails.CC = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.CC).Approvers;
                                    ////sectionDetails.CCName = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.CC).ApproversName;

                                    ////sectionDetails.Legal = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.LEGAL).Approvers;
                                    ////sectionDetails.LegalName = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.LEGAL).ApproversName;
                                });
                            }
                        }
                        else
                        {
                            sectionDetails.ApproversList.Remove(sectionDetails.ApproversList.FirstOrDefault(p => p.Role == UserRoles.VIEWER));

                            if (sectionDetails != null)
                            {
                                sectionDetails.FNLSamplePhoto = sectionDetails.Files != null && sectionDetails.Files.Count > 0 ? JsonConvert.SerializeObject(sectionDetails.Files.Where(x => !string.IsNullOrEmpty(sectionDetails.FNLSamplePhoto) && sectionDetails.FNLSamplePhoto.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                            }

                            var absqTeamSection = newArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.ABSQTEAMSECTION)) as NewArtworkABSQTeamSection;
                            if (absqTeamSection != null)
                            {
                                absqTeamSection.ArtworkAttachmentTempList = new List<NewArtworkAttachment>();
                                absqTeamSection.OldModelName = absqTeamSection.ModelName;
                                absqTeamSection.ArtworkAttachmentTempList.AddRange(absqTeamSection.ArtworkAttachmentList.ConvertAll(o => (o as NewArtworkAttachment)));

                                if (absqTeamSection.ArtworkAttachmentTempList.Count == 0 && absqTeamSection.IsActive)
                                {
                                    var detailSection = newArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.ARTWORKDETAILSECTION)) as NewArtworkDetailSection;
                                    string strArtworkType = detailSection.ArtworkType;
                                    List<IMasterItem> artworkTypeList = sectionDetails.MasterData.FirstOrDefault(p => p.NameOfMaster == ArtworkMasters.ARTWORKTYEMASTER).Items;
                                    List<ArtworkTypeMasterListItem> lstDefaultSelectedArtworkType = artworkTypeList.ConvertAll(p => (ArtworkTypeMasterListItem)p);
                                    if (!string.IsNullOrWhiteSpace(strArtworkType))
                                    {
                                        string[] artworkTypeArray = strArtworkType.Split(',');
                                        foreach (string item in artworkTypeArray)
                                        {
                                            NewArtworkAttachment obj = new NewArtworkAttachment();
                                            obj.ArtworkType = item;
                                            obj.ArtworkTypeCode = lstDefaultSelectedArtworkType.FirstOrDefault(x => x.Title == item).ArtworkTypeCode;
                                            obj.RequestID = itemId;
                                            obj.RequestDate = (DateTime)detailSection.RequestDate;
                                            obj.RequestBy = detailSection.ProposedBy;
                                            absqTeamSection.ArtworkAttachmentTempList.Add(obj);
                                        }
                                    }
                                }
                                else
                                {
                                    absqTeamSection.ArtworkAttachmentTempList.ForEach(p =>
                                    {
                                        if (p.Files != null)
                                        {
                                            p.FileNameList = JsonConvert.SerializeObject(p.Files);
                                        }
                                    });
                                }
                                if (sectionDetails.ApproversList.Any(p => !string.IsNullOrEmpty(p.Approver) && (p.Role == ArtworkRoles.QALEVEL1APPROVER || p.Role == ArtworkRoles.QALEVEL2APPROVER || p.Role == ArtworkRoles.ABSQTEAM || p.Role == ArtworkRoles.ABSQAPPROVER) && p.Approver.Split(',').Contains(userID.Trim())))
                                {
                                    absqTeamSection.FNLQAPAttachment = absqTeamSection.Files != null && absqTeamSection.Files.Count > 0 ? JsonConvert.SerializeObject(absqTeamSection.Files.Where(x => !string.IsNullOrEmpty(absqTeamSection.FNLQAPAttachment) && absqTeamSection.FNLQAPAttachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                                }
                                else
                                {
                                    absqTeamSection.FNLQAPAttachment = string.Empty;
                                }
                                absqTeamSection.FNLTechnicalSpecificationAttachment = absqTeamSection.Files != null && absqTeamSection.Files.Count > 0 ? JsonConvert.SerializeObject(absqTeamSection.Files.Where(x => !string.IsNullOrEmpty(absqTeamSection.FNLTechnicalSpecificationAttachment) && absqTeamSection.FNLTechnicalSpecificationAttachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                            }

                            var qaLevel1Section = newArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.QALEVEL1SECTION)) as NewArtworkQALevel1Section;
                            if (qaLevel1Section != null)
                            {
                                qaLevel1Section.FNLQALevel1Attachment = qaLevel1Section.Files != null && qaLevel1Section.Files.Count > 0 ? JsonConvert.SerializeObject(qaLevel1Section.Files.Where(x => !string.IsNullOrEmpty(qaLevel1Section.FNLQALevel1Attachment) && qaLevel1Section.FNLQALevel1Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                            }

                            var qaLevel2Section = newArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.QALEVEL2SECTION)) as NewArtworkQALevel2Section;
                            if (qaLevel2Section != null)
                            {
                                qaLevel2Section.CC = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.CC).Approvers;
                                qaLevel2Section.CCName = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.CC).ApproversName;
                                qaLevel2Section.Legal = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.LEGAL).Approvers;
                                qaLevel2Section.LegalName = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.LEGAL).ApproversName;
                                qaLevel2Section.FNLQALevel2Attachment = qaLevel2Section.Files != null && qaLevel2Section.Files.Count > 0 ? JsonConvert.SerializeObject(qaLevel2Section.Files.Where(x => !string.IsNullOrEmpty(qaLevel2Section.FNLQALevel2Attachment) && qaLevel2Section.FNLQALevel2Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                            }

                            var scmLevel1Section = newArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.SCMLEVEL1SECTION)) as NewArtworkSCMLevel1Section;
                            if (scmLevel1Section != null)
                            {
                                scmLevel1Section.FNLSCMLevel1Attachment = scmLevel1Section.Files != null && scmLevel1Section.Files.Count > 0 ? JsonConvert.SerializeObject(scmLevel1Section.Files.Where(x => !string.IsNullOrEmpty(scmLevel1Section.FNLSCMLevel1Attachment) && scmLevel1Section.FNLSCMLevel1Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                            }

                            var scmLevel2Section = newArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.SCMLEVEL2SECTION)) as NewArtworkSCMLevel2Section;
                            if (scmLevel2Section != null)
                            {
                                scmLevel2Section.CC = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.CC).Approvers;
                                scmLevel2Section.CCName = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.CC).ApproversName;
                                scmLevel2Section.Legal = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.LEGAL).Approvers;
                                scmLevel2Section.LegalName = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.LEGAL).ApproversName;
                                scmLevel2Section.FNLSCMLevel2Attachment = scmLevel2Section.Files != null && scmLevel2Section.Files.Count > 0 ? JsonConvert.SerializeObject(scmLevel2Section.Files.Where(x => !string.IsNullOrEmpty(scmLevel2Section.FNLSCMLevel2Attachment) && scmLevel2Section.FNLSCMLevel2Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                            }

                            var abdLevel1Section = newArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.ADBLEVEL1SECTION)) as NewArtworkADBLevel1Section;
                            if (abdLevel1Section != null)
                            {
                                abdLevel1Section.FNLADBLevel1Attachment = abdLevel1Section.Files != null && abdLevel1Section.Files.Count > 0 ? JsonConvert.SerializeObject(abdLevel1Section.Files.Where(x => !string.IsNullOrEmpty(abdLevel1Section.FNLADBLevel1Attachment) && abdLevel1Section.FNLADBLevel1Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                            }

                            var abdLevel2Section = newArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.ADBLEVEL2SECTION)) as NewArtworkADBLevel2Section;
                            if (abdLevel2Section != null)
                            {
                                abdLevel2Section.CC = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.CC).Approvers;
                                abdLevel2Section.CCName = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.CC).ApproversName;
                                abdLevel2Section.Legal = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.LEGAL).Approvers;
                                abdLevel2Section.LegalName = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.LEGAL).ApproversName;
                                abdLevel2Section.FNLADBLevel2Attachment = abdLevel2Section.Files != null && abdLevel2Section.Files.Count > 0 ? JsonConvert.SerializeObject(abdLevel2Section.Files.Where(x => !string.IsNullOrEmpty(abdLevel2Section.FNLADBLevel2Attachment) && abdLevel2Section.FNLADBLevel2Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                            }

                            var mktLevel1Section = newArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.MARKETINGLEVEL1SECTION)) as NewArtworkMarketingLevel1Section;
                            if (mktLevel1Section != null)
                            {
                                mktLevel1Section.FNLMarketingLevel1Attachment = mktLevel1Section.Files != null && mktLevel1Section.Files.Count > 0 ? JsonConvert.SerializeObject(mktLevel1Section.Files.Where(x => !string.IsNullOrEmpty(mktLevel1Section.FNLMarketingLevel1Attachment) && mktLevel1Section.FNLMarketingLevel1Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                            }

                            var mktLevel2Section = newArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.MARKETINGLEVEL2SECTION)) as NewArtworkMarketingLevel2Section;
                            if (mktLevel2Section != null)
                            {
                                mktLevel2Section.CC = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.CC).Approvers;
                                mktLevel2Section.CCName = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.CC).ApproversName;
                                mktLevel2Section.Legal = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.LEGAL).Approvers;
                                mktLevel2Section.LegalName = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.LEGAL).ApproversName;
                                mktLevel2Section.FNLMarketingLevel2Attachment = mktLevel2Section.Files != null && mktLevel2Section.Files.Count > 0 ? JsonConvert.SerializeObject(mktLevel2Section.Files.Where(x => !string.IsNullOrEmpty(mktLevel2Section.FNLMarketingLevel2Attachment) && mktLevel2Section.FNLMarketingLevel2Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                            }
                        }
                        ////sectionDetails.FileNameList = sectionDetails.Files != null && sectionDetails.Files.Count > 0 ? JsonConvert.SerializeObject(sectionDetails.Files) : string.Empty;
                        ////if (sectionDetails.Status == FormStatus.COMPLETED || sectionDetails.Status == FormStatus.REJECTED)
                        ////{
                        ////    if (newArtworkForm.Buttons.FirstOrDefault(p => p.Name == "Print") != null)
                        ////    {
                        ////        newArtworkForm.Buttons.Remove(newArtworkForm.Buttons.FirstOrDefault(f => f.ButtonStatus == ButtonActionStatus.Print));
                        ////    }
                        ////}
                    }
                    contract.Forms.Add(newArtworkForm);
                }
            }
            return contract;
        }

        /// <summary>
        /// Gets all vendor code.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <returns>the string</returns>
        public string GetAllVendorCode(string title)
        {
            IMaster vendormaster = this.SearchVendorMasterData(title);
            return JSONHelper.ToJSON<IMaster>(vendormaster);
        }

        /// <summary>
        /// Searches the vendor master data.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <returns>Vendor Master</returns>
        public VendorMaster SearchVendorMasterData(string title)
        {
            try
            {
                VendorMaster searchMaster = new VendorMaster();
                searchMaster.Items = new List<IMasterItem>();
                List spList = this.web.Lists.GetByTitle(ArtworkMasters.VENDORMASTERLIST);
                CamlQuery query = new CamlQuery();

                query.ViewXml = @"<View>
                                    <Query>
                                        <Where><Or>
                                            <Contains>
                                                <FieldRef Name='VendorCode' />
                                                <Value Type='Text'>" + title + @"</Value>
                                            </Contains>
                                             <Contains>
                                                <FieldRef Name='Title' />
                                                <Value Type='Text'>" + title + @"</Value>
                                            </Contains>
                                         </Or>
                                        </Where>
                                    </Query> <ViewFields><FieldRef Name='VendorCode' /><FieldRef Name='Title' /></ViewFields><RowLimit>15</RowLimit>
                                </View>";
                ListItemCollection items = spList.GetItems(query);
                this.context.Load(items);
                this.context.ExecuteQuery();
                if (items != null && items.Count != 0)
                {
                    VendorMasterListItem obj;
                    foreach (ListItem item in items)
                    {
                        obj = new VendorMasterListItem();
                        obj.VendorName = Convert.ToString(item["Title"]);
                        obj.VendorCode = Convert.ToString(item["VendorCode"]);
                        searchMaster.Items.Add(obj);
                    }
                    //// return JSONHelper.ToJSON<ItemMasterListItem>(obj);
                    return searchMaster;
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }

        #endregion

        #region "SAVE DATA"
        /// <summary>
        /// Saves the by section.
        /// </summary>
        /// <param name="sections">The sections.</param>
        /// <param name="objDict">The object dictionary.</param>
        /// <returns>return status</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Already validate"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "artworkDetailSection", Justification = "Already vaildated")]
        public ActionStatus SaveBySection(ISection sectionDetails, Dictionary<string, string> objDict)
        {
            lock (Padlock)
            {
                ActionStatus status = new ActionStatus();
                NewArtworkNoCount currentNewArtworkNo = null;
                if (sectionDetails != null && objDict != null)
                {
                    objDict[Parameter.ACTIVITYLOG] = ArtworkListNames.NEWARTWORKACTIVITYLOG;
                    objDict[Parameter.APPLICATIONNAME] = ArtworkApplicationNameConstants.ARTWORKAPP;
                    objDict[Parameter.FROMNAME] = ArtworkFormNames.NEWARTWORKFORM;
                    NewArtworkDetailSection section = null;
                    if (sectionDetails.SectionName == ArtworkSectionName.ARTWORKDETAILSECTION)
                    {
                        section = sectionDetails as NewArtworkDetailSection;
                        if (section.ListDetails != null && section.ListDetails.Count != 0)
                        {
                            if (this.IsItemCodeExists(section.ItemCode.Trim(), Convert.ToInt32(section.ListDetails[0].ItemId)))
                            {
                                status.IsSucceed = false;
                                status.Messages.Add("Text_DuplicateItemCode");
                                return status;
                            }
                        }
                        if (section.BusinessUnit == ArtworkCommonConstant.EXPBUSINESSUNITCODE)
                        {
                            if (!section.ItemCode.EndsWith("E"))
                            {
                                section.ItemCode = section.ItemCode + "E";
                            }
                        }
                        if (sectionDetails.ActionStatus == ButtonActionStatus.NextApproval && string.IsNullOrEmpty(section.ReferenceNo))
                        {
                            section.RequestDate = DateTime.Now;
                            currentNewArtworkNo = this.GetNewArtworkNo(section.BusinessUnit);
                            if (currentNewArtworkNo != null)
                            {
                                currentNewArtworkNo.CurrentValue = currentNewArtworkNo.CurrentValue + 1;
                                Logger.Info("New Artwork Current Value + 1 = " + currentNewArtworkNo.CurrentValue);
                                ////section.ReferenceNo = string.Format("{0}-{1}-{2}-{3}-{4}", ArtworkCommonConstant.REFERENCENOPREFIX, ArtworkCommonConstant.NEWARTWORKREQUESTABBREVIATION, section.BusinessUnit, section.RequestDate.Value.ToString("yyyy"), string.Format("{0:00000}", currentNewArtworkNo.CurrentValue));
                                section.ReferenceNo = string.Format("{0}-{1}-{2}-{3}", section.BusinessUnit, ArtworkCommonConstant.NEWARTWORKREQUESTABBREVIATION, section.RequestDate.Value.ToString("yyyy"), string.Format("{0:00000}", currentNewArtworkNo.CurrentValue));
                                Logger.Info("New Artwork No is " + section.ReferenceNo);
                                status.ExtraData = section.ReferenceNo;
                            }
                        }
                    }
                    List<ListItemDetail> objSaveDetails = BELDataAccessLayer.Instance.SaveData(this.context, this.web, sectionDetails, objDict);
                    ListItemDetail itemDetails = objSaveDetails.Where(p => p.ListName.Equals(ArtworkListNames.NEWARTWORKLIST)).FirstOrDefault<ListItemDetail>();
                    if (sectionDetails.SectionName == ArtworkSectionName.ARTWORKDETAILSECTION)
                    {
                        if (!string.IsNullOrEmpty(section.OldReferenceNo))
                        {
                            Dictionary<string, dynamic> values = new Dictionary<string, dynamic>();
                            values.Add("IsArtworkRetrieved", true);
                            BELDataAccessLayer.Instance.SaveFormFields(this.context, this.web, ArtworkListNames.NEWARTWORKLIST, section.OldRequestId, values);
                        }

                        if (itemDetails.ItemId > 0 && currentNewArtworkNo != null)
                        {
                            this.UpdateNewArtworkNoCount(currentNewArtworkNo);
                            Logger.Info("Update Reference No " + section.ReferenceNo);
                        }
                    }

                    if (itemDetails.ItemId > 0)
                    {
                        status.IsSucceed = true;
                        if (sectionDetails.ActionStatus == ButtonActionStatus.Complete)
                        {
                            Dictionary<string, string> objContract = new Dictionary<string, string>();
                            objContract.Add(Parameter.FROMNAME, ArtworkFormNames.NEWARTWORKFORM);
                            objContract.Add(Parameter.ITEMID, Convert.ToString(sectionDetails.ListDetails[0].ItemId));
                            objContract.Add(Parameter.USEREID, objDict["UserID"]);
                            objContract.Add("UserName", objDict["UserName"]);
                            AsyncHelper.Call(obj => { BuildItemMasterData(objContract); });
                        }
                        switch (sectionDetails.ButtonCaption.Trim())
                        {
                            case ButtonCaption.SAVEASDRAFT:
                                status.Messages.Add("Text_SaveDraftSuccess");
                                break;
                            case ButtonCaption.SUBMIT:
                                status.Messages.Add(ApplicationConstants.SUCCESSMESSAGE);
                                break;
                            case ButtonCaption.APPROVE:
                                if (sectionDetails.ActionStatus == ButtonActionStatus.Complete)
                                {
                                    status.Messages.Add("Text_CompletedSuccess");
                                }
                                else
                                {
                                    status.Messages.Add("Text_ApproveSuccess");
                                }
                                break;
                            case ButtonCaption.REWORK:
                                status.Messages.Add("Text_ReworkSuccess");
                                break;
                            case ButtonCaption.REJECT:
                                status.Messages.Add("Text_RejectedSuccess");
                                break;
                            case ButtonCaption.HOLD:
                                status.Messages.Add("Text_HoldSuccess");
                                status.ItemID = itemDetails.ItemId;
                                break;
                            case ButtonCaption.RESUME:
                                status.Messages.Add("Text_ResumeSuccess");
                                status.ItemID = itemDetails.ItemId;
                                break;
                            default:
                                status.Messages.Add(ApplicationConstants.SUCCESSMESSAGE);
                                break;
                        }
                    }
                    else
                    {
                        status.IsSucceed = false;
                        status.Messages.Add(ApplicationConstants.ERRORMESSAGE);
                    }
                }
                return status;
            }
        }

        /// <summary>
        /// Get New Artwork No Logic
        /// </summary>
        /// <param name="businessunit">Business Unit</param>
        /// <returns>NPD No Count</returns>
        public NewArtworkNoCount GetNewArtworkNo(string businessunit)
        {
            try
            {
                List<NewArtworkNoCount> lstNPDCount = new List<NewArtworkNoCount>();
                List spList = this.web.Lists.GetByTitle(ArtworkListNames.NEWARTWORKNOCOUNT);
                CamlQuery query = new CamlQuery();
                query.ViewXml = @"<View><ViewFields><FieldRef Name='Title' /><FieldRef Name='Year' /><FieldRef Name='CurrentValue' /></ViewFields></View>";
                ListItemCollection items = spList.GetItems(query);
                this.context.Load(items);
                this.context.ExecuteQuery();
                if (items != null && items.Count != 0)
                {
                    foreach (ListItem item in items)
                    {
                        NewArtworkNoCount obj = new NewArtworkNoCount();
                        obj.ID = item.Id;
                        obj.BusinessUnit = Convert.ToString(item["Title"]);
                        obj.Year = Convert.ToInt32(item["Year"]);
                        obj.CurrentValue = Convert.ToInt32(item["CurrentValue"]);
                        if (obj.Year != DateTime.Today.Year)
                        {
                            obj.Year = DateTime.Today.Year;
                            obj.CurrentValue = 0;
                        }

                        lstNPDCount.Add(obj);
                    }
                }

                if (lstNPDCount != null)
                {
                    return lstNPDCount.FirstOrDefault(p => businessunit.Equals(p.BusinessUnit) && p.Year == DateTime.Today.Year);
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }

        /// <summary>
        /// Update New Artwork No Count
        /// </summary>
        /// <param name="currentValue">Current Value</param>
        public void UpdateNewArtworkNoCount(NewArtworkNoCount currentValue)
        {
            if (currentValue != null && currentValue.ID != 0)
            {
                try
                {
                    Logger.Info("Aync update New Artwork Current value currentValue : " + currentValue.CurrentValue + " BusinessUnit : " + currentValue.BusinessUnit);
                    List spList = this.web.Lists.GetByTitle(ArtworkListNames.NEWARTWORKNOCOUNT);
                    ListItem item = spList.GetItemById(currentValue.ID);
                    item["CurrentValue"] = currentValue.CurrentValue;
                    item["Year"] = currentValue.Year;
                    item.Update();
                    ////context.Load(item);
                    this.context.ExecuteQuery();
                }
                catch (Exception ex)
                {
                    Logger.Error("Error while Update New Artwork no Current Value");
                    Logger.Error(ex);
                }
            }
        }

        /// <summary>
        /// Check Item Id exist or not
        /// </summary>
        /// <param name="itemCode">item Code</param>
        /// <param name="itemId">item Id</param>
        /// <returns>New Artwork No Count</returns>
        public bool IsItemCodeExists(string itemCode, int itemId)
        {
            try
            {
                bool isValid = false;
                List spList = this.web.Lists.GetByTitle(ArtworkListNames.NEWARTWORKLIST);
                CamlQuery query = new CamlQuery();
                query.ViewXml = @"<View><Query><Where><And><Neq><FieldRef Name='ID'/><Value Type='Number'>" + itemId + "</Value></Neq><And><Eq><FieldRef Name='ItemCode'/><Value Type='Text'>" + itemCode + "</Value></Eq><Neq><FieldRef Name='WorkflowStatus'/><Value Type='Text'>Rejected</Value></Neq></And></And></Where></Query></View>";
                ListItemCollection items = spList.GetItems(query);
                this.context.Load(items);
                List itemMasterList = this.web.Lists.GetByTitle(ArtworkMasters.ITEMMASTER);
                CamlQuery itemMasterQuery = new CamlQuery();
                itemMasterQuery.ViewXml = @"<View><Query><Where><Eq><FieldRef Name='ItemCode'/><Value Type='Text'>" + itemCode + "</Value></Eq></Where></Query></View>";
                ListItemCollection itemMasterItems = itemMasterList.GetItems(itemMasterQuery);
                this.context.Load(itemMasterItems);
                this.context.ExecuteQuery();
                if ((items != null && items.Count != 0) || (itemMasterItems != null && itemMasterItems.Count != 0))
                {
                    isValid = true;
                }

                ////if (!isValid)
                ////{
                ////    if (items != null && items.Count != 0)
                ////    {
                ////        isValid = true;
                ////    }
                ////}
                return isValid;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return true;
            }
        }

        /// <summary>
        /// The Build Item Master Data
        /// </summary>
        /// <param name="objContract">The Contract object</param>
        public void BuildItemMasterData(Dictionary<string, string> objContract)
        {
            try
            {
                NewArtworkContract contract = null;
                contract = this.GetNewArtworkDetails(objContract);
                if (contract != null && contract.Forms != null && contract.Forms.Count > 0)
                {
                    NewArtworkDetailSection artworkDetailSection = contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ARTWORKDETAILSECTION) as NewArtworkDetailSection;
                    ItemMasterListItem objItemMaster = new ItemMasterListItem();
                    objItemMaster.ItemCode = artworkDetailSection.ItemCode;
                    objItemMaster.BusinessUnit = artworkDetailSection.BusinessUnit;
                    objItemMaster.ModelName = artworkDetailSection.ModelName;
                    objItemMaster.ProductCategory = artworkDetailSection.ProductCategory;
                    objItemMaster.Color = artworkDetailSection.Color;
                    objItemMaster.BarCode = artworkDetailSection.BarCode;
                    objItemMaster.MasterCartondimensionL = artworkDetailSection.MasterCartondimensionL;
                    objItemMaster.MasterCartondimensionW = artworkDetailSection.MasterCartondimensionW;
                    objItemMaster.MasterCartondimensionH = artworkDetailSection.MasterCartondimensionH;
                    objItemMaster.UnitCartonDimensionL = artworkDetailSection.UnitCartonDimensionL;
                    objItemMaster.UnitCartonDimensionW = artworkDetailSection.UnitCartonDimensionW;
                    objItemMaster.UnitCartonDimensionH = artworkDetailSection.UnitCartonDimensionH;
                    objItemMaster.MRP = artworkDetailSection.MRP;
                    objItemMaster.NoofCartoninMaster = artworkDetailSection.NoofCartoninMaster;
                    objItemMaster.Voltage = artworkDetailSection.Voltage;
                    objItemMaster.Power = artworkDetailSection.Power;
                    objItemMaster.NetWeight = artworkDetailSection.NetWeight;
                    objItemMaster.GrossWeight = artworkDetailSection.GrossWeight;
                    objItemMaster.Warranty = artworkDetailSection.Warranty;
                    objItemMaster.Product = artworkDetailSection.Product;
                    objItemMaster.VendorAddress = artworkDetailSection.VendorAddress;
                    objItemMaster.VendorCode = artworkDetailSection.VendorCode;
                    objItemMaster.Warranty = artworkDetailSection.Warranty;
                    objItemMaster.IsNewArtwork = true;
                    objItemMaster.ReferenceNo = artworkDetailSection.ReferenceNo;

                    NewArtworkABSQTeamSection artworkABSQTeamSection = contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ABSQTEAMSECTION) as NewArtworkABSQTeamSection;
                    List<NewArtworkAttachment> artworkfiles = artworkABSQTeamSection.ArtworkAttachmentList.ConvertAll(o => (o as NewArtworkAttachment));
                    artworkfiles.ForEach(p =>
                    {
                        if (p.FileNameList != null)
                        {
                            p.ItemAction = ItemActionStatus.NEW;
                            p.ItemCode = artworkDetailSection.ItemCode;
                        }
                    });
                    this.SaveItemArtworkTypeMasterData(objItemMaster, artworkfiles, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error While Existing Build Item Master Data");
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Save Item Artwork Type Master Data
        /// </summary>
        /// <param name="itemMaster">The Item Master</param>
        /// <param name="artworkfiles">The Artwork Files</param>
        /// <param name="isNew">Is new or not</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Already Validate"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "artworkDetailSection", Justification = "Already vaildated")]
        public void SaveItemArtworkTypeMasterData(ItemMasterListItem itemMaster, List<NewArtworkAttachment> artworkfiles, bool isNew)
        {
            try
            {
                List spList = this.web.Lists.GetByTitle(ArtworkMasters.ITEMMASTER);
                List spArtworkTypeMappingMaster = this.web.Lists.GetByTitle(ArtworkMasters.ITEMMASTERARTWORKTYPEMAPPINGMASTER);
                ListItemCollection lstArtworkTypeMApping = spArtworkTypeMappingMaster.GetItems(CamlQuery.CreateAllItemsQuery());
                ListItem oItemListItem = null;
                if (isNew)
                {
                    ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                    oItemListItem = spList.AddItem(itemCreateInfo);
                }
                else
                {
                    if (itemMaster != null)
                    {
                        oItemListItem = spList.GetItemById(itemMaster.ID);
                    }
                }
                if (itemMaster != null)
                {
                    oItemListItem["ItemCode"] = itemMaster.ItemCode;
                    oItemListItem["Model"] = itemMaster.ModelName;
                    oItemListItem["BarCode"] = itemMaster.BarCode;
                    oItemListItem["Color"] = itemMaster.Color;
                    oItemListItem["BusinessUnit"] = itemMaster.BusinessUnit;
                    oItemListItem["MasterCartondimensionL"] = itemMaster.MasterCartondimensionL;
                    oItemListItem["MasterCartondimensionW"] = itemMaster.MasterCartondimensionW;
                    oItemListItem["MasterCartondimensionH"] = itemMaster.MasterCartondimensionH;
                    oItemListItem["UnitCartonDimensionL"] = itemMaster.UnitCartonDimensionL;
                    oItemListItem["UnitCartonDimensionW"] = itemMaster.UnitCartonDimensionW;
                    oItemListItem["UnitCartonDimensionH"] = itemMaster.UnitCartonDimensionH;
                    oItemListItem["MRP"] = itemMaster.MRP;
                    oItemListItem["ProductCategory"] = itemMaster.ProductCategory;
                    oItemListItem["ReferenceNo"] = itemMaster.ReferenceNo;
                    oItemListItem["Title"] = itemMaster.Title;
                    oItemListItem["Product"] = itemMaster.Product;
                    oItemListItem["VendorAddress"] = itemMaster.VendorAddress;
                    oItemListItem["VendorCode"] = itemMaster.VendorCode;
                    oItemListItem["Warranty"] = itemMaster.Warranty;
                    oItemListItem["LockingDate"] = DateTime.Now.AddDays(Convert.ToInt32(BELDataAccessLayer.Instance.GetConfigVariable("LockingDays")));
                    oItemListItem["NoofCartoninMaster"] = itemMaster.NoofCartoninMaster;
                    oItemListItem["IsNewArtwork"] = itemMaster.IsNewArtwork;
                    oItemListItem.Update();
                    this.context.Load(lstArtworkTypeMApping);
                    this.context.ExecuteQuery();
                }
                foreach (NewArtworkAttachment item in artworkfiles)
                {
                    ListItem oArtworkTypeMappingItem = null;
                    ListItem oArtworkTypeMappingItemUpdate = null;
                    oArtworkTypeMappingItem = lstArtworkTypeMApping.FirstOrDefault(x => Convert.ToString(x.FieldValues["ItemCode"]) == item.ItemCode && Convert.ToString(x.FieldValues["Title"]) == item.ArtworkType);
                    if (oArtworkTypeMappingItem != null)
                    {
                        oArtworkTypeMappingItemUpdate = spArtworkTypeMappingMaster.GetItemById(oArtworkTypeMappingItem.Id);
                    }
                    else
                    {
                        ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                        oArtworkTypeMappingItemUpdate = spArtworkTypeMappingMaster.AddItem(itemCreateInfo);
                    }
                    oArtworkTypeMappingItemUpdate["FileName"] = item.Files != null && item.Files.Count != 0 ? item.Files[0].FileName : string.Empty;
                    ////oArtworkTypeMappingItemUpdate["FileNameList"] = item.FileNameList;
                    oArtworkTypeMappingItemUpdate["ItemCode"] = item.ItemCode;
                    oArtworkTypeMappingItemUpdate["CreatedDate"] = item.RequestDate;
                    oArtworkTypeMappingItemUpdate["RequestDate"] = item.RequestDate;
                    oArtworkTypeMappingItemUpdate["RequestID"] = oItemListItem.Id;
                    oArtworkTypeMappingItemUpdate["RequestBy"] = item.RequestBy;
                    oArtworkTypeMappingItemUpdate["Title"] = item.ArtworkType;
                    oArtworkTypeMappingItemUpdate["ArtworkTypeCode"] = item.ArtworkTypeCode;
                    oArtworkTypeMappingItemUpdate["ArtworkType"] = item.ArtworkType;
                    oArtworkTypeMappingItemUpdate.Update();
                    this.context.ExecuteQuery();
                    using (MemoryStream mStream = new MemoryStream(CommonBusinessLayer.Instance.DownloadFile(item.Files[0].FileURL, "Artworks")))
                    {
                        AttachmentCreationInformation aci = new AttachmentCreationInformation();
                        aci.ContentStream = mStream;
                        aci.FileName = item.Files[0].FileName;
                        oArtworkTypeMappingItemUpdate.AttachmentFiles.Add(aci);
                        this.context.ExecuteQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error While Save Item Master Data and Artwork Type Item Code Mapping Data");
                Logger.Error(ex);
            }
        }

        #endregion

        #region "GET Admin DATA"
        /// <summary>
        /// Gets the Artwork details.
        /// </summary>
        /// <param name="objDict">The object dictionary.</param>
        /// <returns>byte array</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "test")]
        public NewArtworkContract GetNewArtworkAdminDetails(IDictionary<string, string> objDict)
        {
            NewArtworkContract contract = new NewArtworkContract();
            if (objDict != null && objDict.ContainsKey(Parameter.FROMNAME) && objDict.ContainsKey(Parameter.ITEMID) && objDict.ContainsKey(Parameter.USEREID))
            {
                string formName = objDict[Parameter.FROMNAME];
                int itemId = Convert.ToInt32(objDict[Parameter.ITEMID]);
                string userID = objDict[Parameter.USEREID];
                IForm newartAdminForm = new NewArtworkAdminForm(true);
                newartAdminForm = BELDataAccessLayer.Instance.GetFormData(this.context, this.web, ArtworkApplicationNameConstants.ARTWORKAPP, formName, itemId, userID, newartAdminForm);
                if (newartAdminForm != null && newartAdminForm.SectionsList != null && newartAdminForm.SectionsList.Count > 0)
                {
                    var sectionDetails = newartAdminForm.SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ARTWORKADMINDETAILSECTION) as NewArtworkAdminDetailSection;
                    sectionDetails.IsActive = true;
                    if (sectionDetails != null)
                    {
                        if (itemId == 0)
                        {
                        }
                        else
                        {
                            sectionDetails.ApproversList.Remove(sectionDetails.ApproversList.FirstOrDefault(p => p.Role == UserRoles.VIEWER));
                        }
                        if (sectionDetails != null)
                        {
                            sectionDetails.FNLSamplePhoto = sectionDetails.Files != null && sectionDetails.Files.Count > 0 ? JsonConvert.SerializeObject(sectionDetails.Files.Where(x => !string.IsNullOrEmpty(sectionDetails.FNLSamplePhoto) && sectionDetails.FNLSamplePhoto.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                            if (sectionDetails.FNLSamplePhoto == "[]")
                            {
                                sectionDetails.FNLSamplePhoto = null;
                            }
                        }
                        sectionDetails.FileNameList = sectionDetails.Files != null && sectionDetails.Files.Count > 0 ? JsonConvert.SerializeObject(sectionDetails.Files) : string.Empty;
                        //// var absqTeamSection = newartAdminForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.ABSQTEAMSECTION)) as NewArtworkABSQTeamSection;
                        if (sectionDetails != null)
                        {
                            sectionDetails.ArtworkAttachmentTempList = new List<NewArtworkAttachment>();
                            sectionDetails.ArtworkAttachmentTempList.AddRange(sectionDetails.ArtworkAttachmentList.ConvertAll(o => (o as NewArtworkAttachment)));
                            if (sectionDetails.ArtworkAttachmentTempList.Count == 0 && sectionDetails.IsActive)
                            {
                                string strArtworkType = sectionDetails.ArtworkType;
                                List<IMasterItem> artworkTypeList = sectionDetails.MasterData.FirstOrDefault(p => p.NameOfMaster == ArtworkMasters.ARTWORKTYEMASTER).Items;
                                List<ArtworkTypeMasterListItem> lstDefaultSelectedArtworkType = artworkTypeList.ConvertAll(p => (ArtworkTypeMasterListItem)p);
                                if (!string.IsNullOrWhiteSpace(strArtworkType))
                                {
                                    string[] artworkTypeArray = strArtworkType.Split(',');
                                    foreach (string item in artworkTypeArray)
                                    {
                                        NewArtworkAttachment obj = new NewArtworkAttachment();
                                        obj.ArtworkType = item;
                                        obj.ArtworkTypeCode = lstDefaultSelectedArtworkType.FirstOrDefault(x => x.Title == item).ArtworkTypeCode;
                                        obj.RequestID = itemId;
                                        obj.RequestDate = (DateTime)sectionDetails.RequestDate;
                                        obj.RequestBy = sectionDetails.ProposedBy;
                                        sectionDetails.ArtworkAttachmentTempList.Add(obj);
                                    }
                                }
                            }
                            else
                            {
                                sectionDetails.ArtworkAttachmentTempList.ForEach(p =>
                                {
                                    if (p.Files != null)
                                    {
                                        p.FileNameList = JsonConvert.SerializeObject(p.Files);
                                    }
                                });
                            }
                            if (sectionDetails.ApproversList.Any(p => !string.IsNullOrEmpty(p.Approver) && (p.Role == ArtworkRoles.QALEVEL1APPROVER || p.Role == ArtworkRoles.QALEVEL2APPROVER || p.Role == ArtworkRoles.ABSQTEAM || p.Role == ArtworkRoles.ABSQAPPROVER) && p.Approver.Split(',').Contains(userID.Trim())))
                            {
                                sectionDetails.FNLQAPAttachment = sectionDetails.Files != null && sectionDetails.Files.Count > 0 ? JsonConvert.SerializeObject(sectionDetails.Files.Where(x => !string.IsNullOrEmpty(sectionDetails.FNLQAPAttachment) && sectionDetails.FNLQAPAttachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                            }
                            else
                            {
                                sectionDetails.FNLQAPAttachment = string.Empty;
                            }
                            sectionDetails.FNLTechnicalSpecificationAttachment = sectionDetails.Files != null && sectionDetails.Files.Count > 0 ? JsonConvert.SerializeObject(sectionDetails.Files.Where(x => !string.IsNullOrEmpty(sectionDetails.FNLTechnicalSpecificationAttachment) && sectionDetails.FNLTechnicalSpecificationAttachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                        }
                        sectionDetails.FNLQALevel1Attachment = sectionDetails.Files != null && sectionDetails.Files.Count > 0 ? JsonConvert.SerializeObject(sectionDetails.Files.Where(x => !string.IsNullOrEmpty(sectionDetails.FNLQALevel1Attachment) && sectionDetails.FNLQALevel1Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                        sectionDetails.FNLQALevel2Attachment = sectionDetails.Files != null && sectionDetails.Files.Count > 0 ? JsonConvert.SerializeObject(sectionDetails.Files.Where(x => !string.IsNullOrEmpty(sectionDetails.FNLQALevel2Attachment) && sectionDetails.FNLQALevel2Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                        sectionDetails.FNLSCMLevel1Attachment = sectionDetails.Files != null && sectionDetails.Files.Count > 0 ? JsonConvert.SerializeObject(sectionDetails.Files.Where(x => !string.IsNullOrEmpty(sectionDetails.FNLSCMLevel1Attachment) && sectionDetails.FNLSCMLevel1Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                        sectionDetails.FNLSCMLevel2Attachment = sectionDetails.Files != null && sectionDetails.Files.Count > 0 ? JsonConvert.SerializeObject(sectionDetails.Files.Where(x => !string.IsNullOrEmpty(sectionDetails.FNLSCMLevel2Attachment) && sectionDetails.FNLSCMLevel2Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                        sectionDetails.FNLADBLevel1Attachment = sectionDetails.Files != null && sectionDetails.Files.Count > 0 ? JsonConvert.SerializeObject(sectionDetails.Files.Where(x => !string.IsNullOrEmpty(sectionDetails.FNLADBLevel1Attachment) && sectionDetails.FNLADBLevel1Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                        sectionDetails.FNLADBLevel2Attachment = sectionDetails.Files != null && sectionDetails.Files.Count > 0 ? JsonConvert.SerializeObject(sectionDetails.Files.Where(x => !string.IsNullOrEmpty(sectionDetails.FNLADBLevel2Attachment) && sectionDetails.FNLADBLevel2Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                        sectionDetails.FNLMarketingLevel1Attachment = sectionDetails.Files != null && sectionDetails.Files.Count > 0 ? JsonConvert.SerializeObject(sectionDetails.Files.Where(x => !string.IsNullOrEmpty(sectionDetails.FNLMarketingLevel1Attachment) && sectionDetails.FNLMarketingLevel1Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                        sectionDetails.FNLMarketingLevel2Attachment = sectionDetails.Files != null && sectionDetails.Files.Count > 0 ? JsonConvert.SerializeObject(sectionDetails.Files.Where(x => !string.IsNullOrEmpty(sectionDetails.FNLMarketingLevel2Attachment) && sectionDetails.FNLMarketingLevel2Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                        ////var scmLevel1Section = newartAdminForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.SCMLEVEL1SECTION)) as NewArtworkSCMLevel1Section;
                        ////if (scmLevel1Section != null)
                        ////{
                        ////    scmLevel1Section.FNLSCMLevel1Attachment = scmLevel1Section.Files != null && scmLevel1Section.Files.Count > 0 ? JsonConvert.SerializeObject(scmLevel1Section.Files.Where(x => !string.IsNullOrEmpty(scmLevel1Section.FNLSCMLevel1Attachment) && scmLevel1Section.FNLSCMLevel1Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                        ////}
                        ////var scmLevel2Section = newartAdminForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.SCMLEVEL2SECTION)) as NewArtworkSCMLevel2Section;
                        ////if (scmLevel2Section != null)
                        ////{
                        ////    scmLevel2Section.FNLSCMLevel2Attachment = scmLevel2Section.Files != null && scmLevel2Section.Files.Count > 0 ? JsonConvert.SerializeObject(scmLevel2Section.Files.Where(x => !string.IsNullOrEmpty(scmLevel2Section.FNLSCMLevel2Attachment) && scmLevel2Section.FNLSCMLevel2Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                        ////}

                        ////var abdLevel1Section = newartAdminForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.ADBLEVEL1SECTION)) as NewArtworkADBLevel1Section;
                        ////if (abdLevel1Section != null)
                        ////{
                        ////    abdLevel1Section.FNLADBLevel1Attachment = abdLevel1Section.Files != null && abdLevel1Section.Files.Count > 0 ? JsonConvert.SerializeObject(abdLevel1Section.Files.Where(x => !string.IsNullOrEmpty(abdLevel1Section.FNLADBLevel1Attachment) && abdLevel1Section.FNLADBLevel1Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                        ////}

                        ////var abdLevel2Section = newartAdminForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.ADBLEVEL2SECTION)) as NewArtworkADBLevel2Section;
                        ////if (abdLevel2Section != null)
                        ////{
                        ////    abdLevel2Section.FNLADBLevel2Attachment = abdLevel2Section.Files != null && abdLevel2Section.Files.Count > 0 ? JsonConvert.SerializeObject(abdLevel2Section.Files.Where(x => !string.IsNullOrEmpty(abdLevel2Section.FNLADBLevel2Attachment) && abdLevel2Section.FNLADBLevel2Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                        ////}

                        ////var mktLevel1Section = newartAdminForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.MARKETINGLEVEL1SECTION)) as NewArtworkMarketingLevel1Section;
                        ////if (mktLevel1Section != null)
                        ////{
                        ////    mktLevel1Section.FNLMarketingLevel1Attachment = mktLevel1Section.Files != null && mktLevel1Section.Files.Count > 0 ? JsonConvert.SerializeObject(mktLevel1Section.Files.Where(x => !string.IsNullOrEmpty(mktLevel1Section.FNLMarketingLevel1Attachment) && mktLevel1Section.FNLMarketingLevel1Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                        ////}

                        ////var mktLevel2Section = newartAdminForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.MARKETINGLEVEL2SECTION)) as NewArtworkMarketingLevel2Section;
                        ////if (mktLevel2Section != null)
                        ////{
                        ////    mktLevel2Section.FNLMarketingLevel2Attachment = mktLevel2Section.Files != null && mktLevel2Section.Files.Count > 0 ? JsonConvert.SerializeObject(mktLevel2Section.Files.Where(x => !string.IsNullOrEmpty(mktLevel2Section.FNLMarketingLevel2Attachment) && mktLevel2Section.FNLMarketingLevel2Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                        ////}
                    }
                    contract.Forms.Add(newartAdminForm);
                }
            }
            return contract;
        }
        #endregion

        #region "SAVE Admin DATA"
        /// <summary>
        /// Saves the by section.
        /// </summary>
        /// <param name="sections">The sections.</param>
        /// <param name="objDict">The object dictionary.</param>
        /// <returns>return status</returns>
        public ActionStatus SaveNewArtworkAdminDataBySection(ISection sectionDetails, Dictionary<string, string> objDict)
        {
            lock (Padlock)
            {
                ActionStatus status = new ActionStatus();

                ////BELDataAccessLayer helper = new BELDataAccessLayer();
                if (sectionDetails != null && objDict != null)
                {
                    objDict[Parameter.ACTIVITYLOG] = ArtworkListNames.NEWARTWORKACTIVITYLOG;
                    objDict[Parameter.APPLICATIONNAME] = ArtworkApplicationNameConstants.ARTWORKAPP;
                    objDict[Parameter.FROMNAME] = ArtworkFormNames.NEWARTWORKADMINFORM;
                    ////NPDAdminDetailSection section = null;
                    ////if (sectionDetails.SectionName == NPDSectionName.NPDADMINSECTION)
                    ////{
                    ////    section = sectionDetails as NPDAdminDetailSection;
                    ////}
                    List<ListItemDetail> objSaveDetails = BELDataAccessLayer.Instance.SaveData(this.context, this.web, sectionDetails, objDict);
                    ListItemDetail itemDetails = objSaveDetails.Where(p => p.ListName.Equals(ArtworkListNames.NEWARTWORKLIST)).FirstOrDefault<ListItemDetail>();
                    if (sectionDetails.SectionName == ArtworkSectionName.ARTWORKADMINDETAILSECTION)
                    {
                        ////if (!string.IsNullOrEmpty(section.OldDCRNo))
                        ////{
                        ////    Dictionary<string, dynamic> values = new Dictionary<string, dynamic>();
                        ////    values.Add("IsDCRRetrieved", true);
                        ////    BELDataAccessLayer.Instance.SaveFormFields(this.context, this.web, DCRDCNListNames.DCRLIST, section.OldDCRId, values);
                        ////}
                    }

                    if (itemDetails.ItemId > 0)
                    {
                        status.IsSucceed = true;
                        switch (sectionDetails.ActionStatus)
                        {
                            case ButtonActionStatus.SaveAndNoStatusUpdate:
                                status.Messages.Add("Text_SaveSuccess");
                                break;
                            case ButtonActionStatus.NextApproval:
                                status.Messages.Add(ApplicationConstants.SUCCESSMESSAGE);
                                break;
                            case ButtonActionStatus.Complete:
                                status.Messages.Add("Text_CompleteSuccess");
                                break;
                            case ButtonActionStatus.Rejected:
                                status.Messages.Add("Text_RejectedSuccess");
                                break;
                            default:
                                status.Messages.Add(ApplicationConstants.SUCCESSMESSAGE);
                                break;
                        }
                    }
                    else
                    {
                        status.IsSucceed = false;
                        status.Messages.Add(ApplicationConstants.ERRORMESSAGE);
                    }
                }
                return status;
            }
        }
        #endregion

        /// <summary>
        /// Save New Attachment Transaction Data.
        /// </summary>
        /// <param name="lstAttachments">The lstAttachments.</param>
        /// <param name="modelName">The  modelName.</param>
        /// <param name="requestID">The requestID.</param>
        public void SaveNewAttachmentTransactionData(List<ITrans> lstAttachments, string modelName, int requestID)
        {
            try
            {
                ListItem lstNewArtworkAttachmentItem = null;
                List spList = this.web.Lists.GetByTitle(ArtworkListNames.NEWARTWORKATTACHMENTLIST);
                CamlQuery query = new CamlQuery();
                query.ViewXml = @"<View><Query><Where><Eq><FieldRef Name='RequestID' /><Value Type='Lookup'>" + requestID + @"</Value></Eq></Where></Query></View>";
                ListItemCollection items = spList.GetItems(query);
                this.context.Load(items);
                this.context.ExecuteQuery();

                foreach (var item in lstAttachments)
                {
                    var itemdetail = item as NewArtworkAttachment;
                    if (item.ID > 0)
                    {
                        lstNewArtworkAttachmentItem = spList.GetItemById(itemdetail.ID);
                        if (itemdetail.FileName != null && itemdetail.Files.Count == 0)
                        {
                            if (!string.IsNullOrEmpty(itemdetail.FileName))
                            {
                                if (itemdetail.FileName != itemdetail.ArtworkTypeCode + "_" + modelName + "_" + DateTime.Now.ToString("ddMMyy") + System.IO.Path.GetExtension(itemdetail.FileName))
                                {
                                    string[] strDate = System.IO.Path.GetFileNameWithoutExtension(itemdetail.FileName).Split('_');
                                    lstNewArtworkAttachmentItem["FileName"] = itemdetail.ArtworkTypeCode + "_" + modelName + "_" + strDate[strDate.Length - 1] + System.IO.Path.GetExtension(itemdetail.FileName);
                                    List<FileDetails> fileList = JsonConvert.DeserializeObject<List<FileDetails>>(itemdetail.FileNameList);
                                    if (fileList != null)
                                    {
                                        Microsoft.SharePoint.Client.File file = this.context.Web.GetFileByServerRelativeUrl(fileList[0].FileURL);
                                        this.context.Load(file.ListItemAllFields);
                                        this.context.ExecuteQuery();
                                        file.ListItemAllFields["FileDirRef"] = itemdetail.ArtworkTypeCode + "_" + modelName + "_" + strDate[strDate.Length - 1] + System.IO.Path.GetExtension(itemdetail.FileName);
                                        ////file.MoveTo(file.ListItemAllFields["FileDirRef"] + "/" + itemdetail.ArtworkTypeCode + "_" + modelName + "_" + strDate[strDate.Length - 1], MoveOperations.Overwrite);
                                        this.context.ExecuteQuery();
                                    }
                                }
                            }
                        }
                        ////if(item.FileName) item.ArtworkTypeCode + "_" + modelName + "_" + DateTime.Now.ToString("ddMMyy") + System.IO.Path.GetExtension(item.Files[0].FileName);
                    }
                    else
                    {
                        ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                        lstNewArtworkAttachmentItem = spList.AddItem(itemCreateInfo);
                        lstNewArtworkAttachmentItem["RequestID"] = requestID;
                        lstNewArtworkAttachmentItem["ArtworkType"] = itemdetail.ArtworkType;
                        lstNewArtworkAttachmentItem["RequestID"] = itemdetail.RequestBy;
                        lstNewArtworkAttachmentItem["RequestDate"] = itemdetail.RequestDate;
                        lstNewArtworkAttachmentItem["Status"] = itemdetail.Status;
                        lstNewArtworkAttachmentItem["ArtworkTypeCode"] = itemdetail.ArtworkTypeCode;
                        lstNewArtworkAttachmentItem["FileName"] = itemdetail.ArtworkTypeCode + "_" + modelName + "_" + DateTime.Now.ToString("ddMMyy") + System.IO.Path.GetExtension(itemdetail.Files[0].FileName);
                        if (itemdetail.Files != null && itemdetail.Files.Count > 0)
                        {
                            MemoryStream mStream = new MemoryStream(itemdetail.Files[0].FileContent);
                            AttachmentCreationInformation aci = new AttachmentCreationInformation();
                            aci.ContentStream = mStream;
                            aci.FileName = itemdetail.ArtworkTypeCode + "_" + modelName + "_" + DateTime.Now.ToString("ddMMyy") + System.IO.Path.GetExtension(itemdetail.Files[0].FileName);
                            lstNewArtworkAttachmentItem.AttachmentFiles.Add(aci);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error While Save New Artwork Attachment");
                Logger.Error(ex);
            }
        }

        #region Delete Artwork
        /// <summary>
        /// Deletes the artwork.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Action Status</returns>
        public ActionStatus DeleteArtwork(int id)
        {
            ActionStatus status = new ActionStatus();
            try
            {
                ////delete Activity Log
                List activityLogList = this.web.Lists.GetByTitle(ArtworkListNames.NEWARTWORKACTIVITYLOG);
                if (activityLogList != null)
                {
                    CamlQuery query1 = new CamlQuery
                    {
                        ViewXml = @"<View>
                                            <Query>
                                                <Where>
                                                        <Eq>
                                                            <FieldRef Name='RequestID' />
                                                            <Value Type='Lookup'>" + id + @"</Value>
                                                        </Eq>
                                                </Where>
                                            </Query>
                                            </View>"
                    };
                    ListItemCollection items = activityLogList.GetItems(query1);
                    this.context.Load(items);
                    this.context.ExecuteQuery();

                    if (items != null && items.Count != 0)
                    {
                        foreach (ListItem item in items.ToList())
                        {
                            item.DeleteObject();
                        }
                        this.context.ExecuteQuery();
                    }
                }

                ////delete Approval Matrix List
                List approvalMatrixList = this.web.Lists.GetByTitle(ArtworkListNames.NEWARTWORKAPPAPPROVALMATRIX);
                if (approvalMatrixList != null)
                {
                    CamlQuery query2 = new CamlQuery
                    {
                        ViewXml = @"<View>
                                            <Query>
                                                <Where>
                                                        <Eq>
                                                            <FieldRef Name='RequestID' />
                                                            <Value Type='Lookup'>" + id + @"</Value>
                                                        </Eq>
                                                </Where>
                                            </Query>
                                            </View>"
                    };
                    ListItemCollection items1 = approvalMatrixList.GetItems(query2);
                    this.context.Load(items1);
                    this.context.ExecuteQuery();
                    if (items1 != null && items1.Count != 0)
                    {
                        foreach (ListItem i in items1.ToList())
                        {
                            i.DeleteObject();
                        }
                        this.context.ExecuteQuery();
                    }
                }

                List attachmentList = this.web.Lists.GetByTitle(ArtworkListNames.NEWARTWORKATTACHMENTLIST);
                if (attachmentList != null)
                {
                    CamlQuery query3 = new CamlQuery
                    {
                        ViewXml = @"<View>
                                            <Query>
                                                <Where>
                                                        <Eq>
                                                            <FieldRef Name='RequestID' />
                                                            <Value Type='Lookup'>" + id + @"</Value>
                                                        </Eq>
                                                </Where>
                                            </Query>
                                            </View>"
                    };
                    ListItemCollection items2 = attachmentList.GetItems(query3);
                    this.context.Load(items2);
                    this.context.ExecuteQuery();
                    if (items2 != null && items2.Count != 0)
                    {
                        foreach (ListItem j in items2.ToList())
                        {
                            j.DeleteObject();
                        }
                        this.context.ExecuteQuery();
                    }
                }

                List mainList = this.web.Lists.GetByTitle(ArtworkListNames.NEWARTWORKLIST);
                if (mainList != null)
                {
                    ListItem mainListItem = mainList.GetItemById(id);
                    if (mainListItem != null)
                    {
                        mainListItem.DeleteObject();
                        this.context.ExecuteQuery();
                    }
                }
                status.IsSucceed = true;
                status.Messages.Add("Text_DeleteSuccess");
            }
            catch (Exception ex)
            {
                status.IsSucceed = false;
                status.Messages.Add(ApplicationConstants.ERRORMESSAGE);
                Logger.Error("Error while delete New Artwork Request : Message = " + ex.Message + ", Stack Trace = " + ex.StackTrace);
            }
            return status;
        }

        #endregion
    }
}