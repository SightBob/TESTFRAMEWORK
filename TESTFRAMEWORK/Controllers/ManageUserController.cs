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
        private Research_DBEntities db = new Research_DBEntities();

        // GET: ManageUser
        [AuthorizeUser]
        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

        [HttpGet]
        [AuthorizeUser]
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
        [ValidateAntiForgeryToken]
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
        [AuthorizeUser]
        public ActionResult Edit(int id)
        {
            var user = db.Users.Find(id);
            if (user == null)
                return HttpNotFound();

            var viewModel = new Users
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Users model)
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeUser]
        public JsonResult Delete(int id)
        {
            try
            {
                var user = db.Users.Find(id);
                if (user == null)
                    return Json(new { success = false, message = "ไม่พบผู้ใช้" });

                db.Users.Remove(user);
                db.SaveChanges();

                return Json(new { success = true, message = "ลบผู้ใช้สำเร็จ" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + ex.Message });
            }
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