using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TESTFRAMEWORK.Filters;

namespace TESTFRAMEWORK.Controllers
{
    public class DashboardController : Controller
    {
        // GET: DashBoard
        [AuthorizeUser]
        public ActionResult Index()
        {
            return View();
        }
    }
}