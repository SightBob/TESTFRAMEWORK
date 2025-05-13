using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TESTFRAMEWORK.Filters;
using TESTFRAMEWORK.Models;

namespace TESTFRAMEWORK.Controllers
{
    public class ManageUserController : Controller
    {
        private Research_DBEntities1 db = new Research_DBEntities1();

        // GET: ManageUser
        [AuthorizeUser]
        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

        [HttpGet]
        public JsonResult GetUser(int id)
        {
            var user = db.Users.Find(id);
            if (user == null)
                return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                user.UserId,
                user.Username,
                user.Role
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateRole(int id, string role)
        {
            var user = db.Users.Find(id);
            if (user == null)
                return Json(new { success = false });

            user.Role = role;
            db.SaveChanges();

            return Json(new { success = true });
        }

        // Edit Role of user
        public ActionResult Edit(int id)
        {
            var user = db.Users.Find(id);
            if (user == null)
                return HttpNotFound();

            var viewModel = new User
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(User model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(model.UserId);
                if (user == null)
                    return HttpNotFound();

                user.Role = model.Role;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(model);
        }

        // Delete user
        public ActionResult Delete(int id)
        {
            var user = db.Users.Find(id);
            if (user == null)
                return HttpNotFound();

            db.Users.Remove(user);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}