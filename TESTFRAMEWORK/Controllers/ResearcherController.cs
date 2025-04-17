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
                        .Select(w => (int?)w.Status)  // แปลงเป็น Nullable<int>
                        .FirstOrDefault() ?? 0,  // ถ้าไม่มีค่า ให้ใส่ 0
                    DepartmentName = db.departments
                        .Where(d => d.id == r.department_id)
                        .Select(d => d.name)
                        .FirstOrDefault() ?? "-",
                    StatusDepartment = db.departments
                        .Where(w => w.id == r.department_id)
                        .Select(w => (int?)w.Status)  // แปลงเป็น Nullable<int>
                        .FirstOrDefault() ?? 0,  // ถ้าไม่มีค่า ให้ใส่ 0
                    DivisionName = db.divisions
                        .Where(di => di.id == r.division_id)
                        .Select(di => di.name)
                        .FirstOrDefault() ?? "-",
                    StatusDivision = db.divisions
                        .Where(w => w.id == r.division_id)
                        .Select(w => (int?)w.Status)  // แปลงเป็น Nullable<int>
                        .FirstOrDefault() ?? 0,  // ถ้าไม่มีค่า ให้ใส่ 0
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
                    ResearcherNumber = GenerateInternalResearcherNumber(),
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

        // ✅ GET: Researcher/Create (แสดงฟอร์มเพิ่มนักวิจัย)
        [AuthorizeUser]
        public ActionResult CreateExternal()
        {
            LoadDropdowns();
            return View(new ResearcherViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateExternal(ResearcherViewModel model)
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

            var listItems = db.TypeResearches
                .ToList()
                .Where(tr => tr.type_name != "บุคคลภายนอก")
                .Select(tr => new SelectListItem
                {
                    Value = tr.id.ToString(),
                    Text = tr.type_name
                })
                .ToList();

            ViewBag.TypeResearchList = new SelectList(listItems, "Value", "Text");
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
            LoadDropdowns();
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // ดึงข้อมูลนักวิจัยจากฐานข้อมูล
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

            // โหลด Dropdown List สำหรับ Title และตั้งค่า researcher.Title เป็นค่าที่ถูกเลือก
            ViewBag.TitleList = new SelectList(new[] { "น.ส.", "นาย", "นพ.", "พญ.", "อ.นพ.", "นศ.ทพ.", "ผศ.", "ผศ.พญ.", "ผศ.ดร.", "อ.ดร.", "อ.ทพญ.ดร." }, researcher.Title);

            // โหลด Dropdown Lists สำหรับ WorkGroup, Department, Division, TypeResearch
            ViewBag.WorkGroupList = new SelectList(db.work_groups, "id", "name", researcher.WorkGroupId);

            // ตรวจสอบว่ามี WorkGroupId ก่อนโหลด DepartmentList
            ViewBag.DepartmentList = researcher.WorkGroupId.HasValue
                ? new SelectList(db.departments.Where(d => d.work_group_id == researcher.WorkGroupId), "id", "name", researcher.DepartmentId)
                : new SelectList(Enumerable.Empty<SelectListItem>());

            // ตรวจสอบว่ามี DepartmentId ก่อนโหลด DivisionList
            ViewBag.DivisionList = researcher.DepartmentId.HasValue
                ? new SelectList(db.divisions.Where(di => di.department_id == researcher.DepartmentId), "id", "name", researcher.DivisionId)
                : new SelectList(Enumerable.Empty<SelectListItem>());

            ViewBag.TypeResearchList = new SelectList(db.TypeResearches, "id", "type_name", researcher.TypeResearchId);

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
                // ตรวจสอบว่ามีการ valid ข้อมูลหรือไม่
                if (ModelState.IsValid)
                {
                    // ค้นหาข้อมูลนักวิจัยจากฐานข้อมูล
                    var existingResearcher = db.Researcher_tbl
                                               .Where(r => r.ResearcherNumber == researcher.ResearcherNumber)
                                               .FirstOrDefault();

                    if (existingResearcher == null)
                    {
                        return HttpNotFound("ไม่พบข้อมูลนักวิจัย");
                    }

                    // อัปเดตข้อมูลนักวิจัยในฐานข้อมูล
                    existingResearcher.title = researcher.Title;
                    existingResearcher.Name = researcher.Name;
                    existingResearcher.work_group_id = researcher.WorkGroupId;
                    existingResearcher.department_id = researcher.DepartmentId;
                    existingResearcher.division_id = researcher.DivisionId;
                    existingResearcher.TypeResearch = researcher.TypeResearchId;

                    // บันทึกข้อมูลที่อัปเดต
                    db.SaveChanges();

                    // ส่งข้อความสำเร็จและกลับไปยังหน้าที่เหมาะสม
                    TempData["SuccessMessage"] = "ข้อมูลนักวิจัยถูกอัปเดตสำเร็จ!";
                    return RedirectToAction("Index"); // หรือชื่อของ action ที่คุณต้องการ redirect ไป
                }
                else
                {
                    // หากข้อมูลไม่ valid ให้โหลด dropdown lists อีกครั้งและแสดงฟอร์มใหม่

                    // โหลด Dropdown List สำหรับ Title
                    ViewBag.TitleList = new SelectList(new[] { "น.ส.", "นาย", "นพ.", "พญ.", "อ.นพ.", "นศ.ทพ.", "ผศ.", "ผศ.พญ.", "ผศ.ดร.", "อ.ดร.", "อ.ทพญ.ดร." }, researcher.Title);

                    // โหลด Dropdown Lists สำหรับ WorkGroup, Department, Division, TypeResearch
                    ViewBag.WorkGroupList = new SelectList(db.work_groups, "id", "name", researcher.WorkGroupId);

                    ViewBag.DepartmentList = researcher.WorkGroupId.HasValue
                        ? new SelectList(db.departments.Where(d => d.work_group_id == researcher.WorkGroupId), "id", "name", researcher.DepartmentId)
                        : new SelectList(Enumerable.Empty<SelectListItem>());

                    ViewBag.DivisionList = researcher.DepartmentId.HasValue
                        ? new SelectList(db.divisions.Where(di => di.department_id == researcher.DepartmentId), "id", "name", researcher.DivisionId)
                        : new SelectList(Enumerable.Empty<SelectListItem>());

                    ViewBag.TypeResearchList = new SelectList(db.TypeResearches, "id", "type_name", researcher.TypeResearchId);

                    // แสดงฟอร์มใหม่พร้อมกับค่าที่โหลดจากฐานข้อมูล
                    return View(researcher);
                }
            }
            catch (Exception ex)
            {
                // บันทึกข้อผิดพลาดและแสดงหน้าข้อผิดพลาด
                return View("Error", new HandleErrorInfo(ex, "Researchers", "EditInternal"));
            }
        }

        // GET: EditExternal
        public ActionResult EditExternal(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "รหัสนักวิจัยไม่ถูกต้อง");
            }

            try
            {
                // ดึงข้อมูลนักวิจัยและแมปไปยัง ResearcherViewModel
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

                // จัดการ dropdown list สำหรับคำนำหน้า
                var titleOptions = new[] {
            "น.ส.", "นาย", "นพ.", "พญ.", "อ.นพ.", "นศ.ทพ.",
            "ผศ.", "ผศ.พญ.", "ผศ.ดร.", "อ.ดร.", "อ.ทพญ.ดร.", "อื่นๆ"
        };
                ViewBag.TitleList = new SelectList(titleOptions, researcher.Title);

                // ดึงรายการประเภทการวิจัย
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
                // บันทึกข้อผิดพลาด (เช่น log error) และแสดงหน้าข้อผิดพลาดทั่วไป
                // Logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูลนักวิจัย");
                return View("Error", new HandleErrorInfo(ex, "Researchers", "EditExternal"));
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditExternal(ResearcherViewModel model)
        {
            // ถ้าเลือก "อื่นๆ" ให้ใช้ค่า TitleCustom
            if (model.Title == "อื่นๆ")
            {
                if (string.IsNullOrWhiteSpace(model.TitleCustom))
                {
                    ModelState.AddModelError("TitleCustom", "กรุณากรอกคำนำหน้าแบบกำหนดเอง");
                }
                else
                {
                    // ใช้ค่าของ TitleCustom แทน Title
                    model.Title = model.TitleCustom;
                }
            }

            // ตรวจสอบความยาวของชื่อ
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

            // เตรียม dropdown lists สำหรับกรณีที่ต้องส่งกลับ
            var titleOptions = new[] {
        "น.ส.", "นาย", "นพ.", "พญ.", "อ.นพ.", "นศ.ทพ.",
        "ผศ.", "ผศ.พญ.", "ผศ.ดร.", "อ.ดร.", "อ.ทพญ.ดร.", "อื่นๆ"
    };
            ViewBag.TitleList = new SelectList(titleOptions, model.Title);

            return View(model);
        }

        // ✅ โหลด Dropdown ตาม WorkGroup และ Department
        private void LoadDropdowns(int workGroupId, int departmentId)
        {
            ViewBag.WorkGroupList = new SelectList(db.work_groups, "id", "name", workGroupId);
            ViewBag.DepartmentList = new SelectList(db.departments.Where(d => d.work_group_id == workGroupId), "id", "name", departmentId);
            ViewBag.DivisionList = new SelectList(db.divisions.Where(d => d.department_id == departmentId), "id", "name");
            ViewBag.TypeResearchList = new SelectList(db.TypeResearches, "id", "type_name");
        }

    }
}