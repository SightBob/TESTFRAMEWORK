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
            // ยกเว้นการตรวจสอบสำหรับ Auth Controller ในหน้า Login และ Register
            bool isAuthController = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName == "Auth";
            bool isAuthAction = filterContext.ActionDescriptor.ActionName == "Login" || 
                                filterContext.ActionDescriptor.ActionName == "Register";

            // ตรวจสอบการ Login เฉพาะหน้าที่ไม่ได้อยู่ในข้อยกเว้น
            if (Session["UserId"] == null && !(isAuthController && isAuthAction))
            {
                filterContext.Result = RedirectToAction("Login", "Auth");
            }
            
            base.OnActionExecuting(filterContext);
        }
    }
}