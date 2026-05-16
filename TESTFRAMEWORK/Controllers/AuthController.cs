using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TESTFRAMEWORK.Models;
using TESTFRAMEWORK.Filters;
using BCrypt.Net;
using System.Web.Security;

namespace TESTFRAMEWORK.Controllers
{
    public class AuthController : Controller
    {
        private Research_DBEntities db = new Research_DBEntities();

        // GET: Auth/Login
        public ActionResult Login()
        {
            if (Session["UserId"] != null)
            {
                return RedirectToAction("Index", "Research");
            }
            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "กรุณากรอก Username และ Password";
                return View();
            }

            var user = db.Users.FirstOrDefault(u => u.Username == username);

            try
            {
                // ตรวจสอบรหัสผ่านด้วย BCrypt
                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    FormsAuthentication.SetAuthCookie(user.Username, false);
                    Session["UserId"] = user.UserId;
                    Session["Role"] = user.Role;
                    Session["Username"] = user.Username;
                    return RedirectToAction("Index", "Dashboard");
                }
            }
            catch (BCrypt.Net.SaltParseException)
            {
                ViewBag.Error = "รูปแบบรหัสผ่านของบัญชีนี้ไม่สามารถตรวจสอบได้ กรุณารีเซ็ตรหัสผ่าน";
                return View();
            }

            ViewBag.Error = "Username หรือ Password ไม่ถูกต้อง";
            return View();
        }

        // Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: Auth/ChangePassword
        [AuthorizeUser]
        public ActionResult ChangePassword()
        {
            return View();
        }

        // POST: Auth/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeUser]
        public ActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = Session["UserId"];
            if (userId == null)
                return Json(new { success = false, message = "กรุณาเข้าสู่ระบบใหม่" });

            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
                return Json(new { success = false, message = "กรุณากรอกข้อมูลให้ครบถ้วน" });

            if (newPassword.Length < 6)
                return Json(new { success = false, message = "รหัสผ่านใหม่ต้องมีอย่างน้อย 6 ตัวอักษร" });

            if (newPassword != confirmPassword)
                return Json(new { success = false, message = "รหัสผ่านใหม่ไม่ตรงกัน" });

            var user = db.Users.Find(userId);
            if (user == null)
                return Json(new { success = false, message = "ไม่พบข้อมูลผู้ใช้" });

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                return Json(new { success = false, message = "รหัสผ่านปัจจุบันไม่ถูกต้อง" });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            db.SaveChanges();

            return Json(new { success = true, message = "เปลี่ยนรหัสผ่านเรียบร้อยแล้ว" });
        }

        // GET: Auth/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string username, string password, string confirmPassword, string role)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                return Json(new { success = false, message = "กรุณากรอกข้อมูลให้ครบถ้วน" });
            }

            if (password != confirmPassword)
            {
                return Json(new { success = false, message = "รหัสผ่านไม่ตรงกัน" });
            }

            if (db.Users.Any(u => u.Username == username))
            {
                return Json(new { success = false, message = "Username นี้ถูกใช้ไปแล้ว" });
            }

            // เข้ารหัสรหัสผ่านด้วย BCrypt (ใช้เวอร์ชันที่ปลอดภัยและไม่กำหนด salt เอง)
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password); // ใช้ work factor 12 โดย default

            var newUser = new Users
            {
                Username = username,
                PasswordHash = hashedPassword,
                Role = string.IsNullOrEmpty(role) ? "User" : role
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            return Json(new { success = true, message = "สมัครสมาชิกสำเร็จ!" });
        }
    }
}
