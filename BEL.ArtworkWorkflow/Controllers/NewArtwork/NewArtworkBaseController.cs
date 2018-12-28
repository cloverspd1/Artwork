namespace BEL.ArtworkWorkflow.Controllers
{
    using BEL.CommonDataContract;
    using BEL.ArtworkWorkflow.BusinessLayer;
    using BEL.ArtworkWorkflow.Models.NewArtwork;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// Existing Artwork Base Controller
    /// </summary>
    public class NewArtworkBaseController : BaseController
    {
        /// <summary>
        /// The Get New Artwork Details
        /// </summary>
        /// <param name="objDict">The obj Dict</param>
        /// <returns>return contract</returns>
        public NewArtworkContract GetNewArtworkDetails(IDictionary<string, string> objDict)
        {
            NewArtworkContract contract = NewArtworkBusinessLayer.Instance.GetNewArtworkDetails(objDict);
            return contract;
        }

        /// <summary>
        /// Saves the section.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="objDict">The object dictionary.</param>
        /// <returns>return status</returns>
        protected ActionStatus SaveSection(ISection section, Dictionary<string, string> objDict)
        {
            ActionStatus status = new ActionStatus();
            status = NewArtworkBusinessLayer.Instance.SaveBySection(section, objDict);
            return status;
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
            status = NewArtworkBusinessLayer.Instance.SaveNewArtworkAdminDataBySection(section, objDict);
            return status;
        }

        /// <summary>
        /// Get NewArtwork Admin Details.
        /// </summary>
        /// <param name="objDict">The object dictionary.</param>
        /// <returns>return contract</returns>
        public NewArtworkContract GetNewArtworkAdminDetails(IDictionary<string, string> objDict)
        {
            NewArtworkContract contract = NewArtworkBusinessLayer.Instance.GetNewArtworkAdminDetails(objDict);
            return contract;
        }

        /// <summary>
        /// Gets all vendor code.
        /// </summary>
        /// <param name="q">The q.</param>
        /// <returns>the string</returns>
        protected string GetAllVendorCode(string q)
        {
            string data = NewArtworkBusinessLayer.Instance.GetAllVendorCode(q);
            return data;
        }

        /// <summary>
        /// Deletes the arwork.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Action Status</returns>
        public ActionStatus DeleteArworkById(int id)
        {
            ActionStatus status = new ActionStatus();
            status = NewArtworkBusinessLayer.Instance.DeleteArtwork(id);
            return status;
        }
    }
}