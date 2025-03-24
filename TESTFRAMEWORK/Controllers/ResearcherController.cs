using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TESTFRAMEWORK.Models;

namespace TESTFRAMEWORK.Controllers
{
    public class ResearcherController : Controller
    {
        private Research_DBEntities db = new Research_DBEntities();

        // ✅ GET: Researcher/Index (หน้าแสดงรายชื่อนักวิจัย)
        public ActionResult Index()
        {
            var internalResearchers = db.Researcher_tbl
                 .Where(r => r.work_group_id != null)
                .Select(r => new ResearcherViewModel
                {
                    ResearcherNumber = r.ResearcherNumber ?? "-",
                    Title = r.title ?? "",
                    Name = r.Name ?? "-",
                    WorkGroupName = db.work_groups
                        .Where(w => w.id == r.work_group_id)
                        .Select(w => w.name)
                        .FirstOrDefault() ?? "-",
                    DepartmentName = db.departments
                        .Where(d => d.id == r.department_id)
                        .Select(d => d.name)
                        .FirstOrDefault() ?? "-",
                    DivisionName = db.divisions
                        .Where(di => di.id == r.division_id)
                        .Select(di => di.name)
                        .FirstOrDefault() ?? "-",
                    TypeResearchName = db.TypeResearches
                        .Where(t => t.id == r.TypeResearch)
                        .Select(t => t.type_name)
                        .FirstOrDefault() ?? "-",
                    OtherInfo = r.OtherInfo
                }).ToList();

            return View(internalResearchers);
        }

        public ActionResult ExternalResearchers()
        {
            var externalResearchers = db.Researcher_tbl
                .Where(r => r.work_group_id == null)
                .Select(r => new ResearcherViewModel
                {
                    ResearcherNumber = r.ResearcherNumber ?? "-",
                    Title = r.title ?? "",
                    Name = r.Name ?? "-",
                    TypeResearchName = db.TypeResearches
                        .Where(t => t.id == r.TypeResearch)
                        .Select(t => t.type_name)
                        .FirstOrDefault() ?? "-",
                    OtherInfo = r.OtherInfo,
                }).ToList();

            return View(externalResearchers);
        }

