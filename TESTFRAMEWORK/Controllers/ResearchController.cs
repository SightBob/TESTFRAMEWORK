using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TESTFRAMEWORK.Models;

namespace TESTFRAMEWORK.Controllers
{
    public class ResearchController : Controller
    {
        private Research_DBEntities db = new Research_DBEntities();

        // GET: Research/Index
        public ActionResult Index()
        {
     

            var projects = db.ResearchProject_tbl
                             .Include(p => p.TypeEC_tbl)
                             .Include(p => p.StatusProject_tbl)
                             .Where(p => p.StatusProject_tbl.StatusProjectID == 1)
                             .OrderByDescending(p => p.ProjectID)
                             .ToList();

            return View(projects);
        }

        public ActionResult Success()
        {
            var projects = db.ResearchProject_tbl
                             .Include(p => p.TypeEC_tbl)
                             .Include(p => p.StatusProject_tbl)
                             .Where(p => p.StatusProject_tbl.StatusProjectID == 2)
                             .OrderByDescending(p => p.ProjectID)

                             .ToList();

            return View(projects);
        }

        public ActionResult Expire()
        {


            var projects = db.ResearchProject_tbl
                             .Include(p => p.TypeEC_tbl)
                             .Include(p => p.StatusProject_tbl)
                             .Where(p => p.StatusProject_tbl.StatusProjectID == 3)
                             .OrderByDescending(p => p.ProjectID)
                             .ToList();

            return View(projects);
        }

        // GET: Research/Create
        public ActionResult Create()
        {
            try
            {
                // ดึงข้อมูลรายชื่อนักวิจัยทั้งหมด (ใช้ SelectListItem ตั้งแต่ SQL Query เพื่อลด ToList() ซ้ำซ้อน)
                var researchers = db.Researcher_tbl
                                    .Select(r => new SelectListItem
                                    {
                                        Value = r.ResearcherNumber,
                                        Text = r.ResearcherNumber + " | " + r.title + " " + r.Name
                                    })
                                    .ToList();

                // สร้าง ViewModel เพื่อส่งไปยัง View
                var viewModel = new ResearchProjectViewModel
                {
                    ResearchProject = new ResearchProject_tbl(),
                    ResearchAssistants = new List<ResearchAssistantViewModel>(),
                    HeadResearcherList = researchers
                };

                // เตรียม SelectList สำหรับ DropDown
                ViewBag.TypeECList = new SelectList(db.TypeEC_tbl, "TypeECID", "ECTypeName");
                ViewBag.StatusProjectList = new SelectList(db.StatusProject_tbl, "StatusProjectID", "TypeStatus");
                ViewBag.HeadResearcherList = new SelectList(researchers, "Value", "Text");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "เกิดข้อผิดพลาดในการโหลดหน้า: " + ex.Message);
                return View("Error");
            }
        }

        // POST: Research/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ResearchProjectViewModel model, IEnumerable<HttpPostedFileBase> files)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns(model);
                return View(model);
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var researchProject = model.ResearchProject ?? new ResearchProject_tbl();

                    // ✅ ตรวจสอบและแปลงปี พ.ศ. เป็น ค.ศ.
                    var fiscalYear = researchProject.FiscalYear;
                    //if (fiscalYear > 2500) fiscalYear -= 543;

                    var project = new ResearchProject_tbl
                    {
                        FiscalYear = fiscalYear,
                        ProjectCode = researchProject.ProjectCode ?? "DefaultCode",
                        ProjectName = researchProject.ProjectName ?? "Untitled",
                        TypeECID = researchProject.TypeECID,
                        HeadResearcherId = researchProject.HeadResearcherId,
                        ECApprovalCode = researchProject.ECApprovalCode ?? "",
                        ECApprovalDate = researchProject.ECApprovalDate?.Year > 2500 ?
                                         researchProject.ECApprovalDate.Value.AddYears(-543) :
                                         researchProject.ECApprovalDate,
                        ECExpirationDate = researchProject.ECExpirationDate?.Year > 2500 ?
                                           researchProject.ECExpirationDate.Value.AddYears(-543) :
                                           researchProject.ECExpirationDate,
                        ResearchApprovalDate = researchProject.ResearchApprovalDate?.Year > 2500 ?
                                               researchProject.ResearchApprovalDate.Value.AddYears(-543) :
                                               researchProject.ResearchApprovalDate,
                        ResearchExpirationDate = researchProject.ResearchExpirationDate?.Year > 2500 ?
                                                 researchProject.ResearchExpirationDate.Value.AddYears(-543) :
                                                 researchProject.ResearchExpirationDate,
                        Note = researchProject.Note ?? "",
                        StatusProjectID = 1
                    };

