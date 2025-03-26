using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TESTFRAMEWORK.Models;

namespace TESTFRAMEWORK.Controllers
{
    public class ChartController : Controller
    {
        // GET: Chart
        private Research_DBEntities db = new Research_DBEntities();

        // ✅ GET: แสดงตาราง Departments
        public ActionResult Index()
        {
            var list = (from dept in db.departments
                        join wg in db.work_groups on dept.work_group_id equals wg.id
                        join s in db.Status_tbl on dept.Status equals s.StatusID
                        select new DepartmentViewModel
                        {
                            Id = dept.id,
                            DepartmentName = dept.name,
                            WorkGroupId = dept.work_group_id,
                            WorkGroupName = wg.name,
                            StatusId = dept.Status,
                            StatusName = s.StatusName
                        }).ToList();

            return View(list); // ✅ ตอนนี้ list เป็น List<DepartmentViewModel>
        }



        // ✅ GET: แสดงฟอร์มแก้ไข
        public PartialViewResult EditPartial(int id)
        {
            var dept = db.departments.Find(id);
            if (dept == null)
            {
                return PartialView("EditPartial", new DepartmentViewModel());
            }

            var viewModel = new DepartmentViewModel
            {
                Id = dept.id,
                DepartmentName = dept.name,
                WorkGroupId = dept.work_group_id,
                StatusId = dept.Status
            };

            // โหลด Dropdowns
            ViewBag.WorkGroupList = new SelectList(db.work_groups.ToList(), "id", "name", viewModel.WorkGroupId);
            ViewBag.StatusList = new SelectList(db.Status_tbl.ToList(), "StatusID", "StatusName", viewModel.StatusId);

            return PartialView("EditPartial", viewModel);
        }

        // ✅ POST: บันทึกการแก้ไข
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveDepartment(DepartmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.WorkGroupList = new SelectList(db.work_groups.ToList(), "id", "name", model.WorkGroupId);
                ViewBag.StatusList = new SelectList(db.Status_tbl.ToList(), "StatusID", "StatusName", model.StatusId);
                return PartialView("EditPartial", model);
            }

            var dept = db.departments.Find(model.Id);
            if (dept != null)
            {
                dept.name = model.DepartmentName;
                dept.work_group_id = model.WorkGroupId;
                dept.Status = model.StatusId; // ✅ อัปเดตสถานะ
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // ============== CREATE ==============
        // ============== CREATE ==============
        [HttpGet]
        public PartialViewResult CreatePartial()
        {
            // สำหรับฟอร์มสร้างใหม่
            var viewModel = new DepartmentViewModel();

            // Dropdown ของ Work Groups ทั้งหมด
            ViewBag.WorkGroupList = new SelectList(
                db.work_groups.ToList(),
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
        public ActionResult CreateDepartment(DepartmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // โหลด dropdown ใหม่เมื่อ ModelState ไม่ถูกต้อง
                ViewBag.WorkGroupList = new SelectList(db.work_groups, "id", "name", model.WorkGroupId);
                ViewBag.StatusList = new SelectList(db.Status_tbl, "StatusID", "StatusName", model.StatusId);
                return PartialView("CreatePartial", model);
            }

            var newDept = new department
            {
                name = model.DepartmentName,  // ✅ ชื่อแผนก
                work_group_id = model.WorkGroupId,  // ✅ ID ของกลุ่มงาน
                Status = model.StatusId  // ✅ สถานะ
            };

            db.departments.Add(newDept);
            db.SaveChanges();

            return RedirectToAction("Index");
        }


    }
}