namespace BEL.ArtworkWorkflow.Controllers
{
    using BEL.CommonDataContract;
    using BEL.ArtworkWorkflow.BusinessLayer;
    using BEL.ArtworkWorkflow.Common;
    using BEL.ArtworkWorkflow.Models.Common;
    using BEL.ArtworkWorkflow.Models.NewArtwork;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using BEL.ArtworkWorkflow.Models.Master;
    using System.Web.Mvc.Html;
    using Newtonsoft.Json;

    /// <summary>
    /// New Artwork Controller
    /// </summary>
    public partial class NewArtworkController : NewArtworkBaseController
    {
        #region "Index"
        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="isRetrieve">The Is Retrieve.</param>
        /// <returns>
        /// Index View
        /// </returns>      
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Already Validate"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "artworkDetailSection", Justification = "Already vaildated")]
        [SharePointContextFilter]
        public ActionResult Index(int id = 0, bool isRetrieve = false)
        {
            ////if (id == 0 && !CommonBusinessLayer.Instance.IsCreator(this.CurrentUser.UserId))
            ////{
            ////    return this.RedirectToAction("NotAuthorize", "Master");
            ////}
            NewArtworkContract contract = null;
            Logger.Info("Start New Artwork form and ID = " + id);
            Dictionary<string, string> objDict = new Dictionary<string, string>();
            objDict.Add(Parameter.FROMNAME, ArtworkFormNames.NEWARTWORKFORM);
            objDict.Add(Parameter.ITEMID, id.ToString());
            objDict.Add(Parameter.USEREID, this.CurrentUser.UserId);
            Logger.Info("UserId = " + this.CurrentUser.UserId);
            objDict.Add("UserName", this.CurrentUser.FullName);
            ViewBag.UserID = this.CurrentUser.UserId;
            ViewBag.UserName = this.CurrentUser.FullName;
            contract = this.GetNewArtworkDetails(objDict);
            contract.UserDetails = this.CurrentUser;
            if (!isRetrieve)
            {
                if (contract != null && contract.Forms != null && contract.Forms.Count > 0)
                {
                    Logger.Info("Inside Loop inner");
                    NewArtworkDetailSection artworkDetailSection = contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ARTWORKDETAILSECTION) as NewArtworkDetailSection;
                    if (id == 0)
                    {
                        ApproverMaster approverlist = artworkDetailSection.MasterData.FirstOrDefault(x => x.NameOfMaster.Equals(Masters.APPROVERMASTER)) as ApproverMaster;
                        if (approverlist != null && approverlist.Items != null && approverlist.Items.Count > 0)
                        {
                            var approver = approverlist.Items.ToList();
                            List<Tuple<string, string>> lst = new List<Tuple<string, string>>();
                            foreach (var item in approver)
                            {
                                ApproverMasterListItem requester = item as ApproverMasterListItem;
                                if (!string.IsNullOrWhiteSpace(requester.Requester))
                                {
                                    string[] str = requester.Requester.Split(',');
                                    foreach (string strinner in str)
                                    {
                                        lst.Add(new Tuple<string, string>(strinner, requester.BusinessUnit));
                                    }
                                }
                            }
                            bool isValid = false;
                            foreach (var item in lst)
                            {
                                if (item.Item1 == this.CurrentUser.UserId)
                                {
                                    ////if (item.Item2 == ArtworkCommonConstant.CPBUSINESSUNITCODE || item.Item2 == ArtworkCommonConstant.LUMBUSINESSUNITCODE || item.Item2 == ArtworkCommonConstant.MRBUSINESSUNITCODE || item.Item2 == ArtworkCommonConstant.EXPBUSINESSUNITCODE)
                                    if (item.Item2 == ArtworkCommonConstant.CPBUSINESSUNITCODE || item.Item2 == ArtworkCommonConstant.KAPBUSINESSUNITCODE || item.Item2 == ArtworkCommonConstant.FANSBUSINESSUNITCODE || item.Item2 == ArtworkCommonConstant.DAPBUSINESSUNITCODE || item.Item2 == ArtworkCommonConstant.LIGHTBUSINESSUNITCODE || item.Item2 == ArtworkCommonConstant.LUMBUSINESSUNITCODE || item.Item2 == ArtworkCommonConstant.MRBUSINESSUNITCODE || item.Item2 == ArtworkCommonConstant.EXPBUSINESSUNITCODE)
                                    {
                                        artworkDetailSection.BusinessUnit = item.Item2;
                                        isValid = true;
                                        break;
                                    }
                                }
                            }
                            if (!isValid)
                            {
                                return this.RedirectToAction("NotAuthorize", "Master");
                            }
                        }
                        else
                        {
                            return this.RedirectToAction("NotAuthorize", "Master");
                        }
                    }
                    return this.View(contract);
                }
                else
                {
                    return this.RedirectToAction("NotAuthorize", "Master");
                }
            }
            else
            {
                if (contract != null && contract.Forms != null && contract.Forms.Count != 0)
                {
                    contract.Forms[0].FormStatus = string.Empty;
                    Button btn = new Button();
                    btn.Name = "Submit";
                    btn.ButtonStatus = ButtonActionStatus.NextApproval;
                    btn.JsFunction = "ConfirmSubmit";
                    btn.IsVisible = true;
                    btn.Icon = "fa fa-check";
                    contract.Forms[0].Buttons.Add(btn);
                    contract.Forms[0].Buttons.Remove(contract.Forms[0].Buttons.FirstOrDefault(f => f.ButtonStatus == ButtonActionStatus.Print));
                    NewArtworkDetailSection artworkDetailSection = contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ARTWORKDETAILSECTION) as NewArtworkDetailSection;
                    if (artworkDetailSection != null)
                    {
                        ////If Login user is not ABS Admin then error Unauthorized.

                        string[] strABSAdmin = artworkDetailSection.ABSAdmin.Split(',');
                        if (Array.IndexOf(strABSAdmin, this.CurrentUser.UserId) == -1)
                        {
                            return this.RedirectToAction("NotAuthorize", "Master");
                        }
                        artworkDetailSection.OldReferenceNo = artworkDetailSection.ReferenceNo;
                        artworkDetailSection.OldArtworkCreatedDate = artworkDetailSection.RequestDate;
                        ////artworkDetailSection.OldArtworkRejectedDate = artworkDetailSection.OldArtworkRejectedDate;
                        artworkDetailSection.OldRequestId = id;
                        artworkDetailSection.ProposedBy = CurrentUser.UserId;
                        artworkDetailSection.ProposedByName = CurrentUser.FullName;
                        artworkDetailSection.ReferenceNo = string.Empty;
                        artworkDetailSection.RequestDate = null;
                        artworkDetailSection.WorkflowStatus = string.Empty;
                        artworkDetailSection.IsActive = true;
                        artworkDetailSection.ListDetails[0].ItemId = 0;
                        artworkDetailSection.CurrentApprover.Approver = CurrentUser.UserId;
                        artworkDetailSection.CurrentApprover.ApproverName = CurrentUser.FullName;
                        artworkDetailSection.ApproversList.ForEach(p =>
                                {
                                    if (p.Role == ArtworkRoles.BUAPPROVER)
                                    {
                                        p.Comments = string.Empty;
                                        p.Status = "Not Assigned";
                                        p.AssignDate = null;
                                    }
                                });
                        ////  dcrDetailSection.ApproversList[0].Approver = dcrProcessIcSection.ApproversList[0].Approver;
                        if (artworkDetailSection.Files != null && artworkDetailSection.Files.Count > 0)
                        {
                            foreach (var item in artworkDetailSection.Files)
                            {
                                item.Status = FileStatus.New;
                            }
                        }
                        artworkDetailSection.FNLSamplePhoto = artworkDetailSection.Files != null && artworkDetailSection.Files.Count > 0 ? JsonConvert.SerializeObject(artworkDetailSection.Files) : string.Empty;
                    }
                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.BUHEADAPPROVERSECTION));

                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ADBLEVEL1SECTION));
                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ADBLEVEL2SECTION));

                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.SCMLEVEL1SECTION));
                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.SCMLEVEL2SECTION));

                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.QALEVEL1SECTION));
                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.QALEVEL2SECTION));

                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.MARKETINGLEVEL1SECTION));
                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.MARKETINGLEVEL2SECTION));

                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ABSQTEAMSECTION));
                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ABSQAPPROVERSECTION));

                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == SectionNameConstant.APPLICATIONSTATUS));
                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == SectionNameConstant.ACTIVITYLOG));
                }
                else
                {
                    return this.RedirectToAction("NotAuthorize", "Master");
                }
                return this.View(contract);
            }
        }
        #endregion

        #region "Save Artwork Detail Section"
        /// <summary>
        /// Saves the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// Content result
        /// </returns>
        [HttpPost]
        public ActionResult SaveArtworkDetailSection(NewArtworkDetailSection model)
        {
            ActionStatus status = new ActionStatus();
            if (!string.IsNullOrWhiteSpace(model.Product) && model.Product == "Import")
            {
                ModelState.Remove("VendorCode");
                ModelState.Remove("VendorAddress");
            }
            if (model != null && this.ValidateModelState(model))
            {
                status.Messages = new List<string>();
                ////if (!string.IsNullOrEmpty(model.UnitCartonDimensionL) && !string.IsNullOrEmpty(model.MasterCartondimensionL) && decimal.Parse(model.UnitCartonDimensionL) > decimal.Parse(model.MasterCartondimensionL))
                ////{
                ////    status.Messages.Add("Unit Carton lenght should not be greater than Master Carton lenght.");
                ////}
                ////if (!string.IsNullOrEmpty(model.UnitCartonDimensionW) && !string.IsNullOrEmpty(model.MasterCartondimensionW) && decimal.Parse(model.UnitCartonDimensionW) > decimal.Parse(model.MasterCartondimensionW))
                ////{
                ////    status.Messages.Add("Unit Carton width should not be greater than Master Carton width.");
                ////}
                ////if (!string.IsNullOrEmpty(model.UnitCartonDimensionH) && !string.IsNullOrEmpty(model.MasterCartondimensionH) && decimal.Parse(model.UnitCartonDimensionH) > decimal.Parse(model.MasterCartondimensionH))
                ////{
                ////    status.Messages.Add("Unit Carton height should not be greater than Master Carton height.");
                ////}
                ////if (!string.IsNullOrEmpty(model.NetWeight) && !string.IsNullOrEmpty(model.GrossWeight) && decimal.Parse(model.NetWeight) > decimal.Parse(model.GrossWeight))
                ////{
                ////    status.Messages.Add("Net Weight should not be greater than Gross Weight.");
                ////}
                if (status.Messages.Count != 0)
                {
                    status.IsSucceed = false;
                    return this.Json(status);
                }

                //// model.Files = FileListHelper.GenerateFileBytes(model.FileNameList);  //For Save Attachemennt
                model.Files = new List<FileDetails>();
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLSamplePhoto));
                model.FNLSamplePhoto = string.Join(",", FileListHelper.GetFileNames(model.FNLSamplePhoto));
                if (model.Product == "Import")
                {
                    model.VendorCode = string.Empty;
                    model.VendorAddress = string.Empty;
                }
                Dictionary<string, string> objDict = this.GetSaveDataDictionary(this.CurrentUser.UserId, model.ActionStatus.ToString(), model.ButtonCaption);
                status = this.SaveSection(model, objDict);
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

        #region "Save New Artwork BU Head Section"
        /// <summary>
        /// The New Artwork BU Head Approver Section
        /// </summary>
        /// <param name="model">The section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveBUHeadSection(NewArtworkBUHeadApproverSection model)
        {
            ActionStatus status = new ActionStatus();

            if (model != null && this.ValidateModelState(model))
            {
                Dictionary<string, string> objDict = this.GetSaveDataDictionary(this.CurrentUser.UserId, model.ActionStatus.ToString(), model.ButtonCaption);
                if (!string.IsNullOrEmpty(model.SendBackTo))
                {
                    objDict[Parameter.SENDTOLEVEL] = model.SendBackTo;
                }
                if (model.ActionStatus == ButtonActionStatus.Rejected)
                {
                    model.OldArtworkRejectedDate = DateTime.Now;
                    model.OldArtworkRejectedComment = model.CurrentApprover.Comments;
                }
                status = this.SaveSection(model, objDict);
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

        #region "Save New Artwork ABSQ Team Section"
        /// <summary>
        /// save ABSQ section
        /// </summary>
        /// <param name="model">ABSQ Section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveABSQTeamSection(NewArtworkABSQTeamSection model)
        {
            ActionStatus status = new ActionStatus();
            if (model != null && this.ValidateModelState(model))
            {
                if (model.ArtworkAttachmentTempList != null && model.ArtworkAttachmentTempList.Count != 0)
                {
                    model.ArtworkAttachmentTempList.ForEach(p =>
                    {
                        if (p.FileNameList != null)
                        {
                            p.Files = new List<FileDetails>();
                            p.FileName = string.Join(",", FileListHelper.GetFileNamesForABSQTeam(p.FileNameList, model.ItemCode, p.ArtworkTypeCode));
                            p.Files.AddRange(FileListHelper.GenerateFileBytesForABSQTeam(p.FileNameList, model.ItemCode, p.ArtworkTypeCode));
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
                model.ArtworkAttachmentList = new List<ITrans>();
                model.ArtworkAttachmentList.AddRange(model.ArtworkAttachmentTempList);
                model.Files = new List<FileDetails>();
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLTechnicalSpecificationAttachment));
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLQAPAttachment));
                model.FNLTechnicalSpecificationAttachment = string.Join(",", FileListHelper.GetFileNames(model.FNLTechnicalSpecificationAttachment));
                model.FNLQAPAttachment = string.Join(",", FileListHelper.GetFileNames(model.FNLQAPAttachment));
                Dictionary<string, string> objDict = this.GetSaveDataDictionary(this.CurrentUser.UserId, model.ActionStatus.ToString(), model.ButtonCaption);
                status = this.SaveSection(model, objDict);
                status = this.GetMessage(status, ResourceNames.Artwork);
            }
            else
            {
                status.IsSucceed = false;
                status.Messages = this.GetErrorMessage(System.Web.Mvc.Html.ResourceNames.Artwork);
            }
            return this.Json(status);
        }
        #endregion

        #region "Save New Artwork ABSQ Approver Section"
        /// <summary>
        /// Save ABSQ Approver Section
        /// </summary>
        /// <param name="NewArtworkABSQApproverSection">New Artwork ABSQ Approver Section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveABSQApproverSection(NewArtworkABSQApproverSection model)
        {
            ActionStatus status = new ActionStatus();

            if (model != null && this.ValidateModelState(model))
            {
                Dictionary<string, string> objDict = this.GetSaveDataDictionary(this.CurrentUser.UserId, model.ActionStatus.ToString(), model.ButtonCaption);
                if (!string.IsNullOrEmpty(model.SendBackTo))
                {
                    objDict[Parameter.SENDTOLEVEL] = model.SendBackTo;
                }
                status = this.SaveSection(model, objDict);
                status = this.GetMessage(status, ResourceNames.Artwork);
            }
            else
            {
                status.IsSucceed = false;
                status.Messages = this.GetErrorMessage(System.Web.Mvc.Html.ResourceNames.Artwork);
            }
            return this.Json(status);
        }

        #endregion

        #region "Save New Artwork ADB Level1 Section"
        /// <summary>
        /// The New Artwork ADB Level1 Section
        /// </summary>
        /// <param name="NewArtworkADBLevel1Section">New Artwork ADB Level1 Section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveADBLevel1Section(NewArtworkADBLevel1Section model)
        {
            ActionStatus status = new ActionStatus();

            if (model != null && this.ValidateModelState(model))
            {
                model.Files = new List<FileDetails>();
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLADBLevel1Attachment));
                model.FNLADBLevel1Attachment = string.Join(",", FileListHelper.GetFileNames(model.FNLADBLevel1Attachment));
                Dictionary<string, string> objDict = this.GetSaveDataDictionary(this.CurrentUser.UserId, model.ActionStatus.ToString(), model.ButtonCaption);
                status = this.SaveSection(model, objDict);
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

        #region "Save New Artwork ADB Level2 Section"
        /// <summary>
        /// The New Artwork ADB Level2 Section
        /// </summary>
        /// <param name="model">The section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveADBLevel2Section(NewArtworkADBLevel2Section model)
        {
            ActionStatus status = new ActionStatus();

            if (model != null && this.ValidateModelState(model))
            {
                model.Files = new List<FileDetails>();
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLADBLevel2Attachment));
                model.FNLADBLevel2Attachment = string.Join(",", FileListHelper.GetFileNames(model.FNLADBLevel2Attachment));
                Dictionary<string, string> objDict = this.GetSaveDataDictionary(this.CurrentUser.UserId, model.ActionStatus.ToString(), model.ButtonCaption);
                if (!string.IsNullOrEmpty(model.SendBackTo))
                {
                    objDict[Parameter.SENDTOLEVEL] = model.SendBackTo;
                }
                objDict.Add("UserName", this.CurrentUser.FullName);
                status = this.SaveSection(model, objDict);
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

        #region "Save New Artwork QA Level 1 Section"
        /// <summary>
        /// The New Artwork QA Level1 Section
        /// </summary>
        /// <param name="model">The section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveQALevel1Section(NewArtworkQALevel1Section model)
        {
            ActionStatus status = new ActionStatus();

            if (model != null && this.ValidateModelState(model))
            {
                model.Files = new List<FileDetails>();
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLQALevel1Attachment));
                model.FNLQALevel1Attachment = string.Join(",", FileListHelper.GetFileNames(model.FNLQALevel1Attachment));
                Dictionary<string, string> objDict = this.GetSaveDataDictionary(this.CurrentUser.UserId, model.ActionStatus.ToString(), model.ButtonCaption);
                status = this.SaveSection(model, objDict);
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

        #region "Save New Artwork QA Level 2 Section"
        /// <summary>
        /// The New Artwork QA Level2 Section
        /// </summary>
        /// <param name="model">The section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveQALevel2Section(NewArtworkQALevel2Section model)
        {
            ActionStatus status = new ActionStatus();

            if (model != null && this.ValidateModelState(model))
            {
                model.Files = new List<FileDetails>();
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLQALevel2Attachment));
                model.FNLQALevel2Attachment = string.Join(",", FileListHelper.GetFileNames(model.FNLQALevel2Attachment));
                Dictionary<string, string> objDict = this.GetSaveDataDictionary(this.CurrentUser.UserId, model.ActionStatus.ToString(), model.ButtonCaption);
                if (!string.IsNullOrEmpty(model.SendBackTo))
                {
                    objDict[Parameter.SENDTOLEVEL] = model.SendBackTo;
                }
                objDict.Add("UserName", this.CurrentUser.FullName);
                status = this.SaveSection(model, objDict);
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

        #region "Save New Artwork SCM Level 1 Section"
        /// <summary>
        /// The New Artwork SCM Level1 Section
        /// </summary>
        /// <param name="model">The section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveSCMLevel1Section(NewArtworkSCMLevel1Section model)
        {
            ActionStatus status = new ActionStatus();
            if (model != null && this.ValidateModelState(model))
            {
                model.Files = new List<FileDetails>();
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLSCMLevel1Attachment));
                model.FNLSCMLevel1Attachment = string.Join(",", FileListHelper.GetFileNames(model.FNLSCMLevel1Attachment));
                Dictionary<string, string> objDict = this.GetSaveDataDictionary(this.CurrentUser.UserId, model.ActionStatus.ToString(), model.ButtonCaption);
                status = this.SaveSection(model, objDict);
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

        #region "Save New Artwork SCM Level 2 Section"
        /// <summary>
        /// The New Artwork SCM Level2 Section
        /// </summary>
        /// <param name="model">The section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveSCMLevel2Section(NewArtworkSCMLevel2Section model)
        {
            ActionStatus status = new ActionStatus();
            if (model != null && this.ValidateModelState(model))
            {
                model.Files = new List<FileDetails>();
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLSCMLevel2Attachment));
                model.FNLSCMLevel2Attachment = string.Join(",", FileListHelper.GetFileNames(model.FNLSCMLevel2Attachment));
                Dictionary<string, string> objDict = this.GetSaveDataDictionary(this.CurrentUser.UserId, model.ActionStatus.ToString(), model.ButtonCaption);
                if (!string.IsNullOrEmpty(model.SendBackTo))
                {
                    objDict[Parameter.SENDTOLEVEL] = model.SendBackTo;
                }
                objDict.Add("UserName", this.CurrentUser.FullName);
                status = this.SaveSection(model, objDict);
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

        #region "Save New Artwork Marketing Level 1 Section"
        /// <summary>
        /// The New Artwork Marketing Level1 Section
        /// </summary>
        /// <param name="model">The section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveMarketingLevel1Section(NewArtworkMarketingLevel1Section model)
        {
            ActionStatus status = new ActionStatus();

            if (model != null && this.ValidateModelState(model))
            {
                model.Files = new List<FileDetails>();
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLMarketingLevel1Attachment));
                model.FNLMarketingLevel1Attachment = string.Join(",", FileListHelper.GetFileNames(model.FNLMarketingLevel1Attachment));
                Dictionary<string, string> objDict = this.GetSaveDataDictionary(this.CurrentUser.UserId, model.ActionStatus.ToString(), model.ButtonCaption);
                status = this.SaveSection(model, objDict);
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

        #region "Save New Artwork Marketing Level 2 Section"
        /// <summary>
        /// The New Artwork Marketing Level2 Section
        /// </summary>
        /// <param name="model">The section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveMarketingLevel2Section(NewArtworkMarketingLevel2Section model)
        {
            ActionStatus status = new ActionStatus();

            if (model != null && this.ValidateModelState(model))
            {
                model.Files = new List<FileDetails>();
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLMarketingLevel2Attachment));
                model.FNLMarketingLevel2Attachment = string.Join(",", FileListHelper.GetFileNames(model.FNLMarketingLevel2Attachment));
                Dictionary<string, string> objDict = this.GetSaveDataDictionary(this.CurrentUser.UserId, model.ActionStatus.ToString(), model.ButtonCaption);
                if (!string.IsNullOrEmpty(model.SendBackTo))
                {
                    objDict[Parameter.SENDTOLEVEL] = model.SendBackTo;
                }
                objDict.Add("UserName", this.CurrentUser.FullName);
                status = this.SaveSection(model, objDict);
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

        /// <summary>
        /// Keep Session Alive
        /// </summary>
        /// <returns>json value</returns>
        [System.Web.Mvc.HttpPost]
        public JsonResult KeepSessionAlive()
        {
            return new JsonResult { Data = "Success" };
        }

        /// <summary>
        /// Gets all vendors.
        /// </summary>
        /// <param name="q">The q.</param>
        /// <returns>Json Result</returns>
        public JsonResult GetAllVendors(string q)
        {
            string data = this.GetAllVendorCode(q);
            if (!string.IsNullOrEmpty(data))
            {
                var master = BEL.DataAccessLayer.JSONHelper.ToObject<VendorMaster>(data);
                if (master != null)
                {
                    return this.Json((from item in master.Items select new { id = (item as VendorMasterListItem).VendorCode + "-" + (item as VendorMasterListItem).VendorName, name = (item as VendorMasterListItem).VendorCode + "-" + (item as VendorMasterListItem).VendorName }).ToList(), JsonRequestBehavior.AllowGet);
                }
            }
            return this.Json(null, JsonRequestBehavior.AllowGet);
        }

        #region Delete Artwork

        /// <summary>
        /// Deletes the arwork.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Json Result</returns>
        [HttpPost]
        public JsonResult DeleteArwork(int id)
        {
            ActionStatus status = new ActionStatus();
            status = this.DeleteArworkById(id);
            status = this.GetMessage(status, System.Web.Mvc.Html.ResourceNames.Artwork);
            return this.Json(status);
        }

        #endregion
    }
}