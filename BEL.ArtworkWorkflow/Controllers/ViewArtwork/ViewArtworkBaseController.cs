namespace BEL.ArtworkWorkflow.Controllers.ViewArtwork
{
    using BEL.ArtworkWorkflow.BusinessLayer;
    using BEL.ArtworkWorkflow.Common;
    using BEL.ArtworkWorkflow.Models.Common;
    using BEL.ArtworkWorkflow.Models.ExistingArtwork;
    using BEL.ArtworkWorkflow.Models.Master;
    using BEL.ArtworkWorkflow.Models.ViewArtwork;
    using BEL.CommonDataContract;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// View Artwork Base Controller
    /// </summary>
    public class ViewArtworkBaseController : BaseController
    {
        /// <summary>
        /// Get New Artwork Details
        /// </summary>
        /// <param name="objDict">Dict parameter</param>
        /// <returns>return contract</returns>
        public ViewArtworkDetailSection GetArtworkDetails(ViewArtworkDetailSection section)
        {
            ViewArtworkDetailSection sectiondetail = ViewArtworkBusinessLayer.Instance.GetArtworkDetails(section);
            return sectiondetail;
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
        /// Save Item Master
        /// </summary>
        /// <param name="section">Item Section</param>
        /// <returns>Action Status</returns>
        protected ActionStatus SaveItem(ViewArtworkDetailSection section)
        {
            ActionStatus action = ViewArtworkBusinessLayer.Instance.SaveItemMaster(section);
            return action;
        }

        /// <summary>
        /// Send Email To ABSQTeam
        /// </summary>
        /// <param name="section">Item Section</param>
        /// <returns>Action Status</returns>
        public ActionStatus SendEmailToABSQTeam(Dictionary<string, string> obj)
        {
            return ViewArtworkBusinessLayer.Instance.SendMailToABSQTeam(obj);
        }

        /// <summary>
        /// Gets all vendor code.
        /// </summary>
        /// <param name="q">The q.</param>
        /// <returns>the string</returns>
        protected string GetAllVendorCode(string q)
        {
            string data = ViewArtworkBusinessLayer.Instance.GetAllVendorCode(q);
            return data;
        }
    }
}