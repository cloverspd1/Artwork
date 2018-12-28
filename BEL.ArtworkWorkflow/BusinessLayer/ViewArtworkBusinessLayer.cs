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
    using BEL.ArtworkWorkflow.Models.ViewArtwork;

    /// <summary>
    /// Existing Artwork Business Layer
    /// </summary>
    public class ViewArtworkBusinessLayer
    {
        /// <summary>
        /// The View Artwork Business Layer
        /// </summary>
        private static readonly Lazy<ViewArtworkBusinessLayer> Lazy =
         new Lazy<ViewArtworkBusinessLayer>(() => new ViewArtworkBusinessLayer());

        /// <summary>
        /// The ExistingArtworkBusinessLayer instance
        /// </summary>
        public static ViewArtworkBusinessLayer Instance
        {
            get
            {
                return Lazy.Value;
            }
        }

        /// <summary>
        /// The padlock
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "NA")]
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
        private ViewArtworkBusinessLayer()
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

        /// <summary>
        /// Get Artwork Details
        /// </summary>
        /// <param name="section">section object</param>
        /// <returns>View Artwork Detail Section</returns>
        public ViewArtworkDetailSection GetArtworkDetails(ViewArtworkDetailSection section)
        {
            if (section != null)
            {
                MasterDataHelper masterhelper = new MasterDataHelper();
                section.MasterData = masterhelper.GetMasterData(this.context, this.web, section.MasterData);
                return section;
            }
            return null;
        }

        /// <summary>
        /// Save Item Master
        /// </summary>
        /// <param name="section">Section Details</param>
        /// <returns>Action Status</returns>
        public ActionStatus SaveItemMaster(ViewArtworkDetailSection section)
        {
            ActionStatus status = new ActionStatus();
            try
            {
                if (section != null)
                {
                    List spList = this.web.Lists.GetByTitle(ArtworkMasters.ITEMMASTER);
                    List spArtworkTypeMappingMaster = this.web.Lists.GetByTitle(ArtworkMasters.ITEMMASTERARTWORKTYPEMAPPINGMASTER);
                    ////ListItemCollection lstArtworkTypeMApping = spArtworkTypeMappingMaster.GetItems(CamlQuery.CreateAllItemsQuery());
                    spArtworkTypeMappingMaster.GetItems(CamlQuery.CreateAllItemsQuery());
                    ListItem oItemListItem = null;
                    if (section.ID == 0)
                    {
                        ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                        oItemListItem = spList.AddItem(itemCreateInfo);
                    }
                    else
                    {
                        oItemListItem = spList.GetItemById(section.ID);
                    }

                    oItemListItem["ItemCode"] = section.ItemCode;
                    oItemListItem["Model"] = section.ModelName;
                    oItemListItem["Color"] = section.Color;
                    oItemListItem["BarCode"] = section.BarCode;
                    oItemListItem["BusinessUnit"] = section.BusinessUnit;
                    oItemListItem["DomesticOrImported"] = section.DomesticOrImported;
                    oItemListItem["MasterCartondimensionL"] = section.MasterCartondimensionL;
                    oItemListItem["MasterCartondimensionW"] = section.MasterCartondimensionW;
                    oItemListItem["MasterCartondimensionH"] = section.MasterCartondimensionH;
                    oItemListItem["UnitCartonDimensionL"] = section.UnitCartonDimensionL;
                    oItemListItem["UnitCartonDimensionW"] = section.UnitCartonDimensionW;
                    oItemListItem["UnitCartonDimensionH"] = section.UnitCartonDimensionH;
                    oItemListItem["MRP"] = section.MRP;
                    oItemListItem["ProductCategory"] = section.ProductCategory;

                    oItemListItem["VendorAddress"] = section.VendorAddress;
                    oItemListItem["VendorCode"] = section.VendorCode;
                    oItemListItem["Warranty"] = section.Warranty;
                    oItemListItem["NoofCartoninMaster"] = section.NoofCartoninMaster;
                    oItemListItem["Product"] = section.Product;
                    oItemListItem["Voltage"] = section.Voltage;
                    oItemListItem["Power"] = section.Power;
                    oItemListItem["GrossWeight"] = section.GrossWeight;
                    oItemListItem["NetWeight"] = section.NetWeight;

                    ////oItemListItem["Requester"] = null;
                    if (section.ActionStatus == ButtonActionStatus.NextApproval)
                    {
                        oItemListItem["Title"] = "View/Edit";
                        oItemListItem["ProposedBy"] = section.ProposedBy;
                        oItemListItem["UploadedDate"] = DateTime.Now;
                        oItemListItem["ABSQUsers"] = null;
                    }
                    oItemListItem.Update();
                    this.context.Load(oItemListItem);
                    this.context.ExecuteQuery();

                    TransactionHelper transactionHelper = new TransactionHelper();
                    status.IsSucceed = transactionHelper.SaveTranItems(this.context, this.web, section.ExistingAttachment, ArtworkListNames.ITEMMASTERARTWORKTYPEMAPPINGMASTER, new Dictionary<string, string> { { Parameter.SETPERMISSION, "false" } });

                    if (section.ActionStatus == ButtonActionStatus.NextApproval)
                    {
                        status.Messages.Add("Text_UpdateSuccess");
                    }
                    else if (section.ActionStatus == ButtonActionStatus.SaveAndNoStatusUpdate)
                    {
                        status.Messages.Add("Text_SaveAsDraftSuccess");
                    }

                    FieldUserValue requester = oItemListItem["Requester"] as FieldUserValue;
                    if (section.ActionStatus == ButtonActionStatus.NextApproval)
                    {
                        if (requester != null && requester.LookupId != 0)
                        {
                            string htmlTbl = @"<p>Dear User,<p>
                                    <p>Requtesed Artwork Type file has been uploaded by ABSQ Team.
                                            <a href ='#URL" + BELDataAccessLayer.Instance.GetConfigVariable("ArtworkSiteURL") + @"/Pages/ViewArtwork.aspx'> Click Here</a> to view Artwork file.</p>            
                            <p><b>Item Code</b>: " + section.ItemCode + @"
                            <p><b>Business Unit</b>: " + section.BusinessUnit + @"
                            <p><b>Model Name</b>: " + section.ModelName + @"            
                            </p><p>Thanks & Regards</p><p>Artwork Team</p>";

                            EmailHelper eHelper = new EmailHelper();
                            string toUser = BELDataAccessLayer.GetEmailUsingUserID(this.context, this.web, requester.LookupId.ToString());
                            string ccUser = BELDataAccessLayer.GetEmailUsingUserID(this.context, this.web, Convert.ToString(section.ProposedBy));
                            eHelper.SendMail("All", "All", "UploadArtwork", "Requtesed Artwork Type file has been uploaded for item code - " + section.ItemCode, htmlTbl, BELDataAccessLayer.Instance.GetConfigVariable("FromEmailAddress"), toUser, ccUser, false);
                        }
                    }
                    ////GlobalCachingProvider.Instance.RemoveItem(ArtworkMasters.ITEMMASTER);
                    ////GlobalCachingProvider.Instance.RemoveItem(ArtworkMasters.ITEMMASTERARTWORKTYPEMAPPINGMASTER);
                    ////AsyncHelper.Call(obj =>
                    ////{
                    ////    MasterDataHelper masterhelper = new MasterDataHelper();
                    ////    masterhelper.GetMasterData(this.context, this.web, new List<IMaster>() { new ItemMaster() });
                    ////});
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error While Save Item Master Data and Artwork Type Item Code Mapping Data");
                Logger.Error(ex);
            }
            return status;
        }

        #region "Get ITem Code Data"
        /// <summary>
        /// Get All Items Code
        /// </summary>
        /// <param name="title">search term</param>
        /// <returns>string data</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "NA")]
        public string GetAllItemsCode(string title)
        {
            MasterDataHelper masterHelper = new MasterDataHelper();
            IMaster itemmaster = masterHelper.GetMasterData(this.context, this.web, new List<IMaster>() { new ItemSearchMaster() }).FirstOrDefault();
            string itemjson = JSONHelper.ToJSON<IMaster>(itemmaster);
            var filterdVendor = JSONHelper.ToObject<IMaster>(itemjson);
            if (filterdVendor != null)
            {
                filterdVendor.Items = filterdVendor.Items.Where(x => ((x as ItemSearchMasterListItem).ItemCode ?? string.Empty).ToLower().Trim().Contains((title ?? string.Empty).ToLower().Trim()) || ((x as ItemSearchMasterListItem).ModelName ?? string.Empty).ToLower().Trim().Contains((title ?? string.Empty).ToLower().Trim())).ToList();
                return JSONHelper.ToJSON<IMaster>(filterdVendor);
            }
            else
            {
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
            query.ViewXml = @"<View><Query><Where><Eq><FieldRef Name='RequestID'  /><Value Type='Lookup'>" + Convert.ToInt32(id) + @"</Value></Eq></Where></Query></View>";
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
        #endregion

        #region "Notification"
        /// <summary>
        /// Send Mail To ABSQTeam
        /// </summary>
        /// <param name="obj">object value</param>
        /// <returns>Sent yes or not</returns>
        public ActionStatus SendMailToABSQTeam(Dictionary<string, string> obj)
        {
            ActionStatus status = new ActionStatus();
            if (obj != null && obj.Count > 0)
            {
                try
                {
                    string artTypes = obj.ContainsKey("ArtworkTypes") && !string.IsNullOrEmpty(obj["ArtworkTypes"].ToString()) ? Convert.ToString(obj["ArtworkTypes"]).Replace(",", ", ") : string.Empty;
                    string itemURL = BELDataAccessLayer.Instance.GetConfigVariable("ArtworkSiteURL") + @"/Pages/ViewArtwork.aspx?ItemCode=" + Convert.ToString(obj["ItemCode"]) + @"&Types=" + Convert.ToString(obj["ArtworkTypeCode"]);
                    string htmlTbl = @"<p>Dear User,<p>
                                    <p>Below Request for Upload Artwork Type Files
                                            <a href ='#URL" + itemURL + @"'> Click Here</a> to view/edit Artwork file.</p>            
                            <p><b>Item Code</b>: " + Convert.ToString(obj["ItemCode"]) + @"
                            <p><b>Business Unit</b>: " + Convert.ToString(obj["BusinessUnit"]) + @"
                            <p><b>Model Name</b>: " + Convert.ToString(obj["ModelName"]) + @"            
                            <p><b>Requested By</b>: " + Convert.ToString(obj["ProposedByName"]) + @"    
                            <p><b>Artwork Type</b>: " + artTypes + @"       
                            </p><p>Thanks & Regards</p><p>Artwork Team</p>";

                    EmailHelper eHelper = new EmailHelper();
                    ////string fromUser = BELDataAccessLayer.GetEmailUsingUserID(this.context, this.web, Convert.ToString(obj["ItemCode"]));
                    string toUser = BELDataAccessLayer.GetEmailUsingUserID(this.context, this.web, Convert.ToString(obj["ABSQUsers"]));
                    string ccUser = BELDataAccessLayer.GetEmailUsingUserID(this.context, this.web, Convert.ToString(obj["ProposedBy"]));
                    eHelper.SendMail("All", "All", "UploadArtwork", "Request For Upload Artwork Type File for item code - " + obj["ItemCode"].ToString(), htmlTbl, BELDataAccessLayer.Instance.GetConfigVariable("FromEmailAddress"), toUser, ccUser, false);
                    status.IsSucceed = true;
                    status.Messages.Add("Text_SentEmailtoABSQSuccess");

                    if (obj.ContainsKey("ID") && !string.IsNullOrEmpty(Convert.ToString(obj["ID"])) && obj.ContainsKey("ProposedBy") && !string.IsNullOrEmpty(Convert.ToString(obj["ProposedBy"])))
                    {
                        List spList = this.web.Lists.GetByTitle(ArtworkMasters.ITEMMASTER);
                        ListItem item = spList.GetItemById(Convert.ToInt32(obj["ID"]));
                        item["Requester"] = new FieldUserValue() { LookupId = Convert.ToInt32(obj["ProposedBy"]) };
                        item["RequestedDate"] = DateTime.Now;
                        item["ABSQUsers"] = BELDataAccessLayer.GetFieldUserValueFromPerson(this.context, this.web, Convert.ToString(obj["ABSQUsers"]).Trim(',').Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

                        FieldUrlValue hyper = new FieldUrlValue();
                        hyper.Description = "View Item";
                        hyper.Url = itemURL;
                        item["ViewItem"] = hyper;
                        item.Update();
                        this.context.ExecuteQuery();

                        ////add requester to Requester Group
                        try
                        {
                            FieldUserValue requester = item["Requester"] as FieldUserValue;
                            if (requester != null && requester.LookupId != 0)
                            {
                                GroupCollection groups = this.context.Web.SiteGroups;
                                Group requesterGroup = groups.GetByName("Requester");
                                ////UserCreationInformation userInfo = new UserCreationInformation();
                                User user = this.context.Web.GetUserById(requester.LookupId);
                                this.context.Load(user);
                                this.context.ExecuteQuery();
                                ////userInfo.LoginName = user.LoginName;
                                ////userInfo.Email = user.Email;
                                ////userInfo.Title = user.Title;

                                ////requesterGroup.Users.Add(userInfo);
                                requesterGroup.Users.AddUser(user);
                                this.context.ExecuteQuery();
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Error while adding requester to Requester Group -" + ex.StackTrace);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    status.IsSucceed = false;
                    status.Messages.Add("Text_SentEmailtoABSQFail");
                }
                return status;
            }
            return status;
        }
        #endregion

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
    }
}