        // ✅ GET: Researcher/Create (แสดงฟอร์มเพิ่มนักวิจัย)
        public ActionResult CreateInternal()
        {
            LoadDropdowns();
            return View(new ResearcherViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateInternal(ResearcherViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns();
                return View(model);
            }
            try
            {
                // ✅ ตรวจสอบค่า ถ้าเป็น null ให้เป็น 0
                int work_group_Id = model.WorkGroupId.GetValueOrDefault(0);
                int department_Id = model.DepartmentId.GetValueOrDefault(0);
                int divisionId = model.DivisionId.GetValueOrDefault(0);
                int typeResearchId = model.TypeResearchId.GetValueOrDefault(0);

                // ✅ Debug เช็คค่าก่อนบันทึก
                System.Diagnostics.Debug.WriteLine($"[INFO] Creating Researcher: ResearcherNumber={model.ResearcherNumber}, WorkGroupId={work_group_Id}, DepartmentId={department_Id}, DivisionId={divisionId}, TypeResearchId={typeResearchId}");

                var researcher = new Researcher_tbl
                {
                    ResearcherNumber = model.ResearcherNumber,
                    title = model.Title,
                    Name = model.Name,
                    work_group_id = work_group_Id != 0 ? (int?)work_group_Id : null,
                    department_id = department_Id != 0 ? (int?)department_Id : null,
                    division_id = divisionId != 0 ? (int?)divisionId : null,
                    TypeResearch = typeResearchId != 0 ? (int?)typeResearchId : 4,
                    OtherInfo = model.OtherInfo ?? ""
                };

                db.Researcher_tbl.Add(researcher);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                // ✅ ตรวจสอบ Model Validation Errors
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        ModelState.AddModelError("", $"[Validation Error] Property: {validationError.PropertyName}, Error: {validationError.ErrorMessage}");
                        System.Diagnostics.Debug.WriteLine($"[Validation Error] Property: {validationError.PropertyName}, Error: {validationError.ErrorMessage}");
                    }
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException dbUpdateEx)
            {
                // ✅ ตรวจสอบ SQL Constraint Errors เช่น Primary Key ซ้ำ, Foreign Key ผิด
                if (dbUpdateEx.InnerException != null)
                {
                    ModelState.AddModelError("", $"[DB Update Error] {dbUpdateEx.InnerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"[DB Update Error] {dbUpdateEx.InnerException.Message}");
                }
                else
                {
                    ModelState.AddModelError("", "[DB Update Error] เกิดข้อผิดพลาดขณะบันทึกข้อมูล");
                }
            }
            catch (Exception ex)
            {
                // ✅ แสดง Inner Exception ถ้ามี
                if (ex.InnerException != null)
                {
                    ModelState.AddModelError("", $"[ERROR] {ex.InnerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"[ERROR] {ex.InnerException.Message}");
                }
                else
                {
                    ModelState.AddModelError("", $"[ERROR] {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[ERROR] {ex.Message}");
                }
            }

            // โหลด Dropdown ใหม่และแสดงหน้า View พร้อม Error
            LoadDropdowns();
            return View(model);

        }




        // ✅ ดึงรายชื่อ `departments` ตาม `work_group_id`
        public JsonResult GetDepartmentsByWorkGroup(int workGroupId)
        {
            var departments = db.departments
                                .Where(d => d.work_group_id == workGroupId)
                                .Select(d => new
                                {
                                    Value = d.id,
                                    Text = d.name
                                }).ToList();

            return Json(departments, JsonRequestBehavior.AllowGet);
        }

        // ✅ ดึงรายชื่อ `divisions` ตาม `department_id`
        public JsonResult GetDivisionsByDepartment(int departmentId)
        {
            var divisions = db.divisions
                              .Where(di => di.department_id == departmentId)
                              .Select(di => new
                              {
                                  Value = di.id,
                                  Text = di.name
                              }).ToList();

            return Json(divisions, JsonRequestBehavior.AllowGet);
        }

        // ✅ โหลด Dropdown Lists
        private void LoadDropdowns()
        {
            ViewBag.WorkGroupList = new SelectList(db.work_groups, "id", "name");
            ViewBag.DepartmentList = new SelectList(new List<SelectListItem>());
            ViewBag.DivisionList = new SelectList(new List<SelectListItem>());
            ViewBag.TypeResearchList = new SelectList(db.TypeResearches, "id", "type_name");
        }

        // ✅ GET: Researcher/Edit/5 (โหลดข้อมูลนักวิจัย)
        public ActionResult Edit(string id)  // ✅ เปลี่ยนจาก int → string
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var researcher = db.Researcher_tbl
                               .Where(r => r.ResearcherNumber == id)
                               .Select(r => new ResearcherViewModel
                               {
                                   ResearcherNumber = r.ResearcherNumber,
                                   Title = r.title,
                                   Name = r.Name,
                                   WorkGroupId = r.work_group_id,
                                   DepartmentId = r.department_id,
                                   DivisionId = r.division_id,
                                   TypeResearchId = r.TypeResearch
                               })
                               .FirstOrDefault();

            if (researcher == null)
            {
                return HttpNotFound();
            }

            // โหลด Dropdown สำหรับ WorkGroup, Department, Division, TypeResearch
            ViewBag.WorkGroupList = new SelectList(db.work_groups, "id", "name", researcher.WorkGroupId);
            ViewBag.DepartmentList = new SelectList(db.departments.Where(d => d.work_group_id == researcher.WorkGroupId), "id", "name", researcher.DepartmentId);
            ViewBag.DivisionList = new SelectList(db.divisions.Where(di => di.department_id == researcher.DepartmentId), "id", "name", researcher.DivisionId);
            ViewBag.TypeResearchList = new SelectList(db.TypeResearches, "id", "type_name", researcher.TypeResearchId);

            return View(researcher);
        }


        // ✅ โหลด Dropdown ตาม WorkGroup และ Department
        private void LoadDropdowns(int workGroupId, int departmentId)
        {
            ViewBag.WorkGroupList = new SelectList(db.work_groups, "id", "name", workGroupId);
            ViewBag.DepartmentList = new SelectList(db.departments.Where(d => d.work_group_id == workGroupId), "id", "name", departmentId);
            ViewBag.DivisionList = new SelectList(db.divisions.Where(d => d.department_id == departmentId), "id", "name");
            ViewBag.TypeResearchList = new SelectList(db.TypeResearches, "id", "type_name");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(string id)
        {
            var researcher = db.Researcher_tbl.FirstOrDefault(r => r.ResearcherNumber == id);
            if (researcher == null)
            {
                return Json(new { success = false, message = "ไม่พบนักวิจัยที่ต้องการลบ" });
            }

            db.Researcher_tbl.Remove(researcher);
            db.SaveChanges();

            return Json(new { success = true });
        }

        // AJAX method: rename to avoid conflict
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteResearcher(string id)
        {
            var researcher = db.Researcher_tbl.FirstOrDefault(r => r.ResearcherNumber == id);
            if (researcher == null)
            {
                return Json(new { success = false, message = "ไม่พบนักวิจัยที่ต้องการลบ" });
            }

            db.Researcher_tbl.Remove(researcher);
            db.SaveChanges();

            return Json(new { success = true });
        }

        // ✅ POST: Researcher/Edit/5 (บันทึกการแก้ไขนักวิจัย)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ResearcherViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns(model.WorkGroupId.GetValueOrDefault(0), model.DepartmentId.GetValueOrDefault(0));
                return View(model);
            }

            var researcher = db.Researcher_tbl.FirstOrDefault(r => r.ResearcherNumber == model.ResearcherNumber);
            if (researcher == null)
            {
                return HttpNotFound();
            }

            try
            {
                researcher.title = model.Title;
                researcher.Name = model.Name;
                researcher.work_group_id = model.WorkGroupId.GetValueOrDefault(0) != 0 ? model.WorkGroupId : null;
                researcher.department_id = model.DepartmentId.GetValueOrDefault(0) != 0 ? model.DepartmentId : null;
                researcher.division_id = model.DivisionId.GetValueOrDefault(0) != 0 ? model.DivisionId : null;
                researcher.TypeResearch = model.TypeResearchId.GetValueOrDefault(0) != 0 ? model.TypeResearchId : 4;
                researcher.OtherInfo = model.OtherInfo ?? "";

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"[ERROR] {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERROR] {ex.Message}");
            }

            LoadDropdowns(model.WorkGroupId.GetValueOrDefault(0), model.DepartmentId.GetValueOrDefault(0));
            return View(model);
        }

    }
}