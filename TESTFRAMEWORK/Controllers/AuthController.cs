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
        // GET: Auth
        private Research_DBEntities1 db = new Research_DBEntities1();

        // GET: Auth/Login
        public ActionResult Login()
        {
            if (Session["UserId"] != null)
            {
                return RedirectToAction("Index", "Research"); // ✅ ถ้าล็อกอินอยู่แล้ว ให้ไปที่หน้า Research
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
            
            // ตรวจสอบรหัสผ่านด้วย BCrypt
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                FormsAuthentication.SetAuthCookie(user.Username, false);
                Session["UserId"] = user.UserId;
                return RedirectToAction("Index", "Research");
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

            // ตรวจสอบว่ามี username นี้อยู่ในระบบแล้วหรือยัง
            if (db.Users.Any(u => u.Username == username))
            {
                ViewBag.Error = "Username นี้ถูกใช้ไปแล้ว";
                return View();
            }

            // เข้ารหัสรหัสผ่านก่อนบันทึก
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, 12);

            var newUser = new User
            {
                Username = username,
                PasswordHash = hashedPassword
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            ViewBag.Success = "สมัครสมาชิกสำเร็จ! กรุณาเข้าสู่ระบบ";
            return RedirectToAction("Login");
        }

    }
}