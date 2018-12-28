namespace BEL.ArtworkWorkflow
{
    using BEL.ArtworkWorkflow.Common;
    using System.Web;
    using System.Web.Mvc;
    
    /// <summary>
    /// Filter config
    /// </summary>
    public class FilterConfig
    {
        /// <summary>
        /// Register Global Filters
        /// </summary>
        /// <param name="filters">filter parameter</param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            if (filters != null)
            {
                filters.Add(new HandleErrorAttribute());
                filters.Add(new SessionExpiration());
            }
        }
    }
}
