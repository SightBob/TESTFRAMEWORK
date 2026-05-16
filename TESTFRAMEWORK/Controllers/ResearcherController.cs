using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using TESTFRAMEWORK.Filters;
using TESTFRAMEWORK.Models;

namespace TESTFRAMEWORK.Controllers
{
    public class ResearcherController : Controller
    {
        private Research_DBEntities db = new Research_DBEntities();

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
                    TypeResearchName = db.TypeResearch
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
                    TypeResearchName = db.TypeResearch
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
            // Filter out TypeResearch with id = 4
            var filteredTypeResearch = db.TypeResearch
                .Where(t => t.id != 4)
                .Select(t => new { t.id, t.type_name })
                .ToList();

            ViewBag.TypeResearch = new SelectList(filteredTypeResearch, "id", "type_name");

            var model = new ResearcherViewModel();
            model.AllDivisions = LoadDivisions(); // Populate all divisions and branches
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeUser]
        public ActionResult CreateInternal(ResearcherViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AllDivisions = LoadDivisions();
                ViewBag.TypeResearch = new SelectList(db.TypeResearch.Where(t => t.id != 4), "id", "type_name");
                return View(model);
            }

            try
            {
                int work_group_Id = model.WorkGroupId.GetValueOrDefault(0);
                int department_Id = model.DepartmentId.GetValueOrDefault(0);
                int divisionId = model.DivisionId.GetValueOrDefault(0);
                int? typeResearchId = model.TypeResearchId; // Directly use the nullable int

                System.Diagnostics.Debug.WriteLine($"[INFO] Creating Researcher: UserType={model.UserType}, ResearcherNumber={model.ResearcherNumber}, WorkGroupId={work_group_Id}, DepartmentId={department_Id}, DivisionId={divisionId}, TypeResearchId={typeResearchId}");

                // Validate TypeResearchId
                if (!typeResearchId.HasValue || typeResearchId == 0)
                {
                    ModelState.AddModelError("TypeResearchId", "กรุณาเลือกประเภทผู้วิจัยร่วม");
                    model.AllDivisions = LoadDivisions();
                    ViewBag.TypeResearch = new SelectList(db.TypeResearch.Where(t => t.id != 4), "id", "type_name");
                    return View(model);
                }

                var researcher = new Researcher_tbl
                {
                    ResearcherNumber = GenerateInternalResearcherNumber(),
                    title = model.Title,
                    Name = model.Name,
                    work_group_id = work_group_Id != 0 ? (int?)work_group_Id : null,
                    department_id = department_Id != 0 ? (int?)department_Id : null,
                    division_id = divisionId != 0 ? (int?)divisionId : null,
                    TypeResearch = typeResearchId, // Use the selected value directly
                    OtherInfo = model.UserType
                };

                db.Researcher_tbl.Add(researcher);
                db.SaveChanges();

                TempData["SuccessMessage"] = "เพิ่มข้อมูลนักวิจัยสำเร็จแล้ว";
                return RedirectToAction("CreateInternal");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"[ERROR] {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERROR] {ex.Message}");
                model.AllDivisions = LoadDivisions();
                ViewBag.TypeResearch = new SelectList(db.TypeResearch.Where(t => t.id != 4), "id", "type_name");
                return View(model);
            }
        }

        // ✅ GET: Researcher/Create (แสดงฟอร์มเพิ่มนักวิจัย)
        [AuthorizeUser]
        public ActionResult CreateExternalModal()
        {
            LoadDropdownsForExternal();
            return PartialView("CreateExternal", new ResearcherViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeUser]
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

            var listItems = db.TypeResearch
                .Where(tr => tr.id != 4)
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
            ViewBag.TypeResearchList = new SelectList(db.TypeResearch, "id", "type_name");
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
                System.Diagnostics.Debug.WriteLine("[ERROR] EditInternal GET: ResearcherNumber is null or empty");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "รหัสนักวิจัยไม่ถูกต้อง");
            }

            Researcher_tbl researcher_tbl = db.Researcher_tbl.Find(id);

            if (researcher_tbl == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] EditInternal GET: Researcher not found for ResearcherNumber: {id}");
                return HttpNotFound("ไม่พบข้อมูลนักวิจัย");
            }

            var researcher = new ResearcherViewModel
            {
                ResearcherNumber = researcher_tbl.ResearcherNumber,
                Title = researcher_tbl.title,
                TitleCustom = researcher_tbl.title == "อื่นๆ" ? researcher_tbl.title : null,
                Name = researcher_tbl.Name,
                WorkGroupId = researcher_tbl.work_group_id,
                DepartmentId = researcher_tbl.department_id,
                DivisionId = researcher_tbl.division_id,
                TypeResearchId = researcher_tbl.TypeResearch,
                UserType = researcher_tbl.OtherInfo ?? "HospitalStaff"
            };

            // Populate AllDivisions for dropdowns
            researcher.AllDivisions = LoadDivisions();
            System.Diagnostics.Debug.WriteLine($"[INFO] EditInternal GET: Researcher loaded - ResearcherNumber={researcher.ResearcherNumber}, DivisionId={researcher.DivisionId}, AllDivisions Count={(researcher.AllDivisions != null ? researcher.AllDivisions.Count : 0)}");

            // Populate display names based on DivisionId
            if (researcher.DivisionId.HasValue && researcher.AllDivisions != null)
            {
                var selectedDivision = researcher.AllDivisions.FirstOrDefault(d => d.Id == researcher.DivisionId);
                if (selectedDivision != null)
                {
                    researcher.WorkGroupName = selectedDivision.WorkGroupName;
                    researcher.DepartmentName = selectedDivision.DepartmentName;
                    researcher.DivisionName = selectedDivision.DivisionName;
                    System.Diagnostics.Debug.WriteLine($"[INFO] EditInternal GET: Selected Division - Id={selectedDivision.Id}, WorkGroup={selectedDivision.WorkGroupName}, Department={selectedDivision.DepartmentName}, Division={selectedDivision.DivisionName}");
                }
                else
                {
                    researcher.WorkGroupName = "";
                    researcher.DepartmentName = "";
                    researcher.DivisionName = "";
                    System.Diagnostics.Debug.WriteLine($"[WARNING] EditInternal GET: No division found for DivisionId: {researcher.DivisionId}");
                }
            }
            else
            {
                researcher.WorkGroupName = "";
                researcher.DepartmentName = "";
                researcher.DivisionName = "";
                System.Diagnostics.Debug.WriteLine("[WARNING] EditInternal GET: DivisionId is null or AllDivisions is null");
            }

            // Populate dropdowns
            var titleOptions = new[] { "น.ส.", "นาย", "นพ.", "พญ.", "อ.นพ.", "นศ.ทพ.", "ผศ.", "ผศ.พญ.", "ผศ.ดร.", "อ.ดร.", "อ.ทพญ.ดร.", "อื่นๆ" };
            ViewBag.TitleList = new SelectList(titleOptions, researcher.Title);

            // Filter out TypeResearch with id = 4
            var filteredTypeResearch = db.TypeResearch
                .Where(t => t.id != 4)
                .Select(t => new { t.id, t.type_name })
                .ToList();
            ViewBag.TypeResearch = new SelectList(filteredTypeResearch, "id", "type_name", researcher.TypeResearchId);

            LoadDropdownsForEdit(researcher.WorkGroupId, researcher.DepartmentId);

            return View(researcher);
        }

        [AuthorizeUser]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditInternal(ResearcherViewModel researcher)
        {
            try
            {
                // Handle custom title
                if (researcher.Title == "อื่นๆ")
                {
                    if (string.IsNullOrWhiteSpace(researcher.TitleCustom))
                    {
                        ModelState.AddModelError("TitleCustom", "กรุณากรอกคำนำหน้าแบบกำหนดเอง");
                    }
                    else
                    {
                        researcher.Title = researcher.TitleCustom;
                    }
                }

                if (ModelState.IsValid)
                {
                    var existingResearcher = db.Researcher_tbl
                        .FirstOrDefault(r => r.ResearcherNumber == researcher.ResearcherNumber);

                    if (existingResearcher == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ERROR] EditInternal POST: Researcher not found for ResearcherNumber: {researcher.ResearcherNumber}");
                        return HttpNotFound("ไม่พบข้อมูลนักวิจัย");
                    }

                    existingResearcher.title = researcher.Title;
                    existingResearcher.Name = researcher.Name;
                    existingResearcher.work_group_id = researcher.WorkGroupId;
                    existingResearcher.department_id = researcher.DepartmentId;
                    existingResearcher.division_id = researcher.DivisionId;
                    existingResearcher.TypeResearch = researcher.TypeResearchId;
                    existingResearcher.OtherInfo = researcher.UserType;

                    db.SaveChanges();

                    System.Diagnostics.Debug.WriteLine($"[INFO] EditInternal POST: Researcher updated successfully - Id={researcher.ResearcherNumber}, ResearcherNumber={researcher.ResearcherNumber}");
                    TempData["SuccessMessage"] = "ข้อมูลนักวิจัยถูกอัปเดตสำเร็จ!";
                    return RedirectToAction("Index");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[ERROR] EditInternal POST: ModelState is invalid: " + string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    researcher.AllDivisions = LoadDivisions();
                    // Repopulate display names
                    if (researcher.DivisionId.HasValue && researcher.AllDivisions != null)
                    {
                        var selectedDivision = researcher.AllDivisions.FirstOrDefault(d => d.Id == researcher.DivisionId);
                        researcher.WorkGroupName = selectedDivision?.WorkGroupName ?? "";
                        researcher.DepartmentName = selectedDivision?.DepartmentName ?? "";
                        researcher.DivisionName = selectedDivision?.DivisionName ?? "";
                    }
                    else
                    {
                        researcher.WorkGroupName = "";
                        researcher.DepartmentName = "";
                        researcher.DivisionName = "";
                    }

                    var titleOptions = new[] { "น.ส.", "นาย", "นพ.", "พญ.", "อ.นพ.", "นศ.ทพ.", "ผศ.", "ผศ.พญ.", "ผศ.ดร.", "อ.ดร.", "อ.ทพญ.ดร.", "อื่นๆ" };
                    ViewBag.TitleList = new SelectList(titleOptions, researcher.Title);
                    ViewBag.TypeResearch = new SelectList(db.TypeResearch, "id", "type_name", researcher.TypeResearchId);
                    LoadDropdownsForEdit(researcher.WorkGroupId, researcher.DepartmentId);
                    return View(researcher);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] EditInternal POST: {ex.Message}, StackTrace: {ex.StackTrace}, InnerException: {ex.InnerException?.Message}");
                researcher.AllDivisions = LoadDivisions();
                if (researcher.DivisionId.HasValue && researcher.AllDivisions != null)
                {
                    var selectedDivision = researcher.AllDivisions.FirstOrDefault(d => d.Id == researcher.DivisionId);
                    researcher.WorkGroupName = selectedDivision?.WorkGroupName ?? "";
                    researcher.DepartmentName = selectedDivision?.DepartmentName ?? "";
                    researcher.DivisionName = selectedDivision?.DivisionName ?? "";
                }
                else
                {
                    researcher.WorkGroupName = "";
                    researcher.DepartmentName = "";
                    researcher.DivisionName = "";
                }

                var titleOptions = new[] { "น.ส.", "นาย", "นพ.", "พญ.", "อ.นพ.", "นศ.ทพ.", "ผศ.", "ผศ.พญ.", "ผศ.ดร.", "อ.ดร.", "อ.ทพญ.ดร.", "อื่นๆ" };
                ViewBag.TitleList = new SelectList(titleOptions, researcher.Title);
                ViewBag.TypeResearch = new SelectList(db.TypeResearch, "id", "type_name", researcher.TypeResearchId);
                LoadDropdownsForEdit(researcher.WorkGroupId, researcher.DepartmentId);
                return View(researcher);
            }
        }

        [AuthorizeUser]
        public ActionResult EditExternalModal(string id)
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
                    db.TypeResearch,
                    "id",
                    "type_name",
                    researcher.TypeResearchId
                );

                return PartialView("EditExternal", researcher); // ใช้ PartialView
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "Researchers", "EditExternal"));
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeUser]
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

        // GET: Researcher/DownloadTemplate
        [AuthorizeUser]
        public ActionResult DownloadTemplate()
        {
            try
            {
                using (var package = new ExcelPackage())
                {
                    // ===== Sheet 2: Lookup Tables =====
                    var lookupSheet = package.Workbook.Worksheets.Add("Lookup");

                    // Header style helper
                    Action<int, int, string, System.Drawing.Color> setHeader = (row, col, text, color) =>
                    {
                        lookupSheet.Cells[row, col].Value = text;
                        lookupSheet.Cells[row, col].Style.Font.Bold = true;
                        lookupSheet.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        lookupSheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(color);
                    };

                    // กลุ่มงาน
                    setHeader(1, 1, "กลุ่มงาน", System.Drawing.Color.FromArgb(70, 130, 180));
                    var workGroups = db.work_groups.OrderBy(w => w.name).ToList();
                    for (int i = 0; i < workGroups.Count; i++)
                    {
                        lookupSheet.Cells[i + 2, 1].Value = workGroups[i].name;
                    }

                    // ฝ่ายงาน
                    setHeader(1, 2, "ฝ่ายงาน", System.Drawing.Color.FromArgb(70, 130, 180));
                    var departments = db.departments.OrderBy(d => d.name).ToList();
                    for (int i = 0; i < departments.Count; i++)
                    {
                        lookupSheet.Cells[i + 2, 2].Value = departments[i].name;
                    }

                    // แผนก
                    setHeader(1, 3, "แผนก", System.Drawing.Color.FromArgb(70, 130, 180));
                    var divisions = db.divisions.OrderBy(d => d.name).ToList();
                    for (int i = 0; i < divisions.Count; i++)
                    {
                        lookupSheet.Cells[i + 2, 3].Value = divisions[i].name;
                    }

                    // ประเภทนักวิจัย
                    setHeader(1, 4, "ประเภทนักวิจัย", System.Drawing.Color.FromArgb(70, 130, 180));
                    var typeResearches = db.TypeResearch.OrderBy(t => t.type_name).ToList();
                    for (int i = 0; i < typeResearches.Count; i++)
                    {
                        lookupSheet.Cells[i + 2, 4].Value = typeResearches[i].type_name;
                    }

                    // คำนำหน้า
                    setHeader(1, 5, "คำนำหน้า", System.Drawing.Color.FromArgb(70, 130, 180));
                    var titles = new[] { "น.ส.", "นาย", "นพ.", "พญ.", "อ.นพ.", "นศ.ทพ.", "ผศ.", "ผศ.พญ.", "ผศ.ดร.", "อ.ดร.", "อ.ทพญ.ดร.", "อื่นๆ" };
                    for (int i = 0; i < titles.Length; i++)
                    {
                        lookupSheet.Cells[i + 2, 5].Value = titles[i];
                    }

                    lookupSheet.Cells.AutoFitColumns(0);

                    // Named ranges for Data Validation
                    string wgRange = $"Lookup!$A$2:$A${workGroups.Count + 1}";
                    string deptRange = $"Lookup!$B$2:$B${departments.Count + 1}";
                    string divRange = $"Lookup!$C$2:$C${divisions.Count + 1}";
                    string trRange = $"Lookup!$D$2:$D${typeResearches.Count + 1}";
                    string titleRange = $"Lookup!$E$2:$E${titles.Length + 1}";

                    // ===== Sheet 1: นักวิจัย (form) =====
                    var ws = package.Workbook.Worksheets.Add("นักวิจัย");

                    // Headers
                    var headers = new[] { "คำนำหน้า", "ชื่อ", "กลุ่มงาน", "ฝ่ายงาน", "แผนก", "ประเภทนักวิจัย", "ข้อมูลเพิ่มเติม" };
                    var headerColors = new[] {
                        System.Drawing.Color.FromArgb(255, 199, 206), // pink
                        System.Drawing.Color.FromArgb(255, 199, 206),
                        System.Drawing.Color.FromArgb(198, 239, 206), // green
                        System.Drawing.Color.FromArgb(198, 239, 206),
                        System.Drawing.Color.FromArgb(198, 239, 206),
                        System.Drawing.Color.FromArgb(179, 229, 252), // blue
                        System.Drawing.Color.FromArgb(255, 235, 156)  // yellow
                    };
                    for (int col = 0; col < headers.Length; col++)
                    {
                        ws.Cells[1, col + 1].Value = headers[col];
                        ws.Cells[1, col + 1].Style.Font.Bold = true;
                        ws.Cells[1, col + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells[1, col + 1].Style.Fill.BackgroundColor.SetColor(headerColors[col]);
                    }

                    // Data Validation (dropdown) for rows 2-200
                    var wgValidation = ws.DataValidations.AddListValidation("C2:C200");
                    wgValidation.Formula.ExcelFormula = "Lookup!$A$2:$A$" + (workGroups.Count + 1);

                    var deptValidation = ws.DataValidations.AddListValidation("D2:D200");
                    deptValidation.Formula.ExcelFormula = "Lookup!$B$2:$B$" + (departments.Count + 1);

                    var divValidation = ws.DataValidations.AddListValidation("E2:E200");
                    divValidation.Formula.ExcelFormula = "Lookup!$C$2:$C$" + (divisions.Count + 1);

                    var trValidation = ws.DataValidations.AddListValidation("F2:F200");
                    trValidation.Formula.ExcelFormula = "Lookup!$D$2:$D$" + (typeResearches.Count + 1);

                    var titleValidation = ws.DataValidations.AddListValidation("A2:A200");
                    titleValidation.Formula.ExcelFormula = "Lookup!$E$2:$E$" + (titles.Length + 1);

                    // Sample row
                    ws.Cells[2, 1].Value = "นพ.";
                    ws.Cells[2, 2].Value = "สมชาย ใจดี";
                    ws.Cells[2, 7].Value = "หมายเหตุ";

                    // Column widths
                    ws.Column(1).Width = 15;
                    ws.Column(2).Width = 25;
                    ws.Column(3).Width = 25;
                    ws.Column(4).Width = 25;
                    ws.Column(5).Width = 25;
                    ws.Column(6).Width = 20;
                    ws.Column(7).Width = 30;

                    // Set sheet 1 as active
                    package.Workbook.Worksheets.MoveToStart("นักวิจัย");

                    var fileBytes = package.GetAsByteArray();
                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ResearcherTemplate.xlsx");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการสร้างไฟล์ Template: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Researcher/ImportFromExcel
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeUser]
        public ActionResult ImportFromExcel(HttpPostedFileBase excelFile)
        {
            if (excelFile == null || excelFile.ContentLength == 0)
            {
                return Json(new { success = false, message = "กรุณาเลือกไฟล์ Excel" });
            }

            var extension = Path.GetExtension(excelFile.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".xls")
            {
                return Json(new { success = false, message = "รองรับเฉพาะไฟล์ .xlsx หรือ .xls" });
            }

            try
            {
                int imported = 0;
                var errors = new List<string>();

                using (var stream = excelFile.InputStream)
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        return Json(new { success = false, message = "ไฟล์ Excel ไม่มีข้อมูล" });
                    }

                    int rowCount = worksheet.Dimension.End.Row;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            var title = worksheet.Cells[row, 1].Text?.Trim();
                            var name = worksheet.Cells[row, 2].Text?.Trim();
                            var workGroupName = worksheet.Cells[row, 3].Text?.Trim();
                            var departmentName = worksheet.Cells[row, 4].Text?.Trim();
                            var divisionName = worksheet.Cells[row, 5].Text?.Trim();
                            var typeResearchName = worksheet.Cells[row, 6].Text?.Trim();
                            var otherInfo = worksheet.Cells[row, 7].Text?.Trim();

                            // Skip empty rows
                            if (string.IsNullOrEmpty(name))
                            {
                                continue;
                            }

                            // Skip sample row
                            if (name.StartsWith("(ชื่อ") || name == "หมายเหตุ")
                            {
                                continue;
                            }

                            // Check duplicate name
                            if (db.Researcher_tbl.Any(r => r.Name == name && r.title == title))
                            {
                                errors.Add($"แถว {row}: มี '{title} {name}' ในระบบแล้ว");
                                continue;
                            }

                            // Lookup FK values
                            int? workGroupId = null;
                            int? departmentId = null;
                            int? divisionId = null;
                            int? typeResearchId = null;

                            if (!string.IsNullOrEmpty(workGroupName) && !workGroupName.StartsWith("("))
                            {
                                var wg = db.work_groups.FirstOrDefault(w => w.name == workGroupName);
                                if (wg != null) workGroupId = wg.id;
                                else errors.Add($"แถว {row}: ไม่พบกลุ่มงาน '{workGroupName}'");
                            }

                            if (!string.IsNullOrEmpty(departmentName) && !departmentName.StartsWith("("))
                            {
                                var dept = db.departments.FirstOrDefault(d => d.name == departmentName);
                                if (dept != null) departmentId = dept.id;
                                else errors.Add($"แถว {row}: ไม่พบฝ่ายงาน '{departmentName}'");
                            }

                            if (!string.IsNullOrEmpty(divisionName) && !divisionName.StartsWith("("))
                            {
                                var div = db.divisions.FirstOrDefault(d => d.name == divisionName);
                                if (div != null) divisionId = div.id;
                                else errors.Add($"แถว {row}: ไม่พบแผนก '{divisionName}'");
                            }

                            if (!string.IsNullOrEmpty(typeResearchName) && !typeResearchName.StartsWith("("))
                            {
                                var tr = db.TypeResearch.FirstOrDefault(t => t.type_name == typeResearchName);
                                if (tr != null) typeResearchId = tr.id;
                                else errors.Add($"แถว {row}: ไม่พบประเภทนักวิจัย '{typeResearchName}'");
                            }

                            var researcher = new Researcher_tbl
                            {
                                ResearcherNumber = GenerateInternalResearcherNumber(),
                                title = title,
                                Name = name,
                                work_group_id = workGroupId,
                                department_id = departmentId,
                                division_id = divisionId,
                                TypeResearch = typeResearchId,
                                OtherInfo = otherInfo
                            };

                            db.Researcher_tbl.Add(researcher);
                            db.SaveChanges();
                            imported++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"แถว {row}: {ex.Message}");
                        }
                    }
                }

                return Json(new
                {
                    success = true,
                    imported = imported,
                    errors = errors,
                    message = imported > 0
                        ? $"นำเข้าสำเร็จ {imported} รายการ" + (errors.Any() ? $" (มีปัญหา {errors.Count} รายการ)" : "")
                        : "ไม่สามารถนำเข้าข้อมูลได้ กรุณาตรวจสอบไฟล์"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + ex.Message });
            }
        }
    }
}