using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TESTFRAMEWORK.Models;

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
        public ActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "กรุณากรอก Username และ Password";
                return View();
            }

            // ตรวจสอบ Username และ PasswordHash จากฐานข้อมูล
            var user = db.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == password);

            if (user != null)
            {
                // เก็บ Session
                Session["UserId"] = user.UserId;
                Session["Username"] = user.Username;
                return RedirectToAction("Index", "Research");
            }
            else
            {
                ViewBag.Error = "Username หรือ Password ไม่ถูกต้อง";
                return View();
            }
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

            // สร้าง User ใหม่
            var newUser = new User
            {
                Username = username,
                PasswordHash = password // **ควรเข้ารหัสรหัสผ่านในระบบจริง**
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            ViewBag.Success = "สมัครสมาชิกสำเร็จ! กรุณาเข้าสู่ระบบ";
            return RedirectToAction("Login");
        }

    }
}