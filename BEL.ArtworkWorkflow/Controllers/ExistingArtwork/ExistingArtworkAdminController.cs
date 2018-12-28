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
    using BEL.ArtworkWorkflow.Models.ExistingArtwork;

    /// <summary>
    /// Existing Artwork Controller
    /// </summary>
    public partial class ExistingArtworkController : ExistingArtworkBaseController
    {
        #region "Index"
        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// Index View
        /// </returns>      
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "NA"), SharePointContextFilter]
        public ActionResult ExistingArtworkAdminIndex(int id = 0)
        {
            if (id > 0 && CommonBusinessLayer.Instance.IsAdminUser(this.CurrentUser.UserId))
            {
                ExistingArtworkContract contract = null;
                Logger.Info("Start Artwork Admin form and ID = " + id);
                Dictionary<string, string> objDict = new Dictionary<string, string>();
                objDict.Add(Parameter.FROMNAME, ArtworkFormNames.EXISTINGARTWORKADMINFORM);
                objDict.Add(Parameter.ITEMID, id.ToString());
                objDict.Add(Parameter.USEREID, this.CurrentUser.UserId);
                ViewBag.UserID = this.CurrentUser.UserId;
                ViewBag.UserName = this.CurrentUser.FullName;
                contract = this.GetExistingArtworkAdminDetails(objDict);
                contract.UserDetails = this.CurrentUser;

                if (contract != null && contract.Forms != null && contract.Forms.Count > 0)
                {
                    ExistingArtworkAdminDetailSection existingArtworkDetailSection = contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ARTWORKADMINDETAILSECTION) as ExistingArtworkAdminDetailSection;
                    ApplicationStatusSection newArtworkApprovalSection = contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == SectionNameConstant.APPLICATIONSTATUS) as ApplicationStatusSection;

                    if (newArtworkApprovalSection != null)
                    {
                        existingArtworkDetailSection.ApproversList = newArtworkApprovalSection.ApplicationStatusList;
                        existingArtworkDetailSection.RequesterComment = existingArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.CREATOR).Comments;
                        existingArtworkDetailSection.BUHeadComments = existingArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.BUAPPROVER).Comments;
                        existingArtworkDetailSection.ABSQTeamComments = existingArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.ABSQTEAM).Comments;
                        existingArtworkDetailSection.ABSQApproverComments = existingArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.ABSQAPPROVER).Comments;

                        existingArtworkDetailSection.ADBLevel1Comments = existingArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.ADBLEVEL1APPROVER) != null ? existingArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.ADBLEVEL1APPROVER).Comments : string.Empty;
                        existingArtworkDetailSection.ADBLevel2Comments = existingArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.ADBLEVEL2APPROVER) != null ? existingArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.ADBLEVEL2APPROVER).Comments : string.Empty;

                        existingArtworkDetailSection.QALevel1Comments = existingArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.QALEVEL1APPROVER) != null ? existingArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.QALEVEL1APPROVER).Comments : string.Empty;
                        existingArtworkDetailSection.QALevel2Comments = existingArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.QALEVEL2APPROVER) != null ? existingArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.QALEVEL2APPROVER).Comments : string.Empty;

                        existingArtworkDetailSection.SCMLevel1Comments = existingArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.SCMLEVEL1APPROVER) != null ? existingArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.SCMLEVEL1APPROVER).Comments : string.Empty;
                        existingArtworkDetailSection.SCMLevel2Comments = existingArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.SCMLEVEL2APPROVER) != null ? existingArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.SCMLEVEL2APPROVER).Comments : string.Empty;

                        existingArtworkDetailSection.MarketingLevel1Comments = existingArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.MARKETINGLEVEL1APPROVER) != null ? existingArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.MARKETINGLEVEL1APPROVER).Comments : string.Empty;
                        existingArtworkDetailSection.MarketingLevel2Comments = existingArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.MARKETINGLEVEL2APPROVER) != null ? existingArtworkDetailSection.ApproversList.FirstOrDefault(p => p.Role == ArtworkRoles.MARKETINGLEVEL2APPROVER).Comments : string.Empty;
                    }
                    return this.View("ExistingArtworkAdmin/ExistingArtworkAdminIndex", contract);
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

        #region "Save Existing Artwork Detail Admin Section"
        /// <summary>
        /// Saves the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// Content result
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "NA"), HttpPost]
        public ActionResult SaveArtworkAdminDetailSection(ExistingArtworkAdminDetailSection model)
        {
            ActionStatus status = new ActionStatus();
            if (model != null)
            {
                if (model.Product == "Import")
                {
                    ModelState.Remove("VendorCode");
                    ModelState.Remove("VendorAddress");
                }
            }
            ////ModelState.Remove("FileNameList");
            if (model != null && ModelState.IsValid)
            {
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
                ////foreach (ModelState modelState in ViewData.ModelState.Values)
                ////{
                ////    foreach (ModelError error in modelState.Errors)
                ////    {
                ////        Console.WriteLine("Hi");
                ////    }
                ////}
                status.IsSucceed = false;
                status.Messages = this.GetErrorMessage(System.Web.Mvc.Html.ResourceNames.Artwork);
            }
            return this.Json(status);
        }
        #endregion
    }
}