                    db.ResearchProject_tbl.Add(project);
                    db.SaveChanges();

                    // ✅ บันทึกข้อมูลผู้ช่วยวิจัย
                    if (model.ResearchAssistants?.Any() == true)
                    {
                        var assistants = model.ResearchAssistants.Select(a => new ResearchAssistant_tbl
                        {
                            ProjectID = project.ProjectID,
                            ResearcherNumber = a.ResearcherNumber
                        }).ToList();

                        db.ResearchAssistant_tbl.AddRange(assistants);
                        db.SaveChanges();
                    }

                    // ✅ บันทึกไฟล์แนบ
                    var uploadedFiles = new List<ResearchFile_tbl>();

                    foreach (var file in files.Where(f => f != null && f.ContentLength > 0))
                    {
                        using (var binaryReader = new BinaryReader(file.InputStream))
                        {
                            uploadedFiles.Add(new ResearchFile_tbl
                            {
                                ProjectID = project.ProjectID,
                                FileName = file.FileName,
                                FileType = file.ContentType,
                                FileData = binaryReader.ReadBytes(file.ContentLength),
                                UploadDate = DateTime.Now
                            });
                        }
                    }

                    if (uploadedFiles.Any())
                    {
                        db.ResearchFile_tbl.AddRange(uploadedFiles);
                        db.SaveChanges();
                    }

                    transaction.Commit();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    // ดึงข้อผิดพลาดที่ลึกที่สุด
                    Exception deepestException = ex;
                    string allExceptionMessages = ex.Message;

                    while (deepestException.InnerException != null)
                    {
                        deepestException = deepestException.InnerException;
                        allExceptionMessages += " -> " + deepestException.Message;
                    }

