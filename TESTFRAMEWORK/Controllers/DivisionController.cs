using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TESTFRAMEWORK.Filters;
using TESTFRAMEWORK.Models;

namespace TESTFRAMEWORK.Controllers
{
    public class DivisionController : Controller
    {
        private Research_DBEntities1 db = new Research_DBEntities1();

        // 1) หน้า Index: แสดงข้อมูลแผนก พร้อมชื่อฝ่าย
        [AuthorizeUser]
        public ActionResult Index()
        {
            var list = (from di in db.divisions
                        join dept in db.departments on di.department_id equals dept.id
                        join status in db.Status_tbl on di.Status equals status.StatusID
                        select new DivisionViewModel
                        {
                            Id = di.id,
                            DivisionName = di.name,
                            DepartmentName = dept.name,
                            StatusId = di.Status,  // ดึงค่า Status
                            StatusDepartment = dept.Status,
                            StatusName = status.StatusName  // ชื่อสถานะ
                        })
                        .ToList();

            return View(list);
        }


        // ============== CREATE ==============
        [HttpGet]
        public PartialViewResult CreatePartial()
        {
            // สำหรับฟอร์มสร้างใหม่
            var viewModel = new DivisionViewModel();

            // Dropdown ของ departments ทั้งหมด
            ViewBag.DepartmentList = new SelectList(
                db.departments.ToList(),
                "id",
                "name"
            );

            // เพิ่ม Dropdown สำหรับ Status
            ViewBag.StatusList = new SelectList(
                db.Status_tbl.ToList(),
                "StatusID",
                "StatusName"
            );

            return PartialView("CreatePartial", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateDivision(DivisionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // โหลด dropdown ใหม่เมื่อ ModelState ไม่ถูกต้อง
                ViewBag.DepartmentList = new SelectList(db.departments, "id", "name", model.DepartmentId);
                ViewBag.StatusList = new SelectList(db.Status_tbl, "StatusID", "StatusName", model.StatusId);
                return PartialView("CreatePartial", model);
            }

            var newDiv = new division
            {
                name = model.DivisionName,  // ✅ ชื่อแผนก
                department_id = model.DepartmentId,  // ✅ ID ของ Department
                Status = model.StatusId  // ✅ สถานะ
            };

            db.divisions.Add(newDiv);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // ============== EDIT ==============
        public PartialViewResult EditPartial(int id)
        {
            var division = db.divisions.Find(id);
            if (division == null)
            {
                return PartialView("EditPartial", new DivisionViewModel());
            }

            var viewModel = new DivisionViewModel
            {
                Id = division.id,
                DivisionName = division.name,
                DepartmentId = division.department_id,
                StatusId = division.Status
            };

            // Populate Dropdowns
            ViewBag.DepartmentList = new SelectList(db.departments.ToList(), "id", "name", viewModel.DepartmentId);
            ViewBag.StatusList = new SelectList(db.Status_tbl.ToList(), "StatusID", "StatusName", viewModel.StatusId);

            return PartialView("EditPartial", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveDivision(DivisionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.DepartmentList = new SelectList(db.departments, "id", "name", model.DepartmentId);
                ViewBag.WorkGroupList = new SelectList(db.work_groups, "id", "name", model.WorkGroupId);
                ViewBag.StatusList = new SelectList(db.Status_tbl, "StatusID", "StatusName", model.StatusId);
                return PartialView("EditPartial", model);
            }

            var div = db.divisions.Find(model.Id);
            if (div != null)
            {
                div.name = model.DivisionName;
                div.department_id = model.DepartmentId;
                div.Status = model.StatusId; // ✅ บันทึกสถานะ
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteDivision(int id)
        {
            var div = db.divisions.Find(id);
            if (div == null)
            {
                return Json(new { success = false, message = "ไม่พบแผนกที่ต้องการลบ" });
            }

            db.divisions.Remove(div);
            db.SaveChanges();

            return Json(new { success = true });
        }

    }
}
