using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TESTFRAMEWORK.Filters
{
    public class AuthorizeUserAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = HttpContext.Current.Session["UserId"];
            if (session == null)
            {
                // ถ้าไม่ได้ล็อกอิน, ให้ Redirect ไปหน้า Login
                filterContext.Result = new RedirectResult("/Auth/Login");
            }
            base.OnActionExecuting(filterContext);
        }

    }
}