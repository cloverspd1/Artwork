namespace BEL.ArtworkWorkflow
{
    using BEL.CommonDataContract;
    using BEL.ArtworkWorkflow;
    using BEL.ArtworkWorkflow.Common;
    using BEL.ArtworkWorkflow.Controllers;
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    /// Mvc Exception Handler
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class MvcExceptionHandler : FilterAttribute, IExceptionFilter
    {
        /// <summary>
        /// On Exception
        /// </summary>
        /// <param name="filterContext">Filter Context</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "NA")]
        public void OnException(ExceptionContext filterContext)
        {
            try
            {
                if (filterContext != null && filterContext.Exception != null)
                {
                    HttpContext httpContext = HttpContext.Current;
                    var currentRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));
                    var currentController = string.Empty;
                    var currentAction = string.Empty;
                                      
                    var ex = filterContext.Exception;
                    string id = Guid.NewGuid().ToString();
                    
                    Logger.Error("Id: " + id);
                    Logger.Error("StatusCode: " + filterContext.HttpContext.Response.StatusCode);
                    Logger.Error("Controller: " + currentController);
                    Logger.Error("Action: " + currentAction);
                    Logger.Error(ex);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}