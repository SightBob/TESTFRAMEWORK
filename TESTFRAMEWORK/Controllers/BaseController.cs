using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TESTFRAMEWORK.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // ถ้าไม่ได้ Login ให้ Redirect ไปที่หน้า Login
            if (Session["UserId"] == null)
            {
                filterContext.Result = RedirectToAction("Login", "Auth");
            }
            base.OnActionExecuting(filterContext);
        }
    }
}