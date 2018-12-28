namespace BEL.ArtworkWorkflow.Controllers
{
    using BEL.CommonDataContract;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Http;
    using System.Web.Helpers;
    using BEL.ArtworkWorkflow.Common;

    /// <summary>
    /// Master Data Controller
    /// </summary>
    public class MasterController : BaseController
    {
        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns>Index View</returns>
        public ActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// Gets the masters.
        /// </summary>
        /// <param name="q">The q.</param>
        /// <param name="d">The d.</param>
        /// <returns>
        /// list of master data
        /// </returns>
        public JsonResult GetUsers(string q, string d = null)
        {
            ////dal
            ////using (CommonServiceClient client = new CommonServiceClient())
            ////{
            ////var newList = client.GetUsers(q, d);
            ////JsonResult jResult = this.Json((from n in newList select new { id = n.Key, name = n.Value }).ToList(), JsonRequestBehavior.AllowGet);
            ////return jResult;
            ////}
            return null;
        }

        /// <summary>
        /// Gets the user information.
        /// </summary>
        /// <param name="userEmail">The user email.</param>
        /// <returns>
        /// User Information
        /// </returns>
        public JsonResult GetUserInfo(string userEmail)
        {
            ////using (CommonServiceClient client = new CommonServiceClient())
            ////{
            ////    var user = client.GetUserByEmail(userEmail);
            ////    return this.Json(user, JsonRequestBehavior.AllowGet);
            ////}
            ////DAL
            return null;
        }

        /// <summary>
        /// Errors the specified MSG.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <returns>
        /// error view
        /// </returns>
        public ActionResult Error(string msg)
        {
            return this.View();
        }

        /// <summary>
        /// Nots the authorize.
        /// </summary>
        /// <returns>NotAuthorize View</returns>
        public ActionResult NotAuthorize()
        {
            return this.View();
        }

        /// <summary>
        /// Get Token for CSRF
        /// </summary>
        /// <returns>Json string</returns>
        public JsonResult GetTocken()
        {
            try
            {
                return this.Json("Verified", JsonRequestBehavior.AllowGet);
                ////string cookieToken, formToken;
                ////AntiForgery.GetTokens(null, out cookieToken, out formToken);
                ////HttpContext.Cache[this.CurrentUser.UserEmail + "_formToken"] = formToken;
                ////return this.Json(cookieToken + ":" + formToken, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Json("XYZ123", JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Keep Session Live
        /// </summary>
        /// <returns>Result live</returns>
        [System.Web.Mvc.HttpPost]
        public JsonResult KeepSessionAlive()
        {
            return new JsonResult { Data = "Success" };
        }
 }
}