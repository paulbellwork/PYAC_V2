//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;

//namespace ACQC.Ldap
//{
//    // The class handles Authorization errors. To use it, instead of saying [Authorize]
//    // before a Controller class or a method, use [MyAuthorize]
//    public class MyAuthorizeAttribute : AuthorizeAttribute
//    {
//        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
//        {
//            if (!filterContext.HttpContext.Request.IsAuthenticated)
//                filterContext.Result = new ViewResult { ViewName = "~/Views/Account/Login.cshtml" };
//            else
//                filterContext.Result = new ViewResult { ViewName = "~/Views/Shared/401.cshtml" };
//        }
//    }

//    // Used as a string "enum" for harcoded roles from Ldap
//    public static class LdapRoles
//    {
//        public const string ReadOnly = "MIR_ACQC_READ";
//        public const string Inspector = "MIR_ACQC_INSP";
//        public const string Admin = "MIR_ACQC_ADMIN";
//    }
//}