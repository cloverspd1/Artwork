namespace BEL.ArtworkWorkflow.Controllers
{
    using BEL.CommonDataContract;
    using BEL.ArtworkWorkflow.BusinessLayer;
    using BEL.ArtworkWorkflow.Common;
    using BEL.ArtworkWorkflow.Models.Common;
    using BEL.ArtworkWorkflow.Models.ExistingArtwork;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using BEL.ArtworkWorkflow.Models.Master;
    using Newtonsoft.Json;

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
        /// <param name="isRetrieve">The Is Retrieve.</param>
        /// <returns>
        /// Index View
        /// </returns>      
        [SharePointContextFilter]
        public ActionResult Index(int id = 0, bool isRetrieve = false)
        {
            ////if (id == 0 && !CommonBusinessLayer.Instance.IsCreator(this.CurrentUser.UserId))
            ////{
            ////    return this.RedirectToAction("NotAuthorize", "Master");
            ////}

            ExistingArtworkContract contract = null;
            Logger.Info("Start NPD form and ID = " + id);
            Dictionary<string, string> objDict = new Dictionary<string, string>();
            objDict.Add(Parameter.FROMNAME, ArtworkFormNames.EXISTINGARTWORKFORM);
            objDict.Add(Parameter.ITEMID, id.ToString());
            objDict.Add(Parameter.USEREID, this.CurrentUser.UserId);
            objDict.Add("UserName", this.CurrentUser.FullName);
            ViewBag.UserID = this.CurrentUser.UserId;
            ViewBag.UserName = this.CurrentUser.FullName;
            contract = this.GetNewArtworkDetails(objDict);
            contract.UserDetails = this.CurrentUser;
            if (!isRetrieve)
            {
                if (contract != null && contract.Forms != null && contract.Forms.Count > 0)
                {
                    var sectionDetails = contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ARTWORKDETAILSECTION) as ExistingArtworkDetailSection;
                    if (string.IsNullOrWhiteSpace(sectionDetails.RequesterBusinessUnit))
                    {
                        return this.RedirectToAction("NotAuthorize", "Master");
                    }
                    if (sectionDetails != null && sectionDetails.IsActive)
                    {
                        this.SetTempData<List<ExistingArtworkAttachment>>(TempKeys.ArtworkAttachmentMaster.ToString() + "_" + id, sectionDetails.TempExistingAttachment);
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
                    ExistingArtworkDetailSection artworkDetailSection = contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ARTWORKDETAILSECTION) as ExistingArtworkDetailSection;
                    if (artworkDetailSection != null)
                    {
                        ////If Login user is not ABS Admin then error Unauthorized.

                        string[] strABSAdmin = artworkDetailSection.ABSAdmin.Split(',');
                        int pos = Array.IndexOf(strABSAdmin, this.CurrentUser.UserId);
                        if (pos == -1)
                        {
                            return this.RedirectToAction("NotAuthorize", "Master");
                        }
                        artworkDetailSection.OldReferenceNo = artworkDetailSection.ReferenceNo;
                        artworkDetailSection.OldArtworkCreatedDate = artworkDetailSection.RequestDate;
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
                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ABSQTEAMSECTION));
                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ABSQAPPROVERSECTION));

                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ADBLEVEL1SECTION));
                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.ADBLEVEL2SECTION));

                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.SCMLEVEL1SECTION));
                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.SCMLEVEL2SECTION));

                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.QALEVEL1SECTION));
                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.QALEVEL2SECTION));

                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.MARKETINGLEVEL1SECTION));
                    contract.Forms[0].SectionsList.Remove(contract.Forms[0].SectionsList.FirstOrDefault(f => f.SectionName == ArtworkSectionName.MARKETINGLEVEL2SECTION));

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
        public ActionResult SaveExistingArtworkDetailSection(ExistingArtworkDetailSection model)
        {
            ActionStatus status = new ActionStatus();
            if (model != null && model.Product == "Import")
            {
                ModelState.Remove("VendorCode");
                ModelState.Remove("VendorAddress");
                model.VendorCode = model.VendorAddress = string.Empty;
            }
            if (model != null && this.ValidateModelState(model))
            {
                model.Files = new List<FileDetails>();
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLSamplePhoto));
                model.FNLSamplePhoto = string.Join(",", FileListHelper.GetFileNames(model.FNLSamplePhoto));

                if (model.ActionStatus == ButtonActionStatus.NextApproval)
                {
                    ////var list = this.GetTempData<List<ExistingArtworkAttachment>>(TempKeys.ArtworkAttachmentMaster.ToString() + "_" + GetFormIdFromUrl());
                    ////list.ForEach(p =>
                    ////{
                    ////    p.Files.AddRange(FileListHelper.GenerateFileBytes(p.FileNameList));
                    ////});
                    ////model.ExistingAttachment = new List<ITrans>();
                    ////model.ExistingAttachment.AddRange(list);
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

        #region "Save Existing Artwork BU Head Section"
        /// <summary>
        /// The Existing Artwork BU Head Approver Section
        /// </summary>
        /// <param name="model">The section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveBUHeadSection(ExistingArtworkBUHeadApproverSection model)
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

        #region "Save Existing Artwork ADB Level1 Section"
        /// <summary>
        /// The Existing Artwork ADB Level1 Section
        /// </summary>
        /// <param name="model">The section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveADBLevel1Section(ExistingArtworkADBLevel1Section model)
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

        #region "Save Existing Artwork ADB Level2 Section"
        /// <summary>
        /// The Existing Artwork ADB Level2 Section
        /// </summary>
        /// <param name="model">The section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveADBLevel2Section(ExistingArtworkADBLevel2Section model)
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

        #region "Save Existing Artwork Marketing Level 1 Section"
        /// <summary>
        /// The Existing Artwork Marketing Level1 Section
        /// </summary>
        /// <param name="model">The section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveMarketingLevel1Section(ExistingArtworkMarketingLevel1Section model)
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

        #region "Save Existing Artwork Marketing Level 2 Section"
        /// <summary>
        /// The Existing Artwork Marketing Level2 Section
        /// </summary>
        /// <param name="model">The section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveMarketingLevel2Section(ExistingArtworkMarketingLevel2Section model)
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

        #region "Save Existing Artwork QA Level 1 Section"
        /// <summary>
        /// The Existing Artwork QA Level1 Section
        /// </summary>
        /// <param name="model">The section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveQALevel1Section(ExistingArtworkQALevel1Section model)
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

        #region "Save Existing Artwork QA Level 2 Section"
        /// <summary>
        /// The Existing Artwork QA Level2 Section
        /// </summary>
        /// <param name="model">The section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveQALevel2Section(ExistingArtworkQALevel2Section model)
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

        #region "Save Existing Artwork SCM Level 1 Section"
        /// <summary>
        /// The Existing Artwork SCM Level1 Section
        /// </summary>
        /// <param name="model">The section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveSCMLevel1Section(ExistingArtworkSCMLevel1Section model)
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

        #region "Save Existing Artwork SCM Level 2 Section"
        /// <summary>
        /// The Existing Artwork SCM Level2 Section
        /// </summary>
        /// <param name="model">The section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveSCMLevel2Section(ExistingArtworkSCMLevel2Section model)
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

        #region "Save Existing Artwork ABSQ Team Section"
        /// <summary>
        /// The Existing Artwork ABSQ Team Section
        /// </summary>
        /// <param name="model">The section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveABSQTeamSection(ExistingArtworkABSQTeamSection model)
        {
            ActionStatus status = new ActionStatus();
            if (model != null && this.ValidateModelState(model))
            {
                model.Files = new List<FileDetails>();
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLTechnicalSpecificationAttachment));
                model.Files.AddRange(FileListHelper.GenerateFileBytes(model.FNLQAPAttachment));
                model.FNLTechnicalSpecificationAttachment = string.Join(",", FileListHelper.GetFileNames(model.FNLTechnicalSpecificationAttachment));
                model.FNLQAPAttachment = string.Join(",", FileListHelper.GetFileNames(model.FNLQAPAttachment));
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
                model.ArtworkAttachmentList = new List<ITrans>();
                model.ArtworkAttachmentList.AddRange(model.ArtworkAttachmentTempList);

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

        #region "Save Existing Artwork ABSQ Approver Section"
        /// <summary>
        /// The Existing Artwork ABSQ Approver Section
        /// </summary>
        /// <param name="model">The section</param>
        /// <returns>return status</returns>
        [HttpPost]
        public ActionResult SaveABSQApproverSection(ExistingArtworkABSQApproverSection model)
        {
            ActionStatus status = new ActionStatus();

            if (model != null && this.ValidateModelState(model))
            {
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

        #region  "Get All Items"
        /// <summary>
        /// Gets the All Items search by Model or Item code.
        /// </summary>
        /// <param name="q">The q.</param>
        /// <returns>json object</returns>
        public JsonResult GetAllItems(string q)
        {
            string data = this.GetAllItemsCode(q);
            if (!string.IsNullOrEmpty(data))
            {
                var master = BEL.DataAccessLayer.JSONHelper.ToObject<ItemSearchMaster>(data);
                if (master != null)
                {
                    return this.Json((from item in master.Items select new { id = item.Title, name = item.Title + " - " + (item as ItemSearchMasterListItem).ModelName }).ToList(), JsonRequestBehavior.AllowGet);
                }
            }
            return this.Json(null, JsonRequestBehavior.AllowGet);
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

        /// <summary>
        /// Get Artwork Info
        /// </summary>
        /// <param name="itemCode">item code</param>
        /// <returns>json data</returns>
        public JsonResult GetArtworkInfo(string itemCode)
        {
            string data = this.GetArtworkInfoByItemCode(itemCode);
            if (!string.IsNullOrEmpty(data))
            {
                var master = BEL.DataAccessLayer.JSONHelper.ToObject<ItemMasterListItem>(data);
                return this.Json(master, JsonRequestBehavior.AllowGet);
            }
            return this.Json("[]", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Artwork Type Files
        /// </summary>
        /// <param name="id">item code id</param>
        /// <returns>json result</returns>
        public ActionResult GetArtworkTypeFiles(string id)
        {
            string data = this.GetArtworkTypeFilesByID(id);
            if (!string.IsNullOrEmpty(data))
            {
                ////var master = BEL.DataAccessLayer.JSONHelper.ToObject<ItemMasterArtworkTypeMappingMaster>(data);
                ////List<ItemMasterArtworkTypeMappingMasterListItem> items = master.Items.ConvertAll(o => (o as ItemMasterArtworkTypeMappingMasterListItem));
                List<ItemMasterArtworkTypeMappingMasterListItem> items = BEL.DataAccessLayer.JSONHelper.ToObject<List<ItemMasterArtworkTypeMappingMasterListItem>>(data);
                List<ExistingArtworkAttachment> list = new List<ExistingArtworkAttachment>();
                items.ForEach(p =>
                {
                    ExistingArtworkAttachment obj = new ExistingArtworkAttachment();
                    obj.ArtworkType = p.Title;
                    obj.ArtworkType = p.ArtworkType;
                    obj.ArtworkTypeCode = p.Value;
                    obj.ItemCode = p.ItemCode;
                    if (p.Files != null)
                    {
                        obj.Files = p.Files;
                        p.Files.ForEach(q => q.Status = FileStatus.New);
                        obj.FileNameList = p.Files != null && p.Files.Count > 0 ? JsonConvert.SerializeObject(p.Files) : string.Empty;
                    }
                    else
                    {
                        obj.Files = new List<FileDetails>();
                        obj.FileNameList = string.Empty;
                    }

                    obj.RequestDate = p.RequestDate;
                    obj.RequestBy = p.RequestBy;
                    obj.FileName = p.FileName;
                    list.Add(obj);
                });

                this.SetTempData<List<ExistingArtworkAttachment>>(TempKeys.ArtworkAttachmentMaster.ToString() + "_" + this.GetFormIdFromUrl(), list);
                ////return this.Json(items, JsonRequestBehavior.AllowGet);
                return this.PartialView("_ExisitingAttachmentGrid", list.ToList<ExistingArtworkAttachment>());
            }
            return this.Json("[]", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Check weather item code artwork is Inprogress or not.
        /// </summary>
        /// <param name="itemCode">Item Code</param>
        /// <param name="itemID">Item ID</param>
        /// <returns>true or false</returns>
        public bool CheckInprogress(string itemCode)
        {
            return this.CheckInprogress(itemCode, this.GetFormIdFromUrl());
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