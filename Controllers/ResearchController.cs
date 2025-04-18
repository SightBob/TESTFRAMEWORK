using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication.Models;
using WebApplication.ViewModels;

namespace WebApplication.Controllers
{
    public class ResearchController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ResearchProjectViewModel model, IEnumerable<HttpPostedFileBase> files)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns(model);
                return View(model);
            }

            // Custom Validation เพิ่มเติม
            if (model.ResearchProject.ECExpirationDate < model.ResearchProject.ECApprovalDate)
            {
                ModelState.AddModelError("ResearchProject.ECExpirationDate", "วันหมดอายุ EC ต้องไม่น้อยกว่าวันที่อนุมัติ");
            }

            if (model.ResearchProject.ResearchExpirationDate < model.ResearchProject.ResearchApprovalDate)
            {
                ModelState.AddModelError("ResearchProject.ResearchExpirationDate", "วันหมดอายุเก็บข้อมูลต้องไม่น้อยกว่าวันที่อนุมัติ");
            }

            if (files != null && files.Any(f => f != null))
            {
                foreach (var file in files)
                {
                    if (file.ContentLength > 5 * 1024 * 1024) // 5MB
                    {
                        ModelState.AddModelError("", $"ไฟล์ {file.FileName} มีขนาดเกิน 5MB");
                    }

                    var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                    var extension = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "อนุญาตให้อัปโหลดเฉพาะไฟล์ PDF และ Word เท่านั้น");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                LoadDropdowns(model);
                return View(model);
            }

            // ... โค้ดเดิม ...
        }
    }
} 