                    ModelState.AddModelError("", "เกิดข้อผิดพลาดในการบันทึกข้อมูล: " + allExceptionMessages);
                    LoadDropdowns(model);
                    return View(model);
                }
            }
        }

        // ✅ ฟังก์ชันช่วยโหลด Dropdown Lists
        private void LoadDropdowns(ResearchProjectViewModel model)
        {
            var researchers = db.Researcher_tbl
                                .Select(r => new SelectListItem
                                {
                                    Value = r.ResearcherNumber,
                                    Text = r.ResearcherNumber + " | " + r.Name
                                })
                                .ToList();

            ViewBag.TypeECList = new SelectList(db.TypeEC_tbl, "TypeECID", "ECTypeName");
            ViewBag.StatusProjectList = new SelectList(db.StatusProject_tbl, "StatusProjectID", "TypeStatus");
            ViewBag.HeadResearcherList = new SelectList(researchers, "Value", "Text");

            model.HeadResearcherList = researchers;
        }

        // GET: ดึงข้อมูลนักวิจัยจาก ResearcherNumber
        public JsonResult GetResearcherDetails(string researcherNumber)
        {
            try
            {
                var researcher = (from r in db.Researcher_tbl
                                  where r.ResearcherNumber == researcherNumber
                                  join d in db.departments on r.department_id equals d.id into dept
                                  from d in dept.DefaultIfEmpty()
                                  join div in db.divisions on r.division_id equals div.id into divs
                                  from div in divs.DefaultIfEmpty()
                                  join wg in db.work_groups on r.work_group_id equals wg.id into wgs
                                  from wg in wgs.DefaultIfEmpty()
                                  join tr in db.TypeResearches on r.TypeResearch equals tr.id into trs
                                  from tr in trs.DefaultIfEmpty()
                                  select new
                                  {
                                      Faculty = d != null ? d.name : "-",
                                      Divisions = div != null ? div.name : "-",
                                      Work_groups = wg != null ? wg.name : "-",
                                      TypeResearch = tr != null ? tr.type_name : "-",
                                      OtherInfo = !string.IsNullOrEmpty(r.OtherInfo) ? r.OtherInfo : "-"
                                  }).FirstOrDefault();

                if (researcher == null)
                {
                    return Json(new { error = "ไม่พบข้อมูลนักวิจัย" }, JsonRequestBehavior.AllowGet);
                }

                return Json(researcher, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = "เกิดข้อผิดพลาด: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Research/Edit/5
        public ActionResult Edit(int id)
        {
            // ดึงข้อมูลโครงการวิจัยตาม ID
            var project = db.ResearchProject_tbl
                            .Include(p => p.TypeEC_tbl)
                            .Include(p => p.StatusProject_tbl)
                            .FirstOrDefault(p => p.ProjectID == id);

            if (project == null)
            {
                return HttpNotFound();
            }

            // ดึงข้อมูลรายชื่อนักวิจัยทั้งหมด มาใช้ทำ DropDownList
            var researchers = db.Researcher_tbl
                                .Select(r => new
                                {
                                    ResNo = r.ResearcherNumber,
                                    ResName = r.Name
                                })
                                .ToList()
                                .Select(r => new SelectListItem
                                {
                                    Value = r.ResNo,
                                    Text = r.ResNo + " | " + r.ResName
                                })
                                .ToList();

            // ดึงข้อมูลผู้วิจัยร่วมของโครงการนี้
            var assistants = db.ResearchAssistant_tbl
                               .Where(a => a.ProjectID == id)
                               .Select(a => new ResearchAssistantViewModel
                               {
                                   ResearcherNumber = a.ResearcherNumber
                               })
                               .ToList();

            // ดึงข้อมูลไฟล์แนบของโครงการนี้
            var files = db.ResearchFile_tbl
                          .Where(f => f.ProjectID == id)
                          .ToList();

            // สร้าง ViewModel
            var viewModel = new ResearchProjectViewModel
            {
                ResearchProject = project,
                ResearchAssistants = assistants,
                HeadResearcherList = researchers,
                AttachedFiles = files
            };

            // เตรียม SelectList สำหรับ DropDown
            ViewBag.TypeECList = new SelectList(db.TypeEC_tbl, "TypeECID", "ECTypeName", project.TypeECID);
            ViewBag.StatusProjectList = new SelectList(db.StatusProject_tbl, "StatusProjectID", "TypeStatus", project.StatusProjectID);
            ViewBag.HeadResearcherList = new SelectList(researchers, "Value", "Text", project.HeadResearcherId);

            return View(viewModel);
        }

        // POST: Research/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ResearchProjectViewModel model, IEnumerable<HttpPostedFileBase> files)
        {
            if (ModelState.IsValid)
            {
                // 1) อัพเดทข้อมูลโครงการวิจัย
                var project = db.ResearchProject_tbl.Find(model.ResearchProject.ProjectID);
                if (project == null)
                {
                    return HttpNotFound();
                }

                // อัพเดทข้อมูลทุกฟิลด์
                project.FiscalYear = model.ResearchProject.FiscalYear;
                project.ProjectCode = model.ResearchProject.ProjectCode ?? "DefaultCode";
                project.ProjectName = model.ResearchProject.ProjectName ?? "Untitled";
                project.TypeECID = model.ResearchProject.TypeECID;
                project.HeadResearcherId = model.ResearchProject.HeadResearcherId;
                project.ECApprovalCode = model.ResearchProject.ECApprovalCode ?? "";
                project.ECApprovalDate = model.ResearchProject.ECApprovalDate ?? DateTime.Now;
                project.ECExpirationDate = model.ResearchProject.ECExpirationDate ?? DateTime.Now.AddYears(1);
                project.ResearchApprovalDate = model.ResearchProject.ResearchApprovalDate ?? DateTime.Now;
                project.ResearchExpirationDate = model.ResearchProject.ResearchExpirationDate ?? DateTime.Now.AddYears(1);
                project.Note = model.ResearchProject.Note ?? "";
                project.StatusProjectID = model.ResearchProject.StatusProjectID;

                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();

                // 2) อัพเดทข้อมูลผู้ช่วยวิจัย (ลบเก่าและเพิ่มใหม่)
                // 2.1) ลบข้อมูลผู้ช่วยวิจัยเดิมทั้งหมด
                var existingAssistants = db.ResearchAssistant_tbl.Where(a => a.ProjectID == project.ProjectID);
                db.ResearchAssistant_tbl.RemoveRange(existingAssistants);
                db.SaveChanges();

                // 2.2) เพิ่มข้อมูลผู้ช่วยวิจัยใหม่
                if (model.ResearchAssistants != null)
                {
                    foreach (var assistant in model.ResearchAssistants)
                    {
                        if (!string.IsNullOrEmpty(assistant.ResearcherNumber))
                        {
                            var researchAssistant = new ResearchAssistant_tbl
                            {
                                ProjectID = project.ProjectID,
                                ResearcherNumber = assistant.ResearcherNumber
                            };
                            db.ResearchAssistant_tbl.Add(researchAssistant);
                        }
                    }
                    db.SaveChanges();
                }

                // กำหนดขนาดสูงสุด (ตัวอย่าง: 5MB)
                int maxFileSize = 5 * 1024 * 1024; // 5MB

                // 3) บันทึกไฟล์แนบเพิ่มเติม (ถ้ามี)
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            // ตรวจสอบชนิดของไฟล์
                            var validFileTypes = new[] { ".pdf" };
                            var fileExtension = Path.GetExtension(file.FileName).ToLower();

                            if (!validFileTypes.Contains(fileExtension))
                            {
                                ModelState.AddModelError("", "ไฟล์ที่อัปโหลดต้องเป็นไฟล์ PDF หรือ Word (.docx) เท่านั้น");
                                continue;
                            }

                            // ตรวจสอบขนาดไฟล์
                            if (file.ContentLength > maxFileSize)
                            {
                                ModelState.AddModelError("", $"ไฟล์ {file.FileName} ขนาดเกิน 5MB");
                                continue;
                            }

                            // บันทึกไฟล์
                            byte[] fileData;
                            using (var binaryReader = new BinaryReader(file.InputStream))
                            {
                                fileData = binaryReader.ReadBytes(file.ContentLength);
                            }

                            var researchFile = new ResearchFile_tbl
                            {
                                ProjectID = project.ProjectID,
                                FileName = file.FileName,
                                FileType = file.ContentType,
                                FileData = fileData,
                                UploadDate = DateTime.Now
                            };

                            db.ResearchFile_tbl.Add(researchFile);
                        }
                    }
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();
                    }
                    else
                    {
                        // ถ้ามีข้อผิดพลาด เช่นไฟล์ผิดประเภทหรือขนาดเกิน
                        return View(model); // ส่งกลับไปที่หน้าเดิมเพื่อแสดง error
                    }
                }

                return RedirectToAction("Index");
            }

            // โหลด DropDownList ใหม่หาก ModelState ไม่ผ่าน
            var researchers = db.Researcher_tbl
                                .Select(r => new SelectListItem
                                {
                                    Value = r.ResearcherNumber,
                                    Text = r.Name
                                }).ToList();

            ViewBag.TypeECList = new SelectList(db.TypeEC_tbl, "TypeECID", "ECTypeName", model.ResearchProject.TypeECID);
            ViewBag.StatusProjectList = new SelectList(db.StatusProject_tbl, "StatusProjectID", "TypeStatus", model.ResearchProject.StatusProjectID);
            ViewBag.HeadResearcherList = new SelectList(researchers, "Value", "Text", model.ResearchProject.HeadResearcherId);

            model.HeadResearcherList = researchers;

            // ดึงข้อมูลไฟล์แนบของโครงการนี้อีกครั้ง
            var attachedFiles = db.ResearchFile_tbl
                                  .Where(f => f.ProjectID == model.ResearchProject.ProjectID)
                                  .ToList();
            model.AttachedFiles = attachedFiles;

            return View(model);
        }

        [HttpPost]
        public ActionResult DeleteFile(int fileId)
        {
            var file = db.ResearchFile_tbl.Find(fileId);
            if (file != null)
            {
                db.ResearchFile_tbl.Remove(file);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "ไม่พบไฟล์ที่ต้องการลบ" });
        }

        public ActionResult DownloadFile(int fileId)
        {
            var file = db.ResearchFile_tbl.Find(fileId);
            if (file == null)
            {
                return HttpNotFound();
            }

            return File(file.FileData, file.FileType, file.FileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                // ดึงข้อมูลโครงการจากฐานข้อมูล
                var project = db.ResearchProject_tbl
                                .Include(p => p.ResearchAssistant_tbl)  // ถ้าต้องการลบผู้ช่วยวิจัยด้วย
                                .Include(p => p.ResearchFile_tbl)       // ถ้าต้องการลบไฟล์แนบด้วย
                                .FirstOrDefault(x => x.ProjectID == id);

                if (project == null)
                {
                    return Json(new { success = false, message = "ไม่พบโครงการที่ต้องการลบ" });
                }

                // ตัวอย่าง: หากต้องการลบความสัมพันธ์ (ผู้ช่วยวิจัย, ไฟล์แนบ) ก่อน
                if (project.ResearchAssistant_tbl != null && project.ResearchAssistant_tbl.Any())
                {
                    db.ResearchAssistant_tbl.RemoveRange(project.ResearchAssistant_tbl);
                }
                if (project.ResearchFile_tbl != null && project.ResearchFile_tbl.Any())
                {
                    db.ResearchFile_tbl.RemoveRange(project.ResearchFile_tbl);
                }

                // ลบโครงการ
                db.ResearchProject_tbl.Remove(project);
                db.SaveChanges();

                // ส่ง JSON กลับไปบอกว่าลบสำเร็จ
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // ส่ง JSON กลับไปบอกว่าลบไม่สำเร็จ พร้อมข้อความผิดพลาด
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}
