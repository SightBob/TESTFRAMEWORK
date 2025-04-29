using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TESTFRAMEWORK.Filters;
using TESTFRAMEWORK.Models;

namespace TESTFRAMEWORK.Controllers
{
    public class ResearcherController : Controller
    {
        private Research_DBEntities1 db = new Research_DBEntities1();

        // ✅ GET: Researcher/Index (หน้าแสดงรายชื่อนักวิจัย)
        [AuthorizeUser]
        public ActionResult Index()
        {
            var internalResearchers = db.Researcher_tbl
                 .Where(r => r.TypeResearch != 4)
                .Select(r => new ResearcherViewModel
                {
                    ResearcherNumber = r.ResearcherNumber ?? "-",
                    Title = r.title ?? "",
                    Name = r.Name ?? "-",
                    WorkGroupName = db.work_groups
                        .Where(w => w.id == r.work_group_id)
                        .Select(w => w.name)
                        .FirstOrDefault() ?? "-",
                    StatusWorkGroup = db.work_groups
                        .Where(w => w.id == r.work_group_id)
                        .Select(w => (int?)w.Status)
                        .FirstOrDefault() ?? 0,
                    DepartmentName = db.departments
                        .Where(d => d.id == r.department_id)
                        .Select(d => d.name)
                        .FirstOrDefault() ?? "-",
                    StatusDepartment = db.departments
                        .Where(w => w.id == r.department_id)
                        .Select(w => (int?)w.Status)
                        .FirstOrDefault() ?? 0,
                    DivisionName = db.divisions
                        .Where(di => di.id == r.division_id)
                        .Select(di => di.name)
                        .FirstOrDefault() ?? "-",
                    StatusDivision = db.divisions
                        .Where(w => w.id == r.division_id)
                        .Select(w => (int?)w.Status)
                        .FirstOrDefault() ?? 0,
                    TypeResearchName = db.TypeResearches
                        .Where(t => t.id == r.TypeResearch)
                        .Select(t => t.type_name)
                        .FirstOrDefault() ?? "-",
                    OtherInfo = r.OtherInfo
                }).ToList();

            return View(internalResearchers);
        }

        [AuthorizeUser]
        public ActionResult ExternalResearchers()
        {
            var externalResearchers = db.Researcher_tbl
                .Where(r => r.TypeResearch == 4)
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
        [AuthorizeUser]
        public ActionResult CreateInternal()
        {
            var model = new ResearcherViewModel();
            model.AllDivisions = LoadDivisions(); // Populate all divisions and branches
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateInternal(ResearcherViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AllDivisions = LoadDivisions();
                return View(model);
            }
            try
            {
                int work_group_Id = model.WorkGroupId.GetValueOrDefault(0);
                int department_Id = model.DepartmentId.GetValueOrDefault(0);
                int divisionId = model.DivisionId.GetValueOrDefault(0);
                int typeResearchId = model.TypeResearchId.GetValueOrDefault(0);

                System.Diagnostics.Debug.WriteLine($"[INFO] Creating Researcher: UserType={model.UserType}, ResearcherNumber={model.ResearcherNumber}, WorkGroupId={work_group_Id}, DepartmentId={department_Id}, DivisionId={divisionId}, TypeResearchId={typeResearchId}");

                var researcher = new Researcher_tbl
                {
                    ResearcherNumber = GenerateInternalResearcherNumber(),
                    title = model.Title,
                    Name = model.Name,
                    work_group_id = work_group_Id != 0 ? (int?)work_group_Id : null,
                    department_id = department_Id != 0 ? (int?)department_Id : null,
                    division_id = divisionId != 0 ? (int?)divisionId : null,
                    TypeResearch = typeResearchId != 0 ? (int?)typeResearchId : 4,
                    OtherInfo = model.UserType // Store UserType in OtherInfo or add a new column
                };

                db.Researcher_tbl.Add(researcher);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"[ERROR] {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERROR] {ex.Message}");
                model.AllDivisions = LoadDivisions();
                return View(model);
            }
        }


