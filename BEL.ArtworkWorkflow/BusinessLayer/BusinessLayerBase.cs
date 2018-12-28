namespace BEL.ArtworkWorkflow.BusinessLayer
{
    using CommonDataContract;
    using DataAccessLayer;
    using Microsoft.SharePoint.Client;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web;

    /// <summary>
    /// Business Layer Base
    /// </summary>
    public class BusinessLayerBase
    {
        /// <summary>
        /// The Context
        /// </summary>
        public static ClientContext Context = null;

        /// <summary>
        /// The web
        /// </summary>
        public static Web Web = null;

        /// <summary>
        /// Business Layer Base
        /// </summary>
        protected BusinessLayerBase()
        {
            try
            {
                string siteURL = BELDataAccessLayer.Instance.GetSiteURL(SiteURLs.ARTWORKSITEURL);
                if (!string.IsNullOrEmpty(siteURL))
                {
                    Context = BELDataAccessLayer.Instance.CreateClientContext(siteURL);
                    Web = BELDataAccessLayer.Instance.CreateWeb(Context);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}