namespace BEL.ArtworkWorkflow.BusinessLayer
{
    using BEL.CommonDataContract;
    using BEL.ArtworkWorkflow.Models.ExistingArtwork;
    using BEL.ArtworkWorkflow.Models.Common;
    using BEL.DataAccessLayer;
    using Microsoft.SharePoint.Client;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using ArtworkWorkflow.Models;
    using Newtonsoft.Json;
    using BEL.ArtworkWorkflow.Models.NewArtwork;
    using BEL.ArtworkWorkflow.Models.Master;
    using System.IO;

    /// <summary>
    /// Existing Artwork Business Layer
    /// </summary>
    public class ExistingArtworkBusinessLayer
    {
        /// <summary>
        /// The Existing Artwork Business Layer
        /// </summary>
        private static readonly Lazy<ExistingArtworkBusinessLayer> Lazy =
         new Lazy<ExistingArtworkBusinessLayer>(() => new ExistingArtworkBusinessLayer());

        /// <summary>
        /// The ExistingArtworkBusinessLayer instance
        /// </summary>
        public static ExistingArtworkBusinessLayer Instance
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
        /// Existing Artwork Business Layer
        /// </summary>
        private ExistingArtworkBusinessLayer()
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "test")]
        public ExistingArtworkContract GetExistingArtworkDetails(IDictionary<string, string> objDict)
        {
            ExistingArtworkContract contract = new ExistingArtworkContract();
            if (objDict != null && objDict.ContainsKey(Parameter.FROMNAME) && objDict.ContainsKey(Parameter.ITEMID) && objDict.ContainsKey(Parameter.USEREID))
            {
                string formName = objDict[Parameter.FROMNAME];
                int itemId = Convert.ToInt32(objDict[Parameter.ITEMID]);
                string userID = objDict[Parameter.USEREID];
                string userName = objDict["UserName"];
                IForm existingArtworkForm = new ExistingArtworkForm(true);
                existingArtworkForm = BELDataAccessLayer.Instance.GetFormData(this.context, this.web, ArtworkApplicationNameConstants.ARTWORKAPP, formName, itemId, userID, existingArtworkForm);
                if (existingArtworkForm != null && existingArtworkForm.SectionsList != null && existingArtworkForm.SectionsList.Count > 0)
                {
                    var sectionDetails = existingArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ARTWORKDETAILSECTION) as ExistingArtworkDetailSection;
                    if (sectionDetails != null)
                    {
                        List<IMasterItem> rolewiseapprovers = sectionDetails.MasterData.FirstOrDefault(p => p.NameOfMaster == ArtworkMasters.ROLEWISEAPPROVERMASTER).Items;
                        List<RoleWiseApproverMasterListItem> rolewiseapproversList = rolewiseapprovers.ConvertAll(p => (RoleWiseApproverMasterListItem)p);

                        if (itemId == 0)
                        {
                            sectionDetails.Product = "Local";
                            List<IMasterItem> approvers = sectionDetails.MasterData.FirstOrDefault(p => p.NameOfMaster == ArtworkMasters.EXISTINGAPPROVERMASTER).Items.ToList();
                            List<ExistingApproverMasterListItem> approverList = approvers.ConvertAll(p => (ExistingApproverMasterListItem)p);
                            ExistingApproverMasterListItem obj = null;
                            bool isValidRequester = false;
                            foreach (ExistingApproverMasterListItem item in approverList)
                            {
                                if (item.Requester.Split(',').Contains(userID) && (item.DomesticOrImported == ArtworkCommonConstant.LOCAL || item.DomesticOrImported == ArtworkCommonConstant.IMPORTED))
                                {
                                    isValidRequester = true;
                                }
                                if (isValidRequester)
                                {
                                    sectionDetails.RequesterBusinessUnit = item.BusinessUnit;
                                    sectionDetails.RequesterDomesticOrImported = item.DomesticOrImported;
                                    obj = item;
                                    break;
                                }
                            }
                            if (obj != null)
                            {
                                ////Get Department User
                                sectionDetails.ApproversList.ForEach(p =>
                                {
                                    if (p.Role == ArtworkRoles.CREATOR)
                                    {
                                        p.Approver = userID;
                                        p.ApproverName = userName;
                                    }
                                    ////else if (p.Role == ArtworkRoles.BUAPPROVER)
                                    ////{
                                    ////    p.Approver = obj.BUApprover;
                                    ////    p.ApproverName = obj.BUApproverName;
                                    ////}
                                    ////else if (p.Role == ArtworkRoles.SCMLEVEL1APPROVER)
                                    ////{
                                    ////    p.Approver = obj.SCMLevel1Approver;
                                    ////    p.ApproverName = obj.SCMLevel1ApproverName;
                                    ////}
                                    ////else if (p.Role == ArtworkRoles.SCMLEVEL2APPROVER)
                                    ////{
                                    ////    p.Approver = obj.SCMLevel2Approver;
                                    ////    p.ApproverName = obj.SCMLevel2ApproverName;
                                    ////}
                                    ////else if (p.Role == ArtworkRoles.ADBLEVEL1APPROVER)
                                    ////{
                                    ////    p.Approver = obj.ADBLevel1Approver;
                                    ////    p.ApproverName = obj.ADBLevel1ApproverName;
                                    ////}
                                    ////else if (p.Role == ArtworkRoles.ADBLEVEL2APPROVER)
                                    ////{
                                    ////    p.Approver = obj.ADBLevel2Approver;
                                    ////    p.ApproverName = obj.ADBLevel2ApproverName;
                                    ////}
                                    ////else if (p.Role == ArtworkRoles.QALEVEL1APPROVER)
                                    ////{
                                    ////    p.Approver = obj.QALevel1Approver;
                                    ////    p.ApproverName = obj.QALevel1ApproverName;
                                    ////}
                                    ////else if (p.Role == ArtworkRoles.QALEVEL2APPROVER)
                                    ////{
                                    ////    p.Approver = obj.QALevel2Approver;
                                    ////    p.ApproverName = obj.QALevel2ApproverName;
                                    ////}
                                    ////else if (p.Role == ArtworkRoles.MARKETINGLEVEL1APPROVER)
                                    ////{
                                    ////    p.Approver = obj.MarketingLevel1Approver;
                                    ////    p.ApproverName = obj.MarketingLevel1ApproverName;
                                    ////}
                                    ////else if (p.Role == ArtworkRoles.MARKETINGLEVEL2APPROVER)
                                    ////{
                                    ////    p.Approver = obj.MarketingLevel2Approver;
                                    ////    p.ApproverName = obj.MarketingLevel2ApproverName;
                                    ////}
                                });
                            }
                            if (rolewiseapproversList != null)
                            {
                                ////Get Role wise Department User
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

                            var absqTeamSection = existingArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.ABSQTEAMSECTION)) as ExistingArtworkABSQTeamSection;
                            if (absqTeamSection != null)
                            {
                                absqTeamSection.ArtworkAttachmentTempList = new List<ExistingArtworkAttachment>();
                                absqTeamSection.ArtworkAttachmentTempList.AddRange(absqTeamSection.ArtworkAttachmentList.ConvertAll(o => (o as ExistingArtworkAttachment)));

                                if (absqTeamSection.ArtworkAttachmentTempList.Count == 0 && absqTeamSection.IsActive)
                                {
                                    string strArtworkType = absqTeamSection.ArtworkType;
                                    List<IMasterItem> artworkTypeList = sectionDetails.MasterData.FirstOrDefault(p => p.NameOfMaster == ArtworkMasters.ARTWORKTYEMASTER).Items;
                                    List<ArtworkTypeMasterListItem> lstDefaultSelectedArtworkType = artworkTypeList.ConvertAll(p => (ArtworkTypeMasterListItem)p);

                                    string strFiles = this.GetArtworkTypeFilesByItemCode(sectionDetails.ItemCode);
                                    List<ItemMasterArtworkTypeMappingMasterListItem> lstartWorkFiles = JSONHelper.ToObject<List<ItemMasterArtworkTypeMappingMasterListItem>>(strFiles);
                                    if (lstartWorkFiles != null && lstartWorkFiles.Any(p => p.Files == null))
                                    {
                                        lstartWorkFiles.ForEach(p =>
                                        {
                                            ExistingArtworkAttachment obj = new ExistingArtworkAttachment();
                                            obj.ArtworkType = p.Title;
                                            obj.ArtworkTypeCode = lstDefaultSelectedArtworkType.FirstOrDefault(x => x.Title == p.Title) != null ? lstDefaultSelectedArtworkType.FirstOrDefault(x => x.Title == p.Title).ArtworkTypeCode : string.Empty;
                                            obj.RequestID = itemId;
                                            obj.RequestDate = (DateTime)sectionDetails.RequestDate;
                                            obj.RequestBy = sectionDetails.ProposedBy;
                                            obj.ItemAction = ItemActionStatus.NEW;
                                            absqTeamSection.ArtworkAttachmentTempList.Add(obj);
                                        });
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrWhiteSpace(strArtworkType))
                                        {
                                            string[] artworkTypeArray = strArtworkType.Split(',');
                                            foreach (string item in artworkTypeArray)
                                            {
                                                ExistingArtworkAttachment obj = new ExistingArtworkAttachment();
                                                obj.ArtworkType = item;
                                                obj.ArtworkTypeCode = lstDefaultSelectedArtworkType.FirstOrDefault(x => x.Title == item) != null ? lstDefaultSelectedArtworkType.FirstOrDefault(x => x.Title == item).ArtworkTypeCode : string.Empty;
                                                obj.RequestID = itemId;
                                                obj.RequestDate = (DateTime)sectionDetails.RequestDate;
                                                obj.RequestBy = sectionDetails.ProposedBy;
                                                obj.ItemAction = ItemActionStatus.NEW;
                                                absqTeamSection.ArtworkAttachmentTempList.Add(obj);
                                            }
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

                            var qaLevel1Section = existingArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.QALEVEL1SECTION)) as ExistingArtworkQALevel1Section;
                            if (qaLevel1Section != null)
                            {
                                qaLevel1Section.FNLQALevel1Attachment = qaLevel1Section.Files != null && qaLevel1Section.Files.Count > 0 ? JsonConvert.SerializeObject(qaLevel1Section.Files.Where(x => !string.IsNullOrEmpty(qaLevel1Section.FNLQALevel1Attachment) && qaLevel1Section.FNLQALevel1Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                            }

                            var qaLevel2Section = existingArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.QALEVEL2SECTION)) as ExistingArtworkQALevel2Section;
                            if (qaLevel2Section != null)
                            {
                                qaLevel2Section.FNLQALevel2Attachment = qaLevel2Section.Files != null && qaLevel2Section.Files.Count > 0 ? JsonConvert.SerializeObject(qaLevel2Section.Files.Where(x => !string.IsNullOrEmpty(qaLevel2Section.FNLQALevel2Attachment) && qaLevel2Section.FNLQALevel2Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                            }

                            var scmLevel1Section = existingArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.SCMLEVEL1SECTION)) as ExistingArtworkSCMLevel1Section;
                            if (scmLevel1Section != null)
                            {
                                scmLevel1Section.FNLSCMLevel1Attachment = scmLevel1Section.Files != null && scmLevel1Section.Files.Count > 0 ? JsonConvert.SerializeObject(scmLevel1Section.Files.Where(x => !string.IsNullOrEmpty(scmLevel1Section.FNLSCMLevel1Attachment) && scmLevel1Section.FNLSCMLevel1Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                            }

                            var scmLevel2Section = existingArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.SCMLEVEL2SECTION)) as ExistingArtworkSCMLevel2Section;
                            if (scmLevel2Section != null)
                            {
                                scmLevel2Section.FNLSCMLevel2Attachment = scmLevel2Section.Files != null && scmLevel2Section.Files.Count > 0 ? JsonConvert.SerializeObject(scmLevel2Section.Files.Where(x => !string.IsNullOrEmpty(scmLevel2Section.FNLSCMLevel2Attachment) && scmLevel2Section.FNLSCMLevel2Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                            }

                            var abdLevel1Section = existingArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.ADBLEVEL1SECTION)) as ExistingArtworkADBLevel1Section;
                            if (abdLevel1Section != null)
                            {
                                abdLevel1Section.FNLADBLevel1Attachment = abdLevel1Section.Files != null && abdLevel1Section.Files.Count > 0 ? JsonConvert.SerializeObject(abdLevel1Section.Files.Where(x => !string.IsNullOrEmpty(abdLevel1Section.FNLADBLevel1Attachment) && abdLevel1Section.FNLADBLevel1Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                            }

                            var abdLevel2Section = existingArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.ADBLEVEL2SECTION)) as ExistingArtworkADBLevel2Section;
                            if (abdLevel2Section != null)
                            {
                                abdLevel2Section.FNLADBLevel2Attachment = abdLevel2Section.Files != null && abdLevel2Section.Files.Count > 0 ? JsonConvert.SerializeObject(abdLevel2Section.Files.Where(x => !string.IsNullOrEmpty(abdLevel2Section.FNLADBLevel2Attachment) && abdLevel2Section.FNLADBLevel2Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                            }

                            var mktLevel1Section = existingArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.MARKETINGLEVEL1SECTION)) as ExistingArtworkMarketingLevel1Section;
                            if (mktLevel1Section != null)
                            {
                                mktLevel1Section.FNLMarketingLevel1Attachment = mktLevel1Section.Files != null && mktLevel1Section.Files.Count > 0 ? JsonConvert.SerializeObject(mktLevel1Section.Files.Where(x => !string.IsNullOrEmpty(mktLevel1Section.FNLMarketingLevel1Attachment) && mktLevel1Section.FNLMarketingLevel1Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                            }

                            var mktLevel2Section = existingArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.MARKETINGLEVEL2SECTION)) as ExistingArtworkMarketingLevel2Section;
                            if (mktLevel2Section != null)
                            {
                                mktLevel2Section.FNLMarketingLevel2Attachment = mktLevel2Section.Files != null && mktLevel2Section.Files.Count > 0 ? JsonConvert.SerializeObject(mktLevel2Section.Files.Where(x => !string.IsNullOrEmpty(mktLevel2Section.FNLMarketingLevel2Attachment) && mktLevel2Section.FNLMarketingLevel2Attachment.Split(',').Contains(x.FileName)).ToList()) : string.Empty;
                            }

                            var absqApproverSection = existingArtworkForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.ABSQAPPROVERSECTION)) as ExistingArtworkABSQApproverSection;
                            if (absqApproverSection != null)
                            {
                                absqApproverSection.CC = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.CC).Approvers;
                                absqApproverSection.CCName = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.CC).ApproversName;

                                absqApproverSection.Legal = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.LEGAL).Approvers;
                                absqApproverSection.LegalName = rolewiseapproversList.FirstOrDefault(q => q.Role == ArtworkRoles.LEGAL).ApproversName;
                            }
                        }
                        ////sectionDetails.FileNameList = sectionDetails.Files != null && sectionDetails.Files.Count > 0 ? JsonConvert.SerializeObject(sectionDetails.Files) : string.Empty;
                        ////if (sectionDetails.Status == FormStatus.COMPLETED || sectionDetails.Status == FormStatus.REJECTED)
                        ////{
                        ////    if (existingArtworkForm.Buttons.FirstOrDefault(p => p.Name == "Print") != null)
                        ////    {
                        ////        existingArtworkForm.Buttons.Remove(existingArtworkForm.Buttons.FirstOrDefault(f => f.ButtonStatus == ButtonActionStatus.Print));
                        ////    }
                        ////}
                    }
                    contract.Forms.Add(existingArtworkForm);
                }
            }
            return contract;
        }

        #endregion

        #region "SAVE DATA"
        /// <summary>
        /// Saves the by section.
        /// </summary>
        /// <param name="sections">The sections.</param>
        /// <param name="objDict">The object dictionary.</param>
        /// <returns>return status</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "NA")]
        public ActionStatus SaveBySection(ISection sectionDetails, Dictionary<string, string> objDict)
        {
            lock (Padlock)
            {
                ActionStatus status = new ActionStatus();
                ExistingArtworkNoCount currentExistingArtworkNo = null;
                if (sectionDetails != null && objDict != null)
                {
                    objDict[Parameter.ACTIVITYLOG] = ArtworkListNames.EXISTINGARTWORKACTIVITYLOG;
                    objDict[Parameter.APPLICATIONNAME] = ArtworkApplicationNameConstants.ARTWORKAPP;
                    objDict[Parameter.FROMNAME] = ArtworkFormNames.EXISTINGARTWORKFORM;
                    ExistingArtworkDetailSection section = null;

                    if (sectionDetails.SectionName == ArtworkSectionName.ARTWORKDETAILSECTION)
                    {
                        section = sectionDetails as ExistingArtworkDetailSection;
                        if (!this.IsValidExistingArtworkEntry(section.ItemCode, sectionDetails.ListDetails[0].ItemId) && string.IsNullOrEmpty(section.OldReferenceNo))
                        {
                            status.IsSucceed = false;
                            status.Messages.Add("Text_ItemCodeLocked");
                            return status;
                        }
                        if (sectionDetails.ActionStatus == ButtonActionStatus.NextApproval && string.IsNullOrEmpty(section.ReferenceNo))
                        {
                            section.RequestDate = DateTime.Now;
                            currentExistingArtworkNo = this.GetExistingArtworkNo(section.BusinessUnit);
                            if (currentExistingArtworkNo != null)
                            {
                                currentExistingArtworkNo.CurrentValue = currentExistingArtworkNo.CurrentValue + 1;
                                Logger.Info("Existing Artwork Current Value + 1 = " + currentExistingArtworkNo.CurrentValue);
                                ////section.ReferenceNo = string.Format("{0}-{1}-{2}-{3}-{4}", ArtworkCommonConstant.REFERENCENOPREFIX, ArtworkCommonConstant.EXISTINGARTWORKREQUESTABBREVIATION, section.BusinessUnit, section.RequestDate.Value.ToString("yyyy"), string.Format("{0:00000}", currentExistingArtworkNo.CurrentValue));
                                section.ReferenceNo = string.Format("{0}-{1}-{2}-{3}", section.BusinessUnit, ArtworkCommonConstant.EXISTINGARTWORKREQUESTABBREVIATION, section.RequestDate.Value.ToString("yyyy"), string.Format("{0:00000}", currentExistingArtworkNo.CurrentValue));
                                Logger.Info("Existing Artwork No is " + section.ReferenceNo);
                                status.ExtraData = section.ReferenceNo;
                            }
                        }
                    }
                    List<ListItemDetail> objSaveDetails = BELDataAccessLayer.Instance.SaveData(this.context, this.web, sectionDetails, objDict);
                    ListItemDetail itemDetails = objSaveDetails.Where(p => p.ListName.Equals(ArtworkListNames.EXISTINGARTWORKLIST)).FirstOrDefault<ListItemDetail>();
                    if (sectionDetails.SectionName == ArtworkSectionName.ARTWORKDETAILSECTION)
                    {
                        if (!string.IsNullOrEmpty(section.OldReferenceNo))
                        {
                            Dictionary<string, dynamic> values = new Dictionary<string, dynamic>();
                            values.Add("IsArtworkRetrieved", true);
                            BELDataAccessLayer.Instance.SaveFormFields(this.context, this.web, ArtworkListNames.EXISTINGARTWORKLIST, section.OldRequestId, values);
                        }

                        if (itemDetails.ItemId > 0 && currentExistingArtworkNo != null)
                        {
                            this.UpdateNewArtworkNoCount(currentExistingArtworkNo);
                            Logger.Info("Update Reference No " + section.ReferenceNo);
                        }
                    }

                    if (itemDetails.ItemId > 0)
                    {
                        status.IsSucceed = true;
                        if (sectionDetails.ActionStatus == ButtonActionStatus.Complete)
                        {
                            Dictionary<string, string> objContract = new Dictionary<string, string>();
                            objContract.Add(Parameter.FROMNAME, ArtworkFormNames.EXISTINGARTWORKFORM);
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
        /// Get NPD No Logic
        /// </summary>
        /// <param name="businessunit">Business Unit</param>
        /// <returns>NPD No Count</returns>
        public ExistingArtworkNoCount GetExistingArtworkNo(string businessunit)
        {
            try
            {
                List<ExistingArtworkNoCount> lstNPDCount = new List<ExistingArtworkNoCount>();
                List spList = this.web.Lists.GetByTitle(ArtworkListNames.EXISTINGARTWORKNOCOUNT);
                CamlQuery query = new CamlQuery();
                query.ViewXml = @"<View><ViewFields><FieldRef Name='Title' /><FieldRef Name='Year' /><FieldRef Name='CurrentValue' /></ViewFields>   </View>";
                ListItemCollection items = spList.GetItems(query);
                this.context.Load(items);
                this.context.ExecuteQuery();
                if (items != null && items.Count != 0)
                {
                    foreach (ListItem item in items)
                    {
                        ExistingArtworkNoCount obj = new ExistingArtworkNoCount();
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
        /// Get NPD No Logic
        /// </summary>
        /// <param name="itemCode">item Code</param>
        /// <param name="itemID">item ID</param>
        /// <returns>The Valid entry or not</returns>
        public bool IsValidExistingArtworkEntry(string itemCode, int itemID)
        {
            try
            {
                bool isValid = false;
                List spList = this.web.Lists.GetByTitle(ArtworkListNames.EXISTINGARTWORKLIST);
                CamlQuery query = new CamlQuery();
                query.ViewXml = @"<View><Query><Where><And><Eq><FieldRef Name='ItemCode'/><Value Type='Text'>" + itemCode + "</Value></Eq><And><Neq><FieldRef Name='ID' /><Value Type='Number'>" + itemID + "</Value></Neq><Neq><FieldRef Name='WorkflowStatus'/><Value Type='Text'>Completed</Value></Neq></And></And></Where><OrderBy><FieldRef Name='OldArtworkRejectedDate' Ascending='False'/></OrderBy></Query></View>";
                ListItemCollection items = spList.GetItems(query);
                this.context.Load(items, includes => includes.Include(i => i["ID"], i => i["Status"], i => i["OldArtworkRejectedDate"]));
                this.context.ExecuteQuery();
                if (items.Count == 0)
                {
                    isValid = true;
                }
                if (!isValid)
                {
                    bool isInprogressRequest = false;
                    foreach (ListItem listItem in items)
                    {
                        if (!string.IsNullOrWhiteSpace(Convert.ToString(listItem["Status"])))
                        {
                            if (Convert.ToString(listItem["Status"]) != "Rejected")
                            {
                                isValid = false;
                                isInprogressRequest = true;
                                break;
                            }
                        }
                    }
                    if (!isInprogressRequest)
                    {
                        foreach (ListItem listItem in items)
                        {
                            if (!string.IsNullOrWhiteSpace(Convert.ToString(listItem["OldArtworkRejectedDate"])))
                            {
                                DateTime dtRejectedDate = Convert.ToDateTime(listItem["OldArtworkRejectedDate"]);
                                if (DateTime.Now.Date > dtRejectedDate.AddDays(Convert.ToInt32(BELDataAccessLayer.Instance.GetConfigVariable("LockingDays"))).Date)
                                {
                                    isValid = true;
                                    break;
                                }
                                else
                                {
                                    isValid = false;
                                    break;
                                }
                            }
                        }
                    }
                }
                return isValid;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return true;
            }
        }

        /// <summary>
        /// Update New Artwork No Count
        /// </summary>
        /// <param name="currentValue">Current Value</param>
        public void UpdateNewArtworkNoCount(ExistingArtworkNoCount currentValue)
        {
            if (currentValue != null && currentValue.ID != 0)
            {
                try
                {
                    Logger.Info("Aync update Existing Artwork Current value currentValue : " + currentValue.CurrentValue + " BusinessUnit : " + currentValue.BusinessUnit);
                    List spList = this.web.Lists.GetByTitle(ArtworkListNames.EXISTINGARTWORKNOCOUNT);
                    ListItem item = spList.GetItemById(currentValue.ID);
                    item["CurrentValue"] = currentValue.CurrentValue;
                    item["Year"] = currentValue.Year;
                    item.Update();
                    ////context.Load(item);
                    this.context.ExecuteQuery();
                }
                catch (Exception ex)
                {
                    Logger.Error("Error while Update Existing Artwork no Current Value");
                    Logger.Error(ex);
                }
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
                ExistingArtworkContract contract = null;
                contract = this.GetExistingArtworkDetails(objContract);
                if (contract != null && contract.Forms != null && contract.Forms.Count > 0)
                {
                    ExistingArtworkDetailSection artworkDetailSection = contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ARTWORKDETAILSECTION) as ExistingArtworkDetailSection;
                    ExistingArtworkABSQTeamSection artworkABSQTeamSection = contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ABSQTEAMSECTION) as ExistingArtworkABSQTeamSection;
                    List<ExistingArtworkAttachment> artworkfiles = artworkABSQTeamSection.ArtworkAttachmentList.ConvertAll(o => (o as ExistingArtworkAttachment));

                    ////List<IMasterItem> itemMaster = artworkABSQTeamSection.MasterData.FirstOrDefault(p => p.NameOfMaster == ArtworkMasters.ITEMMASTER).Items;
                    ////var item = itemMaster.Where(p => p.Title == artworkDetailSection.ItemCode).FirstOrDefault();
                    ////objItemMaster.ID = (item as ItemMasterListItem).ID;

                    string strItem = this.GetArtworkInfoByItemCode(artworkDetailSection.ItemCode);
                    if (!string.IsNullOrEmpty(strItem))
                    {
                        ItemMasterListItem item = JSONHelper.ToObject<ItemMasterListItem>(strItem);

                        artworkfiles.ForEach(p =>
                        {
                            if (p.FileNameList != null)
                            {
                                p.ItemAction = ItemActionStatus.NEW;
                                p.ItemCode = artworkDetailSection.ItemCode;
                            }
                        });
                        this.SaveItemArtworkTypeMasterData(Convert.ToString(item.ID), artworkfiles, false);
                    }
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
        public void SaveItemArtworkTypeMasterData(string itemMasterItemID, List<ExistingArtworkAttachment> artworkfiles, bool isNew)
        {
            try
            {
                List spList = this.web.Lists.GetByTitle(ArtworkMasters.ITEMMASTER);
                List spArtworkTypeMappingMaster = this.web.Lists.GetByTitle(ArtworkMasters.ITEMMASTERARTWORKTYPEMAPPINGMASTER);
                ListItemCollection lstArtworkTypeMApping = spArtworkTypeMappingMaster.GetItems(CamlQuery.CreateAllItemsQuery());
                ListItem oItemListItem = null;
                oItemListItem = spList.GetItemById(Convert.ToInt32(itemMasterItemID));
                oItemListItem["IsNewArtwork"] = true;
                oItemListItem["LockingDate"] = DateTime.Now.AddDays(Convert.ToInt32(BELDataAccessLayer.Instance.GetConfigVariable("LockingDays")));
                oItemListItem.Update();
                this.context.Load(lstArtworkTypeMApping);
                this.context.ExecuteQuery();
                foreach (ExistingArtworkAttachment item in artworkfiles)
                {
                    ListItem oArtworkTypeMappingItem = null;
                    ListItem oArtworkTypeMappingItemUpdate = null;
                    oArtworkTypeMappingItem = lstArtworkTypeMApping.FirstOrDefault(x => Convert.ToString(x.FieldValues["ItemCode"]) == item.ItemCode && Convert.ToString(x.FieldValues["Title"]) == item.ArtworkType);
                    if (oArtworkTypeMappingItem != null)
                    {
                        oArtworkTypeMappingItemUpdate = spArtworkTypeMappingMaster.GetItemById(oArtworkTypeMappingItem.Id);
                        this.context.Load(oArtworkTypeMappingItemUpdate, li => li.AttachmentFiles);
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
                    oArtworkTypeMappingItemUpdate["RequestID"] = itemMasterItemID;
                    oArtworkTypeMappingItemUpdate["RequestBy"] = item.RequestBy;
                    oArtworkTypeMappingItemUpdate["Title"] = item.ArtworkType;
                    oArtworkTypeMappingItemUpdate["ArtworkTypeCode"] = item.ArtworkTypeCode;
                    oArtworkTypeMappingItemUpdate["ArtworkType"] = item.ArtworkType;
                    oArtworkTypeMappingItemUpdate.Update();
                    this.context.ExecuteQuery();
                    oArtworkTypeMappingItemUpdate.AttachmentFiles.ToList().ForEach(a => a.DeleteObject());
                    this.context.ExecuteQuery();
                    using (MemoryStream mStream = new MemoryStream(CommonBusinessLayer.Instance.DownloadFile(item.Files[0].FileURL, "Artworks")))
                    {
                        AttachmentCreationInformation aci = new AttachmentCreationInformation();
                        aci.ContentStream = mStream;
                        aci.FileName = item.Files[0].FileName;
                        oArtworkTypeMappingItemUpdate.AttachmentFiles.Add(aci);
                        this.context.ExecuteQuery();
                    }
                    ////Microsoft.SharePoint.Client.File file = this.context.Web.GetFileByServerRelativeUrl(item.Files[0].FileURL);
                    ////ClientResult<System.IO.Stream> data = file.OpenBinaryStream();
                    ////this.context.Load(file);
                    ////this.context.ExecuteQuery();
                    ////using (System.IO.MemoryStream mStream = new System.IO.MemoryStream())
                    ////{
                    ////    if (data != null)
                    ////    {
                    ////        data.Value.CopyTo(mStream);
                    ////        ////byte[] imageArray = mStream.ToArray();
                    ////        AttachmentCreationInformation aci = new AttachmentCreationInformation();
                    ////        aci.ContentStream = mStream;
                    ////        aci.FileName = item.Files[0].FileName;
                    ////        oArtworkTypeMappingItemUpdate.AttachmentFiles.Add(aci);
                    ////        this.context.ExecuteQuery();
                    ////    }
                    ////}
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error While Save Item Master Data and Artwork Type Item Code Mapping Data");
                Logger.Error(ex);
            }
            ////GlobalCachingProvider.Instance.RemoveItem(ArtworkMasters.ITEMMASTER);
            ////GlobalCachingProvider.Instance.RemoveItem(ArtworkMasters.ITEMMASTERARTWORKTYPEMAPPINGMASTER);
            ////AsyncHelper.Call(obj =>
            ////{
            ////    MasterDataHelper masterhelper = new MasterDataHelper();
            ////    masterhelper.GetMasterData(this.context, this.web, new List<IMaster>() { new ItemMaster() });
            ////});
        }
        #endregion

        #region "GET Admin DATA"
        /// <summary>
        /// Gets the Artwork details.
        /// </summary>
        /// <param name="objDict">The object dictionary.</param>
        /// <returns>byte array</returns>
        public ExistingArtworkContract GetExistingArtworkAdminDetails(IDictionary<string, string> objDict)
        {
            ExistingArtworkContract contract = new ExistingArtworkContract();
            if (objDict != null && objDict.ContainsKey(Parameter.FROMNAME) && objDict.ContainsKey(Parameter.ITEMID) && objDict.ContainsKey(Parameter.USEREID))
            {
                string formName = objDict[Parameter.FROMNAME];
                int itemId = Convert.ToInt32(objDict[Parameter.ITEMID]);
                string userID = objDict[Parameter.USEREID];
                IForm existingartAdminForm = new ExistingArtworkAdminForm(true);
                existingartAdminForm = BELDataAccessLayer.Instance.GetFormData(this.context, this.web, ArtworkApplicationNameConstants.ARTWORKAPP, formName, itemId, userID, existingartAdminForm);
                if (existingartAdminForm != null && existingartAdminForm.SectionsList != null && existingartAdminForm.SectionsList.Count > 0)
                {
                    var sectionDetails = existingartAdminForm.SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ARTWORKADMINDETAILSECTION) as ExistingArtworkAdminDetailSection;
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
                        }
                        //// var absqTeamSection = newartAdminForm.SectionsList.FirstOrDefault(f => f.SectionName.Equals(ArtworkSectionName.ABSQTEAMSECTION)) as NewArtworkABSQTeamSection;
                        if (sectionDetails != null)
                        {
                            sectionDetails.ArtworkAttachmentTempList = new List<ExistingArtworkAttachment>();
                            sectionDetails.ArtworkAttachmentTempList.AddRange(sectionDetails.ArtworkAttachmentList.ConvertAll(o => (o as ExistingArtworkAttachment)));
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
                                        ExistingArtworkAttachment obj = new ExistingArtworkAttachment();
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
                    contract.Forms.Add(existingartAdminForm);
                }
            }
            return contract;
        }

        #endregion

        #region "SAVE Existing Admin DATA"
        /// <summary>
        /// Saves the by section.
        /// </summary>
        /// <param name="sections">The sections.</param>
        /// <param name="objDict">The object dictionary.</param>
        /// <returns>return status</returns>
        public ActionStatus SaveExistingArtworkAdminDataBySection(ISection sectionDetails, Dictionary<string, string> objDict)
        {
            lock (Padlock)
            {
                ActionStatus status = new ActionStatus();

                ////BELDataAccessLayer helper = new BELDataAccessLayer();
                if (sectionDetails != null && objDict != null)
                {
                    objDict[Parameter.ACTIVITYLOG] = ArtworkListNames.EXISTINGARTWORKACTIVITYLOG;
                    objDict[Parameter.APPLICATIONNAME] = ArtworkApplicationNameConstants.ARTWORKAPP;
                    objDict[Parameter.FROMNAME] = ArtworkFormNames.EXISTINGARTWORKADMINFORM;
                    ////NPDAdminDetailSection section = null;
                    ////if (sectionDetails.SectionName == NPDSectionName.NPDADMINSECTION)
                    ////{
                    ////    section = sectionDetails as NPDAdminDetailSection;
                    ////}
                    List<ListItemDetail> objSaveDetails = BELDataAccessLayer.Instance.SaveData(this.context, this.web, sectionDetails, objDict);
                    ListItemDetail itemDetails = objSaveDetails.Where(p => p.ListName.Equals(ArtworkListNames.EXISTINGARTWORKLIST)).FirstOrDefault<ListItemDetail>();
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

        #region "Get ITem Code Data"
        /// <summary>
        /// Get All Items Code
        /// </summary>
        /// <param name="title">search term</param>
        /// <returns>string data</returns>
        public string GetAllItemsCode(string title)
        {
            IMaster itemmaster = this.SearchItemMasterData(title);
            return JSONHelper.ToJSON<IMaster>(itemmaster);
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
        /// Get Artwork Info By Item Code
        /// </summary>
        /// <param name="itemCode">item code</param>
        /// <returns>Artwork Detail</returns>
        public string GetArtworkInfoByItemCode(string itemCode)
        {
            try
            {
                List spList = this.web.Lists.GetByTitle(ArtworkMasters.ITEMMASTER);
                CamlQuery query = new CamlQuery();
                query.ViewXml = @"<View>
                                    <Query>
                                        <Where>
                                            <Eq>
                                                <FieldRef Name='ItemCode' />
                                                <Value Type='Text'>" + itemCode + @"</Value>
                                            </Eq>
                                        </Where><RowLimit>1</RowLimit>
                                    </Query> 
                                </View>";

                ////query.ViewXml = @"<View><ViewFields><FieldRef Name='Title' /><FieldRef Name='Model' /></ViewFields>   </View>";
                ListItemCollection items = spList.GetItems(query);
                this.context.Load(items);
                this.context.ExecuteQuery();
                if (items != null && items.Count != 0)
                {
                    ItemMasterListItem obj = new ItemMasterListItem();
                    obj.ID = items[0].Id;
                    obj.ItemCode = Convert.ToString(items[0]["ItemCode"]);
                    obj.Title = Convert.ToString(items[0]["Title"]);
                    obj.ModelName = Convert.ToString(items[0]["Model"]);
                    obj.ReferenceNo = Convert.ToString(items[0]["ReferenceNo"]);
                    obj.BusinessUnit = Convert.ToString(items[0]["BusinessUnit"]);
                    obj.DomesticOrImported = Convert.ToString(items[0]["DomesticOrImported"]);
                    obj.ProductCategory = Convert.ToString(items[0]["ProductCategory"]);
                    obj.BarCode = Convert.ToString(items[0]["BarCode"]);
                    obj.MRP = Convert.ToString(items[0]["MRP"]);
                    obj.NoofCartoninMaster = Convert.ToString(items[0]["NoofCartoninMaster"]);
                    obj.UnitCartonDimensionL = Convert.ToString(items[0]["UnitCartonDimensionL"]);
                    obj.UnitCartonDimensionW = Convert.ToString(items[0]["UnitCartonDimensionW"]);
                    obj.UnitCartonDimensionH = Convert.ToString(items[0]["UnitCartonDimensionH"]);
                    obj.MasterCartondimensionL = Convert.ToString(items[0]["MasterCartondimensionL"]);
                    obj.MasterCartondimensionW = Convert.ToString(items[0]["MasterCartondimensionW"]);
                    obj.MasterCartondimensionH = Convert.ToString(items[0]["MasterCartondimensionH"]);
                    obj.VendorCode = Convert.ToString(items[0]["VendorCode"]);
                    obj.VendorAddress = Convert.ToString(items[0]["VendorAddress"]);
                    obj.Warranty = Convert.ToString(items[0]["Warranty"]);
                    obj.LockingDate = Convert.ToDateTime(items[0]["LockingDate"]);
                    obj.Voltage = Convert.ToString(items[0]["Voltage"]);
                    obj.Power = Convert.ToString(items[0]["Power"]);
                    obj.NetWeight = Convert.ToString(items[0]["NetWeight"]);
                    obj.GrossWeight = Convert.ToString(items[0]["GrossWeight"]);
                    obj.Color = Convert.ToString(items[0]["Color"]);
                    obj.Product = Convert.ToString(items[0]["Product"]);
                    obj.IsNewArtwork = Convert.ToBoolean(items[0]["IsNewArtwork"]);
                    return JSONHelper.ToJSON<ItemMasterListItem>(obj);
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
        /// Get Artwork Type Files By ID
        /// </summary>
        /// <param name="title">item id</param>
        /// <returns>string data</returns>
        public string GetArtworkTypeFilesByID(string id)
        {
            List spList = this.web.Lists.GetByTitle(ArtworkMasters.ITEMMASTERARTWORKTYPEMAPPINGMASTER);
            CamlQuery query = new CamlQuery();
            query.ViewXml = @"<View><Query><Where><And><Eq><FieldRef Name='RequestID'  /><Value Type='Lookup'>" + Convert.ToInt32(id) + @"</Value></Eq><Neq>
                                                                      <FieldRef Name='Status'   />
                                                                      <Value Type='Text'>" + TaskStatus.DELETED + @"</Value>
                                                                     </Neq></And></Where></Query></View>";
            ListItemCollection items = spList.GetItems(query);
            this.context.Load(items);
            this.context.ExecuteQuery();

            if (items != null && items.Count != 0)
            {
                List<ItemMasterArtworkTypeMappingMasterListItem> lstArtworkTypeMapping = new List<ItemMasterArtworkTypeMappingMasterListItem>();
                MasterDataHelper helper = new MasterDataHelper();
                IMaster objMaster = new ItemMasterArtworkTypeMappingMaster();
                objMaster = helper.AssignPropertyValues(objMaster, this.context, this.web, items, typeof(ItemMasterArtworkTypeMappingMasterListItem));
                lstArtworkTypeMapping = objMaster.Items.ConvertAll(o => (o as ItemMasterArtworkTypeMappingMasterListItem));

                ////foreach (ListItem item in items)
                ////{
                ////    ItemMasterArtworkTypeMappingMasterListItem obj = new ItemMasterArtworkTypeMappingMasterListItem();
                ////    obj.ID = item.Id;
                ////    obj.FileName = Convert.ToString(item["FileName"]);
                ////    obj.Title = Convert.ToString(item["Title"]);
                ////    obj.ArtworkType = Convert.ToString(item["ArtworkType"]);
                ////    obj.ArtworkTypeCode = Convert.ToString(item["ArtworkTypeCode"]);
                ////    obj.ItemCode = Convert.ToString(item["ItemCode"]);
                ////    obj.RequestID = Convert.ToString(item["ArtworkTypeCode"]);
                ////    obj.ArtworkTypeCode = Convert.ToString(item["ArtworkTypeCode"]);
                ////    if (Convert.ToString(item["Attachments"]) == "True")
                ////    {
                ////        this.context.Load(item.AttachmentFiles);
                ////        this.context.ExecuteQuery();
                ////        AttachmentCollection attachments = item.AttachmentFiles;
                ////        List<FileDetails> objAttachmentFiles = MasterDataHelper.Instance.GetAttachments(attachments);
                ////        obj.Files = objAttachmentFiles;
                ////    }

                ////    lstArtworkTypeMapping.Add(obj);
                ////}
                return JSONHelper.ToJSON<List<ItemMasterArtworkTypeMappingMasterListItem>>(lstArtworkTypeMapping);
            }
            else
            {
                return null;
            }
            ////MasterDataHelper masterHelper = new MasterDataHelper();
            ////IMaster itemArtworkmaster = masterHelper.GetMasterData(this.context, this.web, new List<IMaster>() { new ItemMasterArtworkTypeMappingMaster() }).FirstOrDefault();
            ////string itemartworkjson = JSONHelper.ToJSON<IMaster>(itemArtworkmaster);
            ////var filterditemartwork = JSONHelper.ToObject<IMaster>(itemartworkjson);
            ////if (filterditemartwork != null && !string.IsNullOrEmpty(id))
            ////{
            ////    filterditemartwork.Items = filterditemartwork.Items.Where(x => Convert.ToInt32((x as ItemMasterArtworkTypeMappingMasterListItem).RequestID) == Convert.ToInt32(id)).ToList();
            ////    return JSONHelper.ToJSON<IMaster>(filterditemartwork);
            ////}
            ////else
            ////{
            ////    return null;
            ////}
        }

        /// <summary>
        /// Get Artwork Type Files By ID
        /// </summary>
        /// <param name="title">item id</param>
        /// <returns>string data</returns>
        public string GetArtworkTypeFilesByItemCode(string itemCode)
        {
            List spList = this.web.Lists.GetByTitle(ArtworkMasters.ITEMMASTERARTWORKTYPEMAPPINGMASTER);
            CamlQuery query = new CamlQuery();
            query.ViewXml = @"<View><Query><Where><And><Eq><FieldRef Name='ItemCode'  /><Value Type='text'>" + itemCode + @"</Value></Eq><Neq>
                                                                      <FieldRef Name='Status'   />
                                                                      <Value Type='Text'>" + TaskStatus.DELETED + @"</Value>
                                                                     </Neq></And></Where></Query></View>";
            ListItemCollection items = spList.GetItems(query);
            this.context.Load(items);
            this.context.ExecuteQuery();

            if (items != null && items.Count != 0)
            {
                List<ItemMasterArtworkTypeMappingMasterListItem> lstArtworkTypeMapping = new List<ItemMasterArtworkTypeMappingMasterListItem>();
                MasterDataHelper helper = new MasterDataHelper();
                IMaster objMaster = new ItemMasterArtworkTypeMappingMaster();
                objMaster = helper.AssignPropertyValues(objMaster, this.context, this.web, items, typeof(ItemMasterArtworkTypeMappingMasterListItem));
                lstArtworkTypeMapping = objMaster.Items.ConvertAll(o => (o as ItemMasterArtworkTypeMappingMasterListItem));
                return JSONHelper.ToJSON<List<ItemMasterArtworkTypeMappingMasterListItem>>(lstArtworkTypeMapping);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Search Direct into sharepoint item master.
        /// </summary>
        /// <param name="title">Searching Term</param>
        /// <returns>Item Search item</returns>
        public ItemSearchMaster SearchItemMasterData(string title)
        {
            try
            {
                ItemSearchMaster searchMaster = new ItemSearchMaster();
                searchMaster.Items = new List<IMasterItem>();
                List spList = this.web.Lists.GetByTitle(ArtworkMasters.ITEMMASTER);
                CamlQuery query = new CamlQuery();

                query.ViewXml = @"<View>
                                    <Query>
                                        <Where><Or>
                                            <Contains>
                                                <FieldRef Name='ItemCode' />
                                                <Value Type='Text'>" + title + @"</Value>
                                            </Contains>
                                             <Contains>
                                                <FieldRef Name='Model' />
                                                <Value Type='Text'>" + title + @"</Value>
                                            </Contains>
                                         </Or>
                                        </Where>
                                    </Query> <ViewFields><FieldRef Name='ItemCode' /><FieldRef Name='Model' /></ViewFields><RowLimit>15</RowLimit>
                                </View>";
                ListItemCollection items = spList.GetItems(query);
                this.context.Load(items);
                this.context.ExecuteQuery();
                if (items != null && items.Count != 0)
                {
                    ItemSearchMasterListItem obj;
                    foreach (ListItem item in items)
                    {
                        obj = new ItemSearchMasterListItem();
                        obj.Title = obj.ItemCode = Convert.ToString(item["ItemCode"]);
                        obj.ModelName = Convert.ToString(item["Model"]);
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
                List activityLogList = this.web.Lists.GetByTitle(ArtworkListNames.EXISTINGARTWORKACTIVITYLOG);
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
                List approvalMatrixList = this.web.Lists.GetByTitle(ArtworkListNames.EXISTINGARTWORKAPPAPPROVALMATRIX);
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

                List attachmentList = this.web.Lists.GetByTitle(ArtworkListNames.EXISTINGARTWORKATTACHMENTLIST);
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

                List mainList = this.web.Lists.GetByTitle(ArtworkListNames.EXISTINGARTWORKLIST);
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
                Logger.Error("Error while delete Existing Artwork Request : Message = " + ex.Message + ", Stack Trace = " + ex.StackTrace);
            }
            return status;
        }

        #endregion
    }
}