namespace BEL.ArtworkWorkflow.BusinessLayer
{
    using BEL.ArtworkWorkflow.Models.Common;
    using BEL.ArtworkWorkflow.Models.Master;
    using BEL.ArtworkWorkflow.Models.NewArtwork;
    using CommonDataContract;
    using DataAccessLayer;
    using Microsoft.SharePoint.Client;
    using Microsoft.SharePoint.Client.UserProfiles;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web;

    /// <summary>
    /// Common Business Layer
    /// </summary>
    public sealed class CommonBusinessLayer
    {
        /// <summary>
        ///  lazy Instance
        /// </summary>
        private static readonly Lazy<CommonBusinessLayer> Lazy = new Lazy<CommonBusinessLayer>(() => new CommonBusinessLayer());

        /// <summary>
        /// Comman Instance
        /// </summary>
        public static CommonBusinessLayer Instance
        {
            get
            {
                return Lazy.Value;
            }
        }

        /// <summary>
        /// The context
        /// </summary>
        private ClientContext context = null;

        /// <summary>
        /// The web
        /// </summary>
        private Web web = null;

        /// <summary>
        /// Common Business Layer
        /// </summary>
        private CommonBusinessLayer()
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
        /// Download File
        /// </summary>
        /// <param name="url">URL string</param>
        /// <param name="applicationName">application Name</param>
        /// <returns>Byte Array</returns>
        public byte[] DownloadFile(string url, string applicationName)
        {
            BELDataAccessLayer helper = new BELDataAccessLayer();
            ////string siteURL = helper.GetSiteURL(applicationName);
            ////context = helper.CreateClientContext(siteURL);
            return helper.GetFileBytesByUrl(this.context, url);
        }

        /// <summary>
        /// Validates the users.
        /// </summary>
        /// <param name="emailList">The email list.</param>
        /// <returns>list of invalid users</returns>
        public List<string> ValidateUsers(List<string> emailList)
        {
            BELDataAccessLayer helper = new BELDataAccessLayer();
            return helper.GetInvalidUsers(emailList);
        }

        /// <summary>
        /// Removes the cache keys.
        /// </summary>
        /// <param name="keys">The keys.</param>
        public void RemoveCacheKeys(List<string> keys)
        {
            if (keys != null && keys.Count != 0)
            {
                foreach (string key in keys)
                {
                    GlobalCachingProvider.Instance.RemoveItem(key);
                }
            }
        }

        /// <summary>
        /// Gets the cache keys.
        /// </summary>
        /// <returns>list of string</returns>
        public List<string> GetCacheKeys()
        {
            return GlobalCachingProvider.Instance.GetAllKeys();
        }

        /// <summary>
        /// Gets the file name list.
        /// </summary>
        /// <param name="sectionDetails">The section details.</param>
        /// <param name="type">The type.</param>
        /// <returns>ISection Detail</returns>
        public ISection GetFileNameList(ISection sectionDetails, Type type)
        {
            if (sectionDetails == null)
            {
                return null;
            }

            dynamic dysectionDetails = Convert.ChangeType(sectionDetails, type);
            dysectionDetails.FileNameList = string.Empty;

            if (dysectionDetails.Files != null && dysectionDetails.Files.Count > 0)
            {
                dysectionDetails.FileNameList = JsonConvert.SerializeObject(dysectionDetails.Files);
            }
            return dysectionDetails;
        }

        /// <summary>
        /// Gets the file name list from current approver.
        /// </summary>
        /// <param name="sectionDetails">The section details.</param>
        /// <param name="type">The type.</param>
        /// <returns>I Section</returns>
        public ISection GetFileNameListFromCurrentApprover(ISection sectionDetails, Type type)
        {
            if (sectionDetails == null)
            {
                return null;
            }
            dynamic dysectionDetails = Convert.ChangeType(sectionDetails, type);
            dysectionDetails.FileNameList = string.Empty;

            if (dysectionDetails.CurrentApprover != null && dysectionDetails.CurrentApprover.Files != null && dysectionDetails.CurrentApprover.Files.Count > 0)
            {
                dysectionDetails.FileNameList = JsonConvert.SerializeObject(dysectionDetails.CurrentApprover.Files);
            }
            return dysectionDetails;
        }

        /// <summary>
        /// Get Login User Detail
        /// </summary>
        /// <param name="id">id value</param>
        /// <returns>user detail</returns>
        public UserDetails GetLoginUserDetail(string id)
        {
            MasterDataHelper masterHelper = new MasterDataHelper();
            List<UserDetails> userInfoList = masterHelper.GetAllEmployee(this.context, this.web);
            UserDetails detail = userInfoList.FirstOrDefault(p => p.UserId == id);
            return detail;
        }

        /// <summary>
        /// Gets all employee.
        /// </summary>
        /// <returns>List of users</returns>
        public List<UserDetails> GetAllEmployee()
        {
            List<UserDetails> userInfoList = new List<UserDetails>();

            var siteUsers = from user in this.web.SiteUsers
                            where user.PrincipalType == Microsoft.SharePoint.Client.Utilities.PrincipalType.User
                            select user;
            var usersResult = this.context.LoadQuery(siteUsers);
            this.context.ExecuteQuery();

            var peopleManager = new PeopleManager(this.context);
            var userProfilesResult = new List<PersonProperties>();

            foreach (var user in usersResult)
            {
                var userProfile = peopleManager.GetPropertiesFor(user.LoginName);
                this.context.Load(userProfile);
                userProfilesResult.Add(userProfile);
            }
            this.context.ExecuteQuery();

            if (userProfilesResult != null && userProfilesResult.Count != 0)
            {
                var result = from userProfile in userProfilesResult
                             where userProfile.ServerObjectIsNull != null && userProfile.ServerObjectIsNull.Value != true
                             select new UserDetails()
                             {
                                 UserId = usersResult.Where(p => p.LoginName == userProfile.AccountName).FirstOrDefault() != null ? usersResult.Where(p => p.LoginName == userProfile.AccountName).FirstOrDefault().Id.ToString() : string.Empty,
                                 FullName = userProfile.Title,
                                 UserEmail = userProfile.Email,
                                 LoginName = userProfile.AccountName,
                                 Department = userProfile.UserProfileProperties.ContainsKey("Department") ? Convert.ToString(userProfile.UserProfileProperties["Department"]) : string.Empty,
                                 ReportingManager = userProfile.UserProfileProperties.ContainsKey("Manager") ? Convert.ToString(userProfile.UserProfileProperties["Manager"]) : string.Empty
                             };

                userInfoList = result.ToList();
                GlobalCachingProvider.Instance.AddItem(this.web.ServerRelativeUrl + "/" + BEL.CommonDataContract.ListNames.EmployeeMasterList, userInfoList);
            }
            return userInfoList;
        }

        /// <summary>
        /// Get Current User
        /// </summary>
        /// <param name="userid">User ID</param>
        /// <returns>User Object</returns>
        public User GetCurrentUser(string userid)
        {
            return BELDataAccessLayer.EnsureUser(this.context, this.web, userid);
        }

        /// <summary>
        /// cehck user is in Admin group.
        /// </summary>
        /// <param name="userid">User ID</param>
        /// <returns>Yes or No</returns>
        public bool IsAdminUser(string userid)
        {
            return BELDataAccessLayer.Instance.IsGroupMember(this.context, this.web, userid, UserRoles.ADMIN);
        }

        /// <summary>
        /// cehck user is in Creator group.
        /// </summary>
        /// <param name="userid">User ID</param>
        /// <returns>Yes or No</returns>
        public bool IsCreator(string userid)
        {
            return BELDataAccessLayer.Instance.IsGroupMember(this.context, this.web, userid, UserRoles.CREATOR);
        }

        /// <summary>
        ///  Save ITem MAster data.
        /// </summary>
        /// <param name="itemMaster">item master data</param>
        /// <param name="artworkfiles">Artwork Files</param>
        /// <param name="isNew">Yes or No</param>
        public void SaveItemArtworkTypeMasterData(ItemMasterListItem itemMaster, List<NewArtworkAttachment> artworkfiles, bool isNew)
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
                oItemListItem["BarCode"] = itemMaster.BarCode;
                oItemListItem["BusinessUnit"] = itemMaster.BusinessUnit;
                oItemListItem["MasterCartondimensionL"] = itemMaster.MasterCartondimensionL;
                oItemListItem["MasterCartondimensionW"] = itemMaster.MasterCartondimensionW;
                oItemListItem["MasterCartondimensionH"] = itemMaster.MasterCartondimensionH;
                oItemListItem["UnitCartonDimensionL"] = itemMaster.UnitCartonDimensionL;
                oItemListItem["UnitCartonDimensionW"] = itemMaster.UnitCartonDimensionW;
                oItemListItem["UnitCartonDimensionH"] = itemMaster.UnitCartonDimensionH;
                oItemListItem["Model"] = itemMaster.ModelName;
                oItemListItem["MRP"] = itemMaster.MRP;
                oItemListItem["ProductCategory"] = itemMaster.ProductCategory;
                oItemListItem["ReferenceNo"] = itemMaster.ReferenceNo;
                oItemListItem["Title"] = itemMaster.Title;
                oItemListItem["VendorAddress"] = itemMaster.VendorAddress;
                oItemListItem["VendorCode"] = itemMaster.VendorCode;
                oItemListItem["Warranty"] = itemMaster.Warranty;
                oItemListItem["NoofCartoninMaster"] = itemMaster.NoofCartoninMaster;
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
                oArtworkTypeMappingItemUpdate["RequestID"] = oItemListItem.Id;
                oArtworkTypeMappingItemUpdate["RequestBy"] = item.RequestBy;
                oArtworkTypeMappingItemUpdate["Title"] = item.ArtworkType;
                oArtworkTypeMappingItemUpdate["ArtworkTypeCode"] = item.ArtworkTypeCode;
                oArtworkTypeMappingItemUpdate.Update();
                this.context.ExecuteQuery();
                Microsoft.SharePoint.Client.File file = this.context.Web.GetFileByServerRelativeUrl(item.Files[0].FileURL);
                ClientResult<System.IO.Stream> data = file.OpenBinaryStream();
                this.context.Load(file);
                this.context.ExecuteQuery();
                using (System.IO.MemoryStream mStream = new System.IO.MemoryStream())
                {
                    if (data != null)
                    {
                        data.Value.CopyTo(mStream);
                        ////byte[] imageArray = mStream.ToArray();
                        AttachmentCreationInformation aci = new AttachmentCreationInformation();
                        aci.ContentStream = mStream;
                        aci.FileName = item.Files[0].FileName;
                        oArtworkTypeMappingItemUpdate.AttachmentFiles.Add(aci);
                        this.context.ExecuteQuery();
                    }
                }
            }
        }
    }
}