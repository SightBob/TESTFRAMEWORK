using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TESTFRAMEWORK.Filters;
using TESTFRAMEWORK.Models;

namespace TESTFRAMEWORK.Controllers
{
    public class DashboardController : Controller
    {
        private Research_DBEntities1 db = new Research_DBEntities1();

        // GET: Dashboard
        [AuthorizeUser]
        public ActionResult Index()
        {
            var powerBILinks = db.PowerBI_Links.ToList(); // ดึงทั้งหมด
            return View(powerBILinks);
        }

        [HttpPost]
        public ActionResult Create(PowerBI_Links model)
        {
            if (ModelState.IsValid)
            {
                var newLink = new PowerBI_Links
                {
                    URL = model.URL
                };
                db.PowerBI_Links.Add(newLink);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View("Index", db.PowerBI_Links.ToList());
        }

        [HttpPost]
        public ActionResult Update(int linkID, string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                ModelState.AddModelError("URL", "URL ไม่สามารถเป็นค่าว่างได้");
            }

            if (ModelState.IsValid)
            {
                var link = db.PowerBI_Links.Find(linkID);
                if (link == null)
                {
                    return HttpNotFound();
                }

                link.URL = url;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View("Index", db.PowerBI_Links.ToList());
        }
    }
}