        // ✅ GET: Researcher/Create (แสดงฟอร์มเพิ่มนักวิจัย)
        [AuthorizeUser]
        public ActionResult CreateExternal()
        {
            LoadDropdownsForExternal();
            return View(new ResearcherViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateExternal(ResearcherViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdownsForExternal();
                return View(model);
            }
            try
            {
                // ✅ ตรวจสอบค่า ถ้าเป็น null ให้เป็น 0
                int work_group_Id = model.WorkGroupId.GetValueOrDefault(0);
                int department_Id = model.DepartmentId.GetValueOrDefault(0);
                int divisionId = model.DivisionId.GetValueOrDefault(0);
                int typeResearchId = model.TypeResearchId.GetValueOrDefault(0);

                var researcher = new Researcher_tbl
                {
                    ResearcherNumber = GenerateExternalResearcherNumber(),
                    title = model.Title,
                    Name = model.Name,
                    work_group_id = work_group_Id != 0 ? (int?)work_group_Id : null,
                    department_id = department_Id != 0 ? (int?)department_Id : null,
                    division_id = divisionId != 0 ? (int?)divisionId : null,
                    TypeResearch = 4,
                    OtherInfo = model.OtherInfo ?? ""
                };

                db.Researcher_tbl.Add(researcher);
                db.SaveChanges();

                return RedirectToAction("ExternalResearchers", "Researcher");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
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

            LoadDropdownsForExternal();
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

        // ✅ Load Divisions for AllDivisions
        private List<DivisionViewModel> LoadDivisions()
        {
            var divisions = db.divisions
                .Join(db.departments, div => div.department_id, dept => dept.id,
                    (div, dept) => new { div, dept })
                .Join(db.work_groups, combined => combined.dept.work_group_id, wg => wg.id,
                    (combined, wg) => new DivisionViewModel
                    {
                        Id = combined.div.id,
                        DivisionName = combined.div.name,
                        DepartmentId = combined.dept.id,
                        DepartmentName = combined.dept.name,
                        WorkGroupId = wg.id,
                        WorkGroupName = wg.name,
                        StatusId = combined.div.Status,
                        StatusDepartment = combined.dept.Status
                    })
                .Where(result => result.DivisionName != null)
                .OrderBy(result => result.WorkGroupName)
                .ThenBy(result => result.DepartmentName)
                .ThenBy(result => result.DivisionName)
                .ToList();

            System.Diagnostics.Debug.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(divisions));
            return divisions;
        }

        // ✅ โหลด Dropdown Lists for External Researchers
        private void LoadDropdownsForExternal()
        {
            ViewBag.WorkGroupList = new SelectList(db.work_groups, "id", "name");

            var listItems = db.TypeResearches
                .Where(tr => tr.type_name != "บุคคลภายนอก")
                .Select(tr => new SelectListItem
                {
                    Value = tr.id.ToString(),
                    Text = tr.type_name
                })
                .ToList();

            ViewBag.TypeResearchList = new SelectList(listItems, "Value", "Text");
        }

        // ✅ โหลด Dropdown Lists for EditInternal
        private void LoadDropdownsForEdit(int? workGroupId, int? departmentId)
        {
            ViewBag.WorkGroupList = new SelectList(db.work_groups, "id", "name", workGroupId);
            ViewBag.DepartmentList = workGroupId.HasValue
                ? new SelectList(db.departments.Where(d => d.work_group_id == workGroupId), "id", "name", departmentId)
                : new SelectList(Enumerable.Empty<SelectListItem>());
            ViewBag.DivisionList = departmentId.HasValue
                ? new SelectList(db.divisions.Where(d => d.department_id == departmentId), "id", "name")
                : new SelectList(Enumerable.Empty<SelectListItem>());
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

        private string GenerateExternalResearcherNumber()
        {
            string prefix = "E";
            var lastExternalResearcher = db.Researcher_tbl
                .Where(r => r.ResearcherNumber.StartsWith(prefix))
                .OrderByDescending(r => r.ResearcherNumber)
                .FirstOrDefault();

            if (lastExternalResearcher != null)
            {
                int lastNumber = int.Parse(lastExternalResearcher.ResearcherNumber.Substring(1));
                return $"{prefix}{lastNumber + 1:D4}";
            }
            else
            {
                return $"{prefix}0001";
            }
        }

        private string GenerateInternalResearcherNumber()
        {
            string prefix = "I";
            var lastExternalResearcher = db.Researcher_tbl
                .Where(r => r.ResearcherNumber.StartsWith(prefix))
                .OrderByDescending(r => r.ResearcherNumber)
                .FirstOrDefault();

            if (lastExternalResearcher != null)
            {
                int lastNumber = int.Parse(lastExternalResearcher.ResearcherNumber.Substring(1));
                return $"{prefix}{lastNumber + 1:D4}";
            }
            else
            {
                return $"{prefix}0001";
            }
        }

        [AuthorizeUser]
        public ActionResult EditInternal(string id)
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

            ViewBag.TitleList = new SelectList(new[] { "น.ส.", "นาย", "นพ.", "พญ.", "อ.นพ.", "นศ.ทพ.", "ผศ.", "ผศ.พญ.", "ผศ.ดร.", "อ.ดร.", "อ.ทพญ.ดร." }, researcher.Title);
            LoadDropdownsForEdit(researcher.WorkGroupId, researcher.DepartmentId);

            return View(researcher);
        }

        [AuthorizeUser]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditInternal(ResearcherViewModel researcher)
        {
            if (researcher == null || string.IsNullOrEmpty(researcher.ResearcherNumber))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "ข้อมูลไม่ถูกต้อง");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var existingResearcher = db.Researcher_tbl
                                               .Where(r => r.ResearcherNumber == researcher.ResearcherNumber)
                                               .FirstOrDefault();

                    if (existingResearcher == null)
                    {
                        return HttpNotFound("ไม่พบข้อมูลนักวิจัย");
                    }

                    existingResearcher.title = researcher.Title;
                    existingResearcher.Name = researcher.Name;
                    existingResearcher.work_group_id = researcher.WorkGroupId;
                    existingResearcher.department_id = researcher.DepartmentId;
                    existingResearcher.division_id = researcher.DivisionId;
                    existingResearcher.TypeResearch = researcher.TypeResearchId;

                    db.SaveChanges();

                    TempData["SuccessMessage"] = "ข้อมูลนักวิจัยถูกอัปเดตสำเร็จ!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.TitleList = new SelectList(new[] { "น.ส.", "นาย", "นพ.", "พญ.", "อ.นพ.", "นศ.ทพ.", "ผศ.", "ผศ.พญ.", "ผศ.ดร.", "อ.ดร.", "อ.ทพญ.ดร." }, researcher.Title);
                    LoadDropdownsForEdit(researcher.WorkGroupId, researcher.DepartmentId);
                    return View(researcher);
                }
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "Researchers", "EditInternal"));
            }
        }

        [AuthorizeUser]
        public ActionResult EditExternal(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "รหัสนักวิจัยไม่ถูกต้อง");
            }

            try
            {
                var researcher = db.Researcher_tbl
                                   .Where(r => r.ResearcherNumber == id)
                                   .Select(r => new ResearcherViewModel
                                   {
                                       ResearcherNumber = r.ResearcherNumber,
                                       Title = r.title,
                                       Name = r.Name,
                                       OtherInfo = r.OtherInfo,
                                       TypeResearchId = r.TypeResearch
                                   })
                                   .FirstOrDefault();

                if (researcher == null)
                {
                    return HttpNotFound("ไม่พบข้อมูลนักวิจัย");
                }

                var titleOptions = new[] {
                    "น.ส.", "นาย", "นพ.", "พญ.", "อ.นพ.", "นศ.ทพ.",
                    "ผศ.", "ผศ.พญ.", "ผศ.ดร.", "อ.ดร.", "อ.ทพญ.ดร.", "อื่นๆ"
                };
                ViewBag.TitleList = new SelectList(titleOptions, researcher.Title);

                ViewBag.TypeResearchList = new SelectList(
                    db.TypeResearches,
                    "id",
                    "type_name",
                    researcher.TypeResearchId
                );

                return View(researcher);
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "Researchers", "EditExternal"));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditExternal(ResearcherViewModel model)
        {
            if (model.Title == "อื่นๆ")
            {
                if (string.IsNullOrWhiteSpace(model.TitleCustom))
                {
                    ModelState.AddModelError("TitleCustom", "กรุณากรอกคำนำหน้าแบบกำหนดเอง");
                }
                else
                {
                    model.Title = model.TitleCustom;
                }
            }

            if (!string.IsNullOrWhiteSpace(model.Name) &&
                (model.Name.Length < 2 || model.Name.Length > 100))
            {
                ModelState.AddModelError("Name", "ชื่อต้องมีความยาวระหว่าง 2-100 ตัวอักษร");
            }

            if (ModelState.IsValid)
            {
                var researcher = db.Researcher_tbl.FirstOrDefault(r => r.ResearcherNumber == model.ResearcherNumber);
                if (researcher == null)
                {
                    return HttpNotFound("ไม่พบข้อมูลนักวิจัย");
                }

                researcher.title = model.Title;
                researcher.Name = model.Name;
                researcher.OtherInfo = model.OtherInfo;

                db.SaveChanges();

                return RedirectToAction("ExternalResearchers");
            }

            var titleOptions = new[] {
                "น.ส.", "นาย", "นพ.", "พญ.", "อ.นพ.", "นศ.ทพ.",
                "ผศ.", "ผศ.พญ.", "ผศ.ดร.", "อ.ดร.", "อ.ทพญ.ดร.", "อื่นๆ"
            };
            ViewBag.TitleList = new SelectList(titleOptions, model.Title);

            return View(model);
        }
    }
}