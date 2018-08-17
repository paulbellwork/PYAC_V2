//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Mvc;
//using Microsoft.AspNet.Identity;
//using Microsoft.AspNet.Identity.EntityFramework;
//using Microsoft.Owin.Security;
//using ACQC.Models;
//using System.Globalization;
//using WebApplication1.Controllers;
//using System.Web.Security;
//using ACQC.Ldap;

//namespace ACQC.Controllers
//{
//    [MyAuthorize]
//    public class Account 
//    {
//        // GET: /Account/Login
//        [AllowAnonymous]
//        public ActionResult Login(string returnUrl)
//        {

//            ViewBag.ReturnUrl = returnUrl;
//            return View();
//        }

//        // POST: /Account/Login
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public ActionResult Login(LoginViewModel model, string returnUrl)
//        {


//            if (ModelState.IsValid && !Request.IsAuthenticated && Membership.ValidateUser(model.UserName, model.Password))
//            {
//                DBConnection db = new DBConnection();
//                if (db.Open())
//                {
//                    Session["DBConnection"] = db;
//                    Session["User"] = model.UserName;
//                    FormsAuthentication.SetAuthCookie(model.UserName, false);
//                    return RedirectToLocal("~/Home/CureCycles");
//                }
//                else
//                    ModelState.AddModelError("", "DB Invalid.");
//            }
//            else
//                ModelState.AddModelError("", "Invalid username or password.");
//            return View(model);
//        }

//        // POST: /Account/LogOff
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult LogOff()
//        {
//            DBConnection conn = (DBConnection)Session["DBConnection"];
//            if (conn != null)
//                conn.Close();
//            FormsAuthentication.SignOut();
//            Session.Abandon();
//            return Redirect("Login");
//        }

//        // GET: /Account/ExternalLoginFailure
//        [AllowAnonymous]
//        public ActionResult ExternalLoginFailure()
//        {
//            return View();
//        }

//        #region Applications auxiliaires
//        // Used for XSRF protection when adding external logins
//        private const string XsrfKey = "XsrfId";

//        private void AddErrors(IdentityResult result)
//        {
//            foreach (var error in result.Errors)
//            {
//                ModelState.AddModelError("", error);
//            }
//        }

//        public enum ManageMessageId
//        {
//            ChangePasswordSuccess,
//            SetPasswordSuccess,
//            RemoveLoginSuccess,
//            Error
//        }

//        private ActionResult RedirectToLocal(string returnUrl)
//        {
//            if (Url.IsLocalUrl(returnUrl))
//            {
//                return Redirect(returnUrl);
//            }
//            else
//            {
//                return RedirectToAction("Index", "Home");
//            }
//        }

//        private class ChallengeResult : HttpUnauthorizedResult
//        {
//            public ChallengeResult(string provider, string redirectUri) : this(provider, redirectUri, null)
//            {
//            }

//            public ChallengeResult(string provider, string redirectUri, string userId)
//            {
//                LoginProvider = provider;
//                RedirectUri = redirectUri;
//                UserId = userId;
//            }

//            public string LoginProvider { get; set; }
//            public string RedirectUri { get; set; }
//            public string UserId { get; set; }

//            public override void ExecuteResult(ControllerContext context)
//            {
//                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
//                if (UserId != null)
//                {
//                    properties.Dictionary[XsrfKey] = UserId;
//                }
//                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
//            }
//        }
//        #endregion
//    }
//}