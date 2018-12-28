namespace BEL.ArtworkWorkflow.Controllers
{
    using BEL.CommonDataContract;
    using BEL.ArtworkWorkflow.BusinessLayer;
    using BEL.ArtworkWorkflow.Common;
    using BEL.ArtworkWorkflow.Models.Common;
    using BEL.ArtworkWorkflow.Models.NewArtwork;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// Existing Artwork Admin Controller
    /// </summary>
    public partial class NewArtworkController : NewArtworkBaseController
    {
        #region "Index"
        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// Index View
        /// </returns>      
        [SharePointContextFilter]
        public ActionResult NewArtworkAdminIndex(int id = 0)
        {
            if (id > 0 && CommonBusinessLayer.Instance.IsAdminUser(this.CurrentUser.UserId))
            {
                NewArtworkContract contract = null;
                Logger.Info("Start Artwork Admin form and ID = " + id);
                Dictionary<string, string> objDict = new Dictionary<string, string>();
                objDict.Add(Parameter.FROMNAME, ArtworkFormNames.NEWARTWORKADMINFORM);
                objDict.Add(Parameter.ITEMID, id.ToString());
                objDict.Add(Parameter.USEREID, this.CurrentUser.UserId);
                ViewBag.UserID = this.CurrentUser.UserId;
                ViewBag.UserName = this.CurrentUser.FullName;
                contract = this.GetNewArtworkAdminDetails(objDict);
                contract.UserDetails = this.CurrentUser;

                if (contract != null && contract.Forms != null && contract.Forms.Count > 0)
                {
                    NewArtworkAdminDetailSection newArtworkDetailSection = contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ARTWORKADMINDETAILSECTION) as NewArtworkAdminDetailSection;
                    ApplicationStatusSection newArtworkApprovalSection = contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == SectionNameConstant.APPLICATIONSTATUS) as ApplicationStatusSection;

                    if (newArtworkApprovalSection != null)
                    {
                        newArtworkDetailSection.ApproversList = newArtworkApprovalSection.ApplicationStatusList;
                        newArtworkDetailSection.RequesterComment = newArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.CREATOR).Comments;
                        newArtworkDetailSection.BUHeadComments = newArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.BUAPPROVER).Comments;
                        newArtworkDetailSection.ABSQTeamComments = newArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.ABSQTEAM).Comments;
                        newArtworkDetailSection.ABSQApproverComments = newArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.ABSQAPPROVER).Comments;

                        newArtworkDetailSection.ADBLevel1Comments = newArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.ADBLEVEL1APPROVER) != null ? newArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.ADBLEVEL1APPROVER).Comments : string.Empty;
                        newArtworkDetailSection.ADBLevel2Comments = newArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.ADBLEVEL2APPROVER) != null ? newArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.ADBLEVEL2APPROVER).Comments : string.Empty;

                        newArtworkDetailSection.QALevel1Comments = newArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.QALEVEL1APPROVER) != null ? newArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.QALEVEL1APPROVER).Comments : string.Empty;
                        newArtworkDetailSection.QALevel2Comments = newArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.QALEVEL2APPROVER) != null ? newArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.QALEVEL2APPROVER).Comments : string.Empty;

                        newArtworkDetailSection.SCMLevel1Comments = newArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.SCMLEVEL1APPROVER) != null ? newArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.SCMLEVEL1APPROVER).Comments : string.Empty;
                        newArtworkDetailSection.SCMLevel2Comments = newArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.SCMLEVEL2APPROVER) != null ? newArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.SCMLEVEL2APPROVER).Comments : string.Empty;

                        newArtworkDetailSection.MarketingLevel1Comments = newArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.MARKETINGLEVEL1APPROVER) != null ? newArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.MARKETINGLEVEL1APPROVER).Comments : string.Empty;
                        newArtworkDetailSection.MarketingLevel2Comments = newArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.MARKETINGLEVEL2APPROVER) != null ? newArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.MARKETINGLEVEL2APPROVER).Comments : string.Empty;
                    }
                    return this.View("NewArtworkAdmin/NewArtworkAdminIndex", contract);
                }
                else
                {
                    return this.RedirectToAction("NotAuthorize", "Master");
                }
            }
            else
            {
                return this.RedirectToAction("NotAuthorize", "Master");
            }
        }
        #endregion

        #region "Save Feedbacks Detail Admin Section"
        /// <summary>
        /// Saves the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// Content result
        /// </returns>
        [HttpPost]
        public ActionResult SaveArtworkAdminDetailSection(NewArtworkAdminDetailSection model)
        {
            ActionStatus status = new ActionStatus();
            if (model.Product == "Import")
            {
                ModelState.Remove("VendorCode");
                ModelState.Remove("VendorAddress");
            }
            if (model != null && ModelState.IsValid)
            {
                status.Messages = new List<string>();
                if (!string.IsNullOrEmpty(model.UnitCartonDimensionL) && !string.IsNullOrEmpty(model.MasterCartondimensionL) && decimal.Parse(model.UnitCartonDimensionL) > decimal.Parse(model.MasterCartondimensionL))
                {
                    status.Messages.Add("Unit Carton lenght should not be greater than Master Carton lenght.");
                }
                if (!string.IsNullOrEmpty(model.UnitCartonDimensionW) && !string.IsNullOrEmpty(model.MasterCartondimensionW) && decimal.Parse(model.UnitCartonDimensionW) > decimal.Parse(model.MasterCartondimensionW))
                {
                    status.Messages.Add("Unit Carton width should not be greater than Master Carton width.");
                }
                if (!string.IsNullOrEmpty(model.UnitCartonDimensionH) && !string.IsNullOrEmpty(model.MasterCartondimensionH) && decimal.Parse(model.UnitCartonDimensionH) > decimal.Parse(model.MasterCartondimensionH))
                {
                    status.Messages.Add("Unit Carton height should not be greater than Master Carton height.");
                }
                if (!string.IsNullOrEmpty(model.NetWeight) && !string.IsNullOrEmpty(model.GrossWeight) && decimal.Parse(model.NetWeight) > decimal.Parse(model.GrossWeight))
                {
                    status.Messages.Add("Net Weight should not be greater than Gross Weight.");
                }
                if (status.Messages.Count != 0)
                {
                    status.IsSucceed = false;
                    return this.Json(status);
                }

                //// model.Files = FileListHelper.GenerateFileBytes(model.FileNameList);  //For Save Attachemennt
                model.Files = new List<FileDetails>();
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLSamplePhoto));
                model.FNLSamplePhoto = string.Join(",", FileListHelper.GetFileNames(model.FNLSamplePhoto));
                ////if (string.IsNullOrWhiteSpace(model.FNLSamplePhoto))
                ////{
                ////    status.IsSucceed = false;
                ////    status.Messages.Add("Error_FNLSamplePhoto");
                ////    status = this.GetMessage(status, System.Web.Mvc.Html.ResourceNames.Artwork);
                ////    return this.Json(status);
                ////}
                if (model.Product == "Import")
                {
                    model.VendorCode = string.Empty;
                    model.VendorAddress = string.Empty;
                }
                if (model.ApproversList != null)
                {
                    model.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.CREATOR).Comments = model.RequesterComment;
                    model.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.BUAPPROVER).Comments = model.BUHeadComments;
                    model.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.ABSQTEAM).Comments = model.ABSQTeamComments;
                    model.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.ABSQAPPROVER).Comments = model.ABSQApproverComments;
                    model.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.QALEVEL1APPROVER).Comments = model.QALevel1Comments;
                    model.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.QALEVEL2APPROVER).Comments = model.QALevel2Comments;
                    model.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.SCMLEVEL1APPROVER).Comments = model.SCMLevel1Comments;
                    model.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.SCMLEVEL2APPROVER).Comments = model.SCMLevel2Comments;
                    model.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.ADBLEVEL1APPROVER).Comments = model.ADBLevel1Comments;
                    model.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.ADBLEVEL2APPROVER).Comments = model.ADBLevel2Comments;
                    model.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.MARKETINGLEVEL1APPROVER).Comments = model.MarketingLevel1Comments;
                    model.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.MARKETINGLEVEL2APPROVER).Comments = model.MarketingLevel2Comments;
                }
                bool isValid = true;
                if (model.ArtworkAttachmentTempList != null && model.ArtworkAttachmentTempList.Count != 0)
                {
                    model.ArtworkAttachmentTempList.ForEach(p =>
                    {
                        if (p.FileNameList != null)
                        {
                            p.Files = new List<FileDetails>();
                            p.FileName = string.Join(",", FileListHelper.GetFileNames(p.FileNameList));
                            p.Files.AddRange(FileListHelper.GenerateFileBytes(p.FileNameList));
                            if (p.Files.Count == 1 && p.Files.Where(x => x.Status == FileStatus.Delete).Count() == 1)
                            {
                                isValid = false;
                            }
                            ////if (p.Files.Count == 2 && p.Files.Where(x => x.Status == FileStatus.Delete).Count() == 1)
                            ////{
                            ////    status.IsSucceed = false;
                            ////    status.Messages = this.GetErrorMessage(System.Web.Mvc.Html.ResourceNames.Artwork);
                            ////}
                            if (p.ID > 0)
                            {
                                p.ItemAction = ItemActionStatus.UPDATED;
                            }
                            else
                            {
                                p.ItemAction = ItemActionStatus.NEW;
                            }
                        }
                    });
                }
                if (!isValid)
                {
                    status.IsSucceed = false;
                    status.Messages.Add("Text_AdminUserdelete");
                    status = this.GetMessage(status, System.Web.Mvc.Html.ResourceNames.Artwork);
                    return this.Json(status);
                }
                model.ArtworkAttachmentList = new List<ITrans>();
                model.ArtworkAttachmentList.AddRange(model.ArtworkAttachmentTempList);
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLTechnicalSpecificationAttachment));
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLQAPAttachment));
                model.FNLTechnicalSpecificationAttachment = string.Join(",", FileListHelper.GetFileNames(model.FNLTechnicalSpecificationAttachment));
                model.FNLQAPAttachment = string.Join(",", FileListHelper.GetFileNames(model.FNLQAPAttachment));
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLADBLevel1Attachment));
                model.FNLADBLevel1Attachment = string.Join(",", FileListHelper.GetFileNames(model.FNLADBLevel1Attachment));
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLADBLevel2Attachment));
                model.FNLADBLevel2Attachment = string.Join(",", FileListHelper.GetFileNames(model.FNLADBLevel2Attachment));
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLQALevel1Attachment));
                model.FNLQALevel1Attachment = string.Join(",", FileListHelper.GetFileNames(model.FNLQALevel1Attachment));
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLQALevel2Attachment));
                model.FNLQALevel2Attachment = string.Join(",", FileListHelper.GetFileNames(model.FNLQALevel2Attachment));
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLSCMLevel1Attachment));
                model.FNLSCMLevel1Attachment = string.Join(",", FileListHelper.GetFileNames(model.FNLSCMLevel1Attachment));
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLSCMLevel2Attachment));
                model.FNLSCMLevel2Attachment = string.Join(",", FileListHelper.GetFileNames(model.FNLSCMLevel2Attachment));
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLMarketingLevel1Attachment));
                model.FNLMarketingLevel1Attachment = string.Join(",", FileListHelper.GetFileNames(model.FNLMarketingLevel1Attachment));
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLMarketingLevel2Attachment));
                model.FNLMarketingLevel2Attachment = string.Join(",", FileListHelper.GetFileNames(model.FNLMarketingLevel2Attachment));
                Dictionary<string, string> objDict = this.GetSaveDataDictionary(this.CurrentUser.UserId, model.ActionStatus.ToString(), model.ButtonCaption);
                status = this.SaveAdminDetailSection(model, objDict);
                status = this.GetMessage(status, System.Web.Mvc.Html.ResourceNames.Artwork);
            }
            else
            {
                status.IsSucceed = false;
                status.Messages = this.GetErrorMessage(System.Web.Mvc.Html.ResourceNames.Artwork);
            }
            return this.Json(status);
        }
        #endregion
    }
}