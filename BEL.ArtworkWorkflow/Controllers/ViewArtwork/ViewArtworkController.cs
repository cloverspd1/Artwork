namespace BEL.ArtworkWorkflow.Controllers.ViewArtwork
{
    using BEL.ArtworkWorkflow.BusinessLayer;
    using BEL.ArtworkWorkflow.Common;
    using BEL.ArtworkWorkflow.Models.Common;
    using BEL.ArtworkWorkflow.Models.ExistingArtwork;
    using BEL.ArtworkWorkflow.Models.Master;
    using BEL.ArtworkWorkflow.Models.ViewArtwork;
    using BEL.CommonDataContract;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    /// <summary>
    /// View Artwork Controller
    /// </summary>
    public class ViewArtworkController : ViewArtworkBaseController
    {
        /// <summary>
        /// Index Page Load
        /// </summary>
        /// <param name="itemCode">item Code</param>
        /// <param name="types">Artwrok Types</param>
        /// <returns>Detail Section</returns>
        [SharePointContextFilter]
        public ActionResult Index(string itemCode = "", string types = "")
        {
            ViewArtworkDetailSection section = new ViewArtworkDetailSection(true);
            section = this.GetArtworkDetails(section);
            section.ProposedBy = this.CurrentUser.UserId;
            section.ProposedByName = this.CurrentUser.FullName;
            List<IMasterItem> approvers = section.MasterData.FirstOrDefault(p => p.NameOfMaster == ArtworkMasters.ROLEWISEAPPROVERMASTER).Items.ToList();
            List<RoleWiseApproverMasterListItem> approverList = approvers.ConvertAll(p => (RoleWiseApproverMasterListItem)p);
            RoleWiseApproverMasterListItem obj = null;
            bool isValidRequester = false;
            foreach (RoleWiseApproverMasterListItem item in approverList)
            {
                string[] strRequester = item.Approvers.Split(',');
                foreach (string str in strRequester)
                {
                    if (str == this.CurrentUser.UserId && (item.Role == ArtworkRoles.ABSQTEAM || item.Role == ArtworkRoles.ABSQAPPROVER))
                    {
                        isValidRequester = true;
                    }
                    if (item.Role == ArtworkRoles.ABSQTEAM || item.Role == ArtworkRoles.ABSQAPPROVER)
                    {
                        section.ABSQUsers += item.Approvers + "," + section.ABSQUsers;
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
                section.IsABSQUser = true;
            }
            if (!string.IsNullOrEmpty(itemCode))
            {
                section.ItemCode = itemCode;
            }
            if (!string.IsNullOrEmpty(types))
            {
                section.ArtworkType = types.Trim();
            }
            string[] absqlst = section.ABSQUsers.Trim(',').Split(',');
            section.ABSQUsers = string.Join(",", absqlst.Distinct().ToArray());
            section.Product = "Local";
            List<IMasterItem> artworkTypes = section.MasterData.FirstOrDefault(p => p.NameOfMaster == ArtworkMasters.ARTWORKTYEMASTER).Items.ToList();
            List<ArtworkTypeMasterListItem> artworkTypeList = artworkTypes.ConvertAll(p => (ArtworkTypeMasterListItem)p);
            this.SetTempData<List<ArtworkTypeMasterListItem>>(TempKeys.ArtworkTypeMaster.ToString() + "_" + this.GetFormIdFromUrl(), artworkTypeList.ToList());

            List<ExistingArtworkAttachment> list = new List<ExistingArtworkAttachment>();
            this.SetTempData<List<ExistingArtworkAttachment>>(TempKeys.ArtworkAttachmentMaster.ToString() + "_" + this.GetFormIdFromUrl(), list);
            return this.View(section);
        }

        /// <summary>
        /// Save Item Master Data
        /// </summary>
        /// <param name="model">Artwork Detail</param>
        /// <returns>Action Status</returns>
        [HttpPost]
        public ActionResult SaveItemMaster(ViewArtworkDetailSection model)
        {
            ActionStatus status = new ActionStatus();
            ModelState.Remove("ArtworkType");
            List<bool> checkvalue = new List<bool>();
            if (model.TempExistingAttachment != null && model.TempExistingAttachment.Count > 0)
            {
                foreach (ExistingArtworkAttachment item in model.TempExistingAttachment)
                {
                    bool checkCheckoxValue = item.IsNotApplicable;
                    checkvalue.Add(checkCheckoxValue);
                }
            }
            if (model != null && model.Product == "Import")
            {
                ModelState.Remove("VendorCode");
                ModelState.Remove("VendorAddress");
                model.VendorCode = model.VendorAddress = string.Empty;
            }
            if (model != null && this.ValidateModelState(model))
            {
                if (model.IsABSQUser)
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

                    model.ProposedBy = this.CurrentUser.UserId;
                    model.ProposedByName = this.CurrentUser.FullName;
                    bool filenotavailble = false;
                    List<ExistingArtworkAttachment> list = this.GetTempData<List<ExistingArtworkAttachment>>(TempKeys.ArtworkAttachmentMaster.ToString() + "_" + this.GetFormIdFromUrl());
                    int i = 0;
                    foreach (bool value in checkvalue)
                    {
                        list[i].IsNotApplicable = value;
                        i++;
                    }
                    list.ForEach(p =>
                    {
                        p.RequestDate = DateTime.Now;
                        p.RequestID = model.ID;
                        p.ItemCode = model.ItemCode;
                        p.RequestBy = this.CurrentUser.UserId;

                        if (p.ItemAction != ItemActionStatus.DELETED)
                        {
                            if (p.IsNotApplicable == false)
                            {
                                if (!string.IsNullOrWhiteSpace(p.FileNameList))          ////( p.FileNameList != null)
                                {
                                    p.Files = new List<FileDetails>();
                                    p.FileName = string.Join(",", FileListHelper.GetFileNamesForABSQTeam(p.FileNameList, model.ItemCode, p.ArtworkTypeCode));
                                    p.Files.AddRange(FileListHelper.GenerateFileBytesForABSQTeam(p.FileNameList, model.ItemCode, p.ArtworkTypeCode));
                                    if (p.Files != null && (!string.IsNullOrWhiteSpace(p.FileNameList)) && p.Files.Count() > 0)
                                    {
                                        if (p.ID == 0)
                                        {
                                            p.ItemAction = ItemActionStatus.NEW;
                                        }
                                    }
                                    if (p.Files == null || (p.Files != null && (p.Files.Count == 0 || !p.Files.Any(m => m.Status != FileStatus.Delete))))
                                    {
                                        filenotavailble = true;
                                    }
                                }
                                else
                                {
                                    filenotavailble = true;
                                }
                            }
                            ////else
                            ////{
                            ////    filenotavailble = true;
                            ////}
                        }
                    });
                    if (filenotavailble)
                    {
                        status.IsSucceed = false;
                        status.Messages = new List<string>() { "Artwork Type Attachtment is required." };
                        return this.Json(status);
                    }
                    model.ExistingAttachment = new List<ITrans>();
                    model.ExistingAttachment.AddRange(list);

                    status = this.SaveItem(model);
                    status = this.GetMessage(status, System.Web.Mvc.Html.ResourceNames.Artwork);
                    return this.Json(status);
                }
                else
                {
                    if (string.IsNullOrEmpty(model.StrArtworkTypeList))
                    {
                        status.IsSucceed = false;
                        status.Messages.Add("Error_StrArtworkTypeList");
                        status = this.GetMessage(status, System.Web.Mvc.Html.ResourceNames.Artwork);
                        return this.Json(status);
                    }
                    ////Send Mail to User
                    Dictionary<string, string> obj = new Dictionary<string, string>();
                    string data = this.GetArtworkInfoByItemCode(model.ItemCode);
                    if (!string.IsNullOrEmpty(data))
                    {
                        var master = BEL.DataAccessLayer.JSONHelper.ToObject<ItemMasterListItem>(data);
                        obj.Add("ID", Convert.ToString(model.ID));
                        obj.Add("ItemCode", model.ItemCode);
                        obj.Add("BusinessUnit", master.BusinessUnit);
                        obj.Add("ModelName", master.ModelName);
                        obj.Add("ProductCategory", master.ProductCategory);
                        obj.Add("ProposedBy", model.ProposedBy);
                        obj.Add("ProposedByName", model.ProposedByName);
                        obj.Add("ABSQUsers", model.ABSQUsers);
                        obj.Add("ArtworkTypeCode", model.ArtworkType);
                        obj.Add("ArtworkTypes", model.StrArtworkTypeList);
                        status = this.SendEmailToABSQTeam(obj);
                        status = this.GetMessage(status, System.Web.Mvc.Html.ResourceNames.Artwork);
                        return this.Json(status);
                    }
                }
            }
            else
            {
                status.IsSucceed = false;
                status.Messages = this.GetErrorMessage(System.Web.Mvc.Html.ResourceNames.Artwork);
            }
            return this.Json(status);
        }

        /// <summary>
        /// Add Artwork Type
        /// </summary>
        /// <param name="artWorkTypeCodes">ArtWork Type Codes</param>
        /// <param name="itemCode">Item Code</param>
        /// <returns>Action Result</returns>
        [HttpGet]
        public ActionResult AddArtworkType(string artWorkTypeCodes, string itemCode)
        {
            if (!string.IsNullOrEmpty(itemCode))
            {
                List<ExistingArtworkAttachment> list = new List<ExistingArtworkAttachment>();
                artWorkTypeCodes = artWorkTypeCodes == "null" ? string.Empty : artWorkTypeCodes;
                list = this.GetTempData<List<ExistingArtworkAttachment>>(TempKeys.ArtworkAttachmentMaster.ToString() + "_" + this.GetFormIdFromUrl());
                if (!string.IsNullOrEmpty(artWorkTypeCodes))
                {
                    List<ArtworkTypeMasterListItem> artworkTypeList = this.GetTempData<List<ArtworkTypeMasterListItem>>(TempKeys.ArtworkTypeMaster.ToString() + "_" + this.GetFormIdFromUrl());
                    string[] types = artWorkTypeCodes.Split(',');
                    list.ForEach(p => p.ItemAction = ItemActionStatus.DELETED);

                    foreach (string strcode in types)
                    {
                        ExistingArtworkAttachment obj = list.FirstOrDefault(p => p.ArtworkTypeCode == strcode);

                        if (obj == null)
                        {
                            obj = new ExistingArtworkAttachment();
                            obj.ItemAction = ItemActionStatus.NEW;
                            obj.ItemCode = itemCode;
                            obj.ArtworkTypeCode = strcode;
                            obj.RequestDate = DateTime.Now;
                            obj.RequestBy = this.CurrentUser.UserId;
                            obj.ArtworkType = artworkTypeList.FirstOrDefault(p => p.ArtworkTypeCode == strcode).Title;
                            list.Add(obj);
                        }
                        else if (obj != null)
                        {
                            obj.ItemAction = ItemActionStatus.UPDATED;
                        }
                    }
                }
                this.SetTempData<List<ExistingArtworkAttachment>>(TempKeys.ArtworkAttachmentMaster.ToString() + "_" + this.GetFormIdFromUrl(), list.OrderBy(x => x.Index).ToList());
                return this.PartialView("_ExisitingAttachmentGrid", list.Where(x => x.ItemAction != ItemActionStatus.DELETED).ToList());
            }
            return this.PartialView("_ExisitingAttachmentGrid", new List<ExistingArtworkAttachment>());
        }

        #region  "Get All Items"
        /// <summary>
        /// Gets the All Items search by Model or Item code.
        /// </summary>
        /// <param name="q">The q.</param>
        /// <returns>json object</returns>
        public JsonResult GetAllItems(string q)
        {
            string data = ExistingArtworkBusinessLayer.Instance.GetAllItemsCode(q);
            if (!string.IsNullOrEmpty(data))
            {
                var master = BEL.DataAccessLayer.JSONHelper.ToObject<ItemSearchMaster>(data);
                return this.Json((from item in master.Items select new { id = item.Title, name = item.Title + " - " + (item as ItemSearchMasterListItem).ModelName }).ToList(), JsonRequestBehavior.AllowGet);
            }
            return this.Json("[]", JsonRequestBehavior.AllowGet);
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
            List<ExistingArtworkAttachment> list = new List<ExistingArtworkAttachment>();
            this.SetTempData<List<ExistingArtworkAttachment>>(TempKeys.ArtworkAttachmentMaster.ToString() + "_" + this.GetFormIdFromUrl(), list);
            return this.Json("[]", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Artwork Type Files
        /// </summary>
        /// <param name="id">item code id</param>
        /// <returns>json result</returns>
        public ActionResult GetArtworkTypeFiles(string id)
        {
            try
            {
                string data = ExistingArtworkBusinessLayer.Instance.GetArtworkTypeFilesByID(id);
                if (!string.IsNullOrEmpty(data))
                {
                    List<ItemMasterArtworkTypeMappingMasterListItem> items = BEL.DataAccessLayer.JSONHelper.ToObject<List<ItemMasterArtworkTypeMappingMasterListItem>>(data);
                    List<ExistingArtworkAttachment> list = new List<ExistingArtworkAttachment>();
                    items.ForEach(p =>
                    {
                        ExistingArtworkAttachment obj = new ExistingArtworkAttachment();
                        obj.ArtworkType = p.Title;
                        obj.ArtworkType = p.ArtworkType;
                        obj.ArtworkTypeCode = p.ArtworkTypeCode;
                        obj.IsNotApplicable = p.IsNotApplicable;

                        if (p.Files != null)
                        {
                            obj.Files = p.Files;
                            p.Files.ForEach(q => q.Status = FileStatus.NoAction);
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
                        obj.ID = p.ID;
                        obj.ItemCode = p.ItemCode;
                        obj.ItemAction = ItemActionStatus.NOCHANGE;
                        list.Add(obj);
                    });

                    this.SetTempData<List<ExistingArtworkAttachment>>(TempKeys.ArtworkAttachmentMaster.ToString() + "_" + this.GetFormIdFromUrl(), list);
                    ////return this.Json(items, JsonRequestBehavior.AllowGet);
                    return this.PartialView("_ExisitingAttachmentGrid", list.ToList<ExistingArtworkAttachment>());
                }
                return this.Json("[]", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Json("[]", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        /// <summary>
        /// Add Update UploadFile
        /// </summary>
        /// <param name="qqfile">qqfile data</param>
        /// <param name="artworkTypeCode">artworkType Code</param>
        /// <param name="modelName">model Name</param>
        /// <returns>Json Result</returns>
        public JsonResult AddUpdateUploadFile(string qqfile, string artworkTypeCode, string modelName)
        {
            var stream = this.Request.InputStream;
            ////Security Testing Fixes start
            ////check server side valid file extension
            if (!Helper.IsValidFileExtension(System.IO.Path.GetExtension(qqfile).Replace(".", string.Empty)))
            {
                return this.Json(new ActionStatus() { IsSucceed = false, Message = System.IO.Path.GetFileName(qqfile) + " type of not allowed." });
            }
            ////Security Testing Fixes End
            string id = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(qqfile);
            if (!string.IsNullOrWhiteSpace(modelName))
            {
                modelName = Regex.Replace(modelName, @"\t|\n|\r", " ");
            }
            string fileName = artworkTypeCode + "_" + modelName.Trim() + "_" + DateTime.Now.ToString("ddMMyy") + System.IO.Path.GetExtension(qqfile);
            ////System.IO.Path.GetFileNameWithoutExtension(qqfile) + System.IO.Path.GetExtension(qqfile);
            if (string.IsNullOrEmpty(this.Request["qqfile"]))
            {
                //// IE Fix
                HttpPostedFileBase postedFile = this.Request.Files[0];
                stream = postedFile.InputStream;
            }
            byte[] fileData = null;

            using (var binaryReader = new BinaryReader(stream))
            {
                fileData = binaryReader.ReadBytes((int)stream.Length);
            }

            System.IO.File.WriteAllBytes(Server.MapPath("~/Uploads/" + id), fileData);

            //// For View Artwrok start
            FileDetails file = new FileDetails() { FileId = id, FileName = fileName, FileURL = this.ApplicatinBaseUrl.Trim('/') + Url.Content("/Uploads/" + id), Status = FileStatus.New };
            List<ExistingArtworkAttachment> list = this.GetTempData<List<ExistingArtworkAttachment>>(TempKeys.ArtworkAttachmentMaster.ToString() + "_" + this.GetFormIdFromUrl());
            List<FileDetails> files = new List<FileDetails>();

            if (list != null)
            {
                ExistingArtworkAttachment item = list.FirstOrDefault(p => p.ArtworkTypeCode == artworkTypeCode);
                if (item != null)
                {
                    if (!string.IsNullOrEmpty(item.FileNameList))
                    {
                        files.AddRange(JsonConvert.DeserializeObject<List<FileDetails>>(item.FileNameList));
                        files.ForEach(q => q.Status = FileStatus.Delete);
                    }

                    files.Add(file);
                    item.FileNameList = JsonConvert.SerializeObject(files);
                }
                this.SetTempData<List<ExistingArtworkAttachment>>(TempKeys.ArtworkAttachmentMaster.ToString() + "_" + this.GetFormIdFromUrl(), list.OrderBy(x => x.Index).ToList());
            }
            ////For View Artwrok End

            ////return this.Json(new FileDetails() { FileId = id, FileName = fileName, FileURL = this.BaseUrl.Trim('/') + Url.Content("~/Uploads/" + id), Status = FileStatus.New });
            return this.Json(file);
        }

        /// <summary>
        /// Clear Temp Data
        /// </summary>
        /// <returns>Action Result</returns>
        public ActionResult ClearTempData()
        {
            List<ExistingArtworkAttachment> list = new List<ExistingArtworkAttachment>();
            this.SetTempData<List<ExistingArtworkAttachment>>(TempKeys.ArtworkAttachmentMaster.ToString() + "_" + this.GetFormIdFromUrl(), list);
            return this.PartialView("_ExisitingAttachmentGrid", list.ToList<ExistingArtworkAttachment>());
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
    }
}