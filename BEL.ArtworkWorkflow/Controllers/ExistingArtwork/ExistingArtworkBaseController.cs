namespace BEL.ArtworkWorkflow.Controllers
{
    using BEL.CommonDataContract;
    using BEL.ArtworkWorkflow.BusinessLayer;
    using BEL.ArtworkWorkflow.Models.ExistingArtwork;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using BEL.DataAccessLayer;
    using BEL.ArtworkWorkflow.Models.Master;

    /// <summary>
    /// Existing Artwork Base Controller
    /// </summary>
    public class ExistingArtworkBaseController : BaseController
    {
        /// <summary>
        /// Get New Artwork Details
        /// </summary>
        /// <param name="objDict">Dict parameter</param>
        /// <returns>return contract</returns>
        public ExistingArtworkContract GetNewArtworkDetails(IDictionary<string, string> objDict)
        {
            ExistingArtworkContract contract = ExistingArtworkBusinessLayer.Instance.GetExistingArtworkDetails(objDict);
            return contract;
        }

        /// <summary>
        /// The Save Section
        /// </summary>
        /// <param name="section">The section</param>
        /// <param name="objDict">The objDict</param>
        /// <returns>return status</returns>
        protected ActionStatus SaveSection(ISection section, Dictionary<string, string> objDict)
        {
            ActionStatus status = new ActionStatus();
            status = ExistingArtworkBusinessLayer.Instance.SaveBySection(section, objDict);
            return status;
        }

        /// <summary>
        /// Get NewArtwork Admin Details.
        /// </summary>
        /// <param name="objDict">The object dictionary.</param>
        /// <returns>return contract</returns>
        public ExistingArtworkContract GetExistingArtworkAdminDetails(IDictionary<string, string> objDict)
        {
            ExistingArtworkContract contract = ExistingArtworkBusinessLayer.Instance.GetExistingArtworkAdminDetails(objDict);
            return contract;
        }

        /// <summary>
        /// Save Admin Detail Section.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="objDict">The object dictionary.</param>
        /// <returns>return status</returns>
        protected ActionStatus SaveAdminDetailSection(ISection section, Dictionary<string, string> objDict)
        {
            ActionStatus status = new ActionStatus();
            status = ExistingArtworkBusinessLayer.Instance.SaveExistingArtworkAdminDataBySection(section, objDict);
            return status;
        }

        /// <summary>
        /// Gets all items.
        /// </summary>
        /// <param name="q">The q.</param>
        /// <returns>json string</returns>
        protected string GetAllItemsCode(string q)
        {
            string data = ExistingArtworkBusinessLayer.Instance.GetAllItemsCode(q);
            return data;
        }

        /// <summary>
        /// Gets all vendor code.
        /// </summary>
        /// <param name="q">The q.</param>
        /// <returns>the string</returns>
        protected string GetAllVendorCode(string q)
        {
            string data = ExistingArtworkBusinessLayer.Instance.GetAllVendorCode(q);
            return data;
        }

        /// <summary>
        /// Gets Artwork Info
        /// </summary>
        /// <param name="itemCode">The item code.</param>
        /// <returns>json string</returns>
        protected string GetArtworkInfoByItemCode(string itemCode)
        {
            string data = ExistingArtworkBusinessLayer.Instance.GetArtworkInfoByItemCode(itemCode);
            return data;
        }

        /// <summary>
        /// Gets Artwork Info
        /// </summary>
        /// <param name="itemCode">The item code.</param>
        /// <returns>json string</returns>
        protected string GetArtworkTypeFilesByID(string id)
        {
            string data = ExistingArtworkBusinessLayer.Instance.GetArtworkTypeFilesByID(id);
            return data;
        }

        /// <summary>
        /// Gets Artwork Info
        /// </summary>
        /// <param name="itemCode">The item code.</param>
        /// <param name="itemID">The item ID.</param>
        /// <returns>json string</returns>
        protected bool CheckInprogress(string itemCode, int itemID)
        {
             return ExistingArtworkBusinessLayer.Instance.IsValidExistingArtworkEntry(itemCode, itemID);
        }

        /// <summary>
        /// Deletes the arwork.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Action Status</returns>
        public ActionStatus DeleteArworkById(int id)
        {
            ActionStatus status = new ActionStatus();
            status = ExistingArtworkBusinessLayer.Instance.DeleteArtwork(id);
            return status;
        }
    }
}