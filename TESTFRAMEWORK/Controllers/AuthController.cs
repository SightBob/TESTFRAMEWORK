using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TESTFRAMEWORK.Models;
using BCrypt.Net;
using System.Web.Security;

namespace TESTFRAMEWORK.Controllers
{
    public class AuthController : Controller
    {
        private Research_DBEntities1 db = new Research_DBEntities1();

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

        // GET: Auth/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string username, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.Error = "กรุณากรอกข้อมูลให้ครบถ้วน";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "รหัสผ่านไม่ตรงกัน";
                return View();
            }

            if (db.Users.Any(u => u.Username == username))
            {
                ViewBag.Error = "Username นี้ถูกใช้ไปแล้ว";
                return View();
            }

            // เข้ารหัสรหัสผ่านด้วย BCrypt (ใช้เวอร์ชันที่ปลอดภัยและไม่กำหนด salt เอง)
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password); // ใช้ work factor 12 โดย default

            var newUser = new User
            {
                Username = username,
                PasswordHash = hashedPassword,
                Role = "User"
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            ViewBag.Success = "สมัครสมาชิกสำเร็จ! กรุณาเข้าสู่ระบบ";
            return RedirectToAction("Login");
        }
    }
}
