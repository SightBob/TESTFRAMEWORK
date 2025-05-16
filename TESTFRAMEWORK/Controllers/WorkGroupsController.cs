using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TESTFRAMEWORK.Filters;
using TESTFRAMEWORK.Models;

namespace TESTFRAMEWORK.Controllers
{
    public class WorkGroupsController : Controller
    {
        // GET: WorkGroups
        private Research_DBEntities1 db = new Research_DBEntities1();

        // ✅ GET: แสดงตาราง Work Groups
        [AuthorizeUser]
        public ActionResult Index()
        {
            var list = (from wg in db.work_groups
                        join s in db.Status_tbl on wg.Status equals s.StatusID
                        select new WorkGroupViewModel
                        {
                            Id = wg.id,
                            WorkGroupName = wg.name,
                            StatusId = wg.Status,
                            StatusName = s.StatusName
                        }).ToList();

            return View(list);
        }

        // ✅ GET: ฟอร์มเพิ่มข้อมูล (โหลดเป็น Partial View)
        public PartialViewResult CreatePartial()
        {
            ViewBag.StatusList = new SelectList(db.Status_tbl.ToList(), "StatusID", "StatusName");
            return PartialView("Create", new WorkGroupViewModel());
        }

        // ✅ POST: บันทึกข้อมูลที่เพิ่มใหม่
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateWorkGroup(WorkGroupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.StatusList = new SelectList(db.Status_tbl.ToList(), "StatusID", "StatusName", model.StatusId);
                return PartialView("Create", model);
            }

            var wg = new work_groups
            {
                name = model.WorkGroupName,
                Status = "1"
            };

            db.work_groups.Add(wg);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // ✅ GET: ฟอร์มแก้ไขข้อมูล (โหลดเป็น Partial View)
        public PartialViewResult EditPartial(int id)
        {
            var wg = db.work_groups.Find(id);
            if (wg == null)
            {
                return PartialView("Edit", new WorkGroupViewModel());
            }

            var viewModel = new WorkGroupViewModel
            {
                Id = wg.id,
                WorkGroupName = wg.name,
                StatusId = wg.Status
            };

            ViewBag.StatusList = new SelectList(db.Status_tbl.ToList(), "StatusID", "StatusName", viewModel.StatusId);
            return PartialView("Edit", viewModel);
        }

        // ✅ POST: บันทึกข้อมูลที่แก้ไข
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveWorkGroup(WorkGroupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.StatusList = new SelectList(db.Status_tbl.ToList(), "StatusID", "StatusName", model.StatusId);
                return PartialView("Edit", model);
            }

            var wg = db.work_groups.Find(model.Id);
            if (wg != null)
            {
                wg.name = model.WorkGroupName;
                wg.Status = model.StatusId;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}