using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using TESTFRAMEWORK.Filters;
using TESTFRAMEWORK.Models;
using System.Data.Entity;
using System.Collections.Generic;
using System.Drawing;

namespace TESTFRAMEWORK.Controllers
{
    public class ExportController : Controller
    {
        private Research_DBEntities1 db = new Research_DBEntities1();

        // Action เพื่อดึงรายการปีงบประมาณ (พ.ศ.)
        [AuthorizeUser]
        public JsonResult GetFiscalYears()
        {
            try
            {
                // ดึงปีงบประมาณที่ไม่ซ้ำกันจาก ResearchProject_tbl และแปลงเป็นพ.ศ.
                var fiscalYears = db.ResearchProject_tbl
                    .Where(p => p.FiscalYear.HasValue)
                    .Select(p => p.FiscalYear.Value.Year + 543) // แปลงค.ศ. เป็นพ.ศ.
                    .Distinct()
                    .OrderByDescending(year => year)
                    .ToList();

                return Json(fiscalYears, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { returnMessage = $"เกิดข้อผิดพลาด: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Export
        [AuthorizeUser]
        public ActionResult ExportAssetToExcel(string fiscalYear = null)
        {
            try
            {
                // กำหนดพาธสำหรับบันทึกไฟล์
                string exportFolder = Server.MapPath("~/Files/Export");
                string resultFileName = $"{Guid.NewGuid()}.xlsx";
                string pathExcelFile = Path.Combine(exportFolder, resultFileName);

                // ตรวจสอบและสร้างโฟลเดอร์ถ้ายังไม่มี
                if (!Directory.Exists(exportFolder))
                {
                    Directory.CreateDirectory(exportFolder);
                }

                FileInfo excelFile = new FileInfo(pathExcelFile);

                using (var package = new ExcelPackage(excelFile))
                {
                    var workbook = package.Workbook;
                    var worksheet = workbook.Worksheets.Add("ResearchProjects");
                    worksheet.Cells.Style.Font.Name = "TH SarabunPSK";
                    worksheet.Cells.Style.Font.Size = 16;
                    worksheet.View.ShowGridLines = false;

                    // หัวข้อ
                    worksheet.Cells["A1"].Value = "ข้อมูลโครงการวิจัย";
                    var rh1 = worksheet.Cells["A1:R1"]; // อัปเดตเป็น R1 เพื่อรวมคอลัมน์ FiscalYear
                    rh1.Merge = true;
                    rh1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rh1.Style.Font.Size = 16;
                    rh1.Style.Font.Bold = true;
                    rh1.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rh1.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#0000C9"));
                    rh1.Style.Font.Color.SetColor(Color.White);
                    worksheet.Row(1).Height = 30;

                    worksheet.Cells["A2"].Value = "มหาวิทยาลัยเทคโนโลยีสุรนารี";
                    var rh2 = worksheet.Cells["A2:R2"];
                    rh2.Merge = true;
                    rh2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rh2.Style.Font.Size = 16;
                    rh2.Style.Font.Bold = true;
                    rh2.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rh2.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#0000C9"));
                    rh2.Style.Font.Color.SetColor(Color.White);
                    worksheet.Row(2).Height = 30;

                    worksheet.Cells["A3"].Value = $"ประจำวันที่ {DateTime.Now.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("th-TH"))}";
                    var rh3 = worksheet.Cells["A3:R3"];
                    rh3.Merge = true;
                    rh3.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rh3.Style.Font.Size = 16;
                    rh3.Style.Font.Bold = true;
                    rh3.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rh3.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#0000C9"));
                    rh3.Style.Font.Color.SetColor(Color.White);
                    worksheet.Row(3).Height = 30;

                    // หัวคอลัมน์
                    worksheet.Cells["A4"].Value = "ลำดับ";
                    worksheet.Cells["B4"].Value = "ปีงบประมาณ"; // คอลัมน์ใหม่: ปีงบประมาณ
                    worksheet.Cells["C4"].Value = "วัน/เดือน/ปี";
                    worksheet.Cells["D4"].Value = "รหัสโครงการ";
                    worksheet.Cells["E4"].Value = "ชื่อโครงการ";
                    worksheet.Cells["F4"].Value = "ประเภทการพิจารณาจาก EC";
                    worksheet.Cells["G4"].Value = "หัวหน้าโครงการ";
                    worksheet.Cells["H4"].Value = "สำนักวิชา/หน่วยงาน";
                    worksheet.Cells["I4"].Value = "ผู้วิจัยร่วม/ผู้ช่วยวิจัย/\nที่ปรึกษาโครงการวิจัย";
                    worksheet.Cells["J4"].Value = "สำนักวิชา/หน่วยงาน";
                    worksheet.Cells["K4"].Value = "รหัสผ่านการรับรอง EC SUT";
                    worksheet.Cells["L4"].Value = "วันที่รับรอง EC SUT";
                    worksheet.Cells["M4"].Value = "วันหมดอายุ EC SUT";
                    worksheet.Cells["N4"].Value = "วันที่อนุมัติ รพ.มทส";
                    worksheet.Cells["O4"].Value = "วันหมดอายุ รพ.มทส";
                    worksheet.Cells["P4"].Value = "สถานะ";
                    worksheet.Cells["Q4"].Value = "รหัสทุน รพ.มทส";
                    worksheet.Cells["R4"].Value = "หมายเหตุ/เอกสารแนบ";

                    var rh4 = worksheet.Cells["A4:R4"];
                    rh4.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rh4.Style.Font.Size = 16;
                    rh4.Style.Font.Bold = true;
                    rh4.Style.WrapText = true;
                    rh4.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rh4.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#0000C9"));
                    rh4.Style.Font.Color.SetColor(Color.White);
                    worksheet.Row(4).Height = 45;

                    // ดึงข้อมูลจาก ResearchProject_tbl
                    var dataQuery = db.ResearchProject_tbl
                        .Include(p => p.TypeEC_tbl)
                        .Include(p => p.StatusProject_tbl)
                        .Include(p => p.Researcher_tbl)
                        .Include(p => p.ResearchAssistant_tbl);

                    // กรองตามปีงบประมาณ (พ.ศ.) ถ้ามีการเลือก
                    if (!string.IsNullOrEmpty(fiscalYear) && int.TryParse(fiscalYear, out int selectedYear))
                    {
                        int convertedYear = selectedYear > 2500 ? selectedYear - 543 : selectedYear;
                        dataQuery = dataQuery.Where(p => p.FiscalYear.HasValue && p.FiscalYear.Value.Year == convertedYear);
                    }

                    var data = dataQuery.ToList();

                    // ตรวจสอบว่ามีข้อมูลหรือไม่
                    if (!data.Any())
                    {
                        return Json(new { returnMessage = "ไม่มีข้อมูลสำหรับปีที่เลือก" }, JsonRequestBehavior.AllowGet);
                    }

                    // ดึงข้อมูลนักวิจัยทั้งหมดเพื่อใช้ในการ join และดึงข้อมูลหน่วยงาน
                    var researchers = db.Researcher_tbl
                        .Select(r => new
                        {
                            r.ResearcherNumber,
                            r.title,
                            r.Name,
                            Department = db.departments
                                .Where(d => d.id == r.department_id)
                                .Select(d => d.name)
                                .FirstOrDefault(),
                            Division = db.divisions
                                .Where(div => div.id == r.division_id)
                                .Select(div => div.name)
                                .FirstOrDefault(),
                            r.OtherInfo
                        })
                        .ToList();

                    int row = 0;
                    int start_row = 5;

                    foreach (var d in data)
                    {
                        row++;
                        // 1. ลำดับ
                        worksheet.Cells[$"A{start_row}"].Value = row;
                        worksheet.Cells[$"A{start_row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // 2. ปีงบประมาณ (แสดงเป็นพ.ศ.)
                        worksheet.Cells[$"B{start_row}"].Value = d.FiscalYear.HasValue
                            ? (d.FiscalYear.Value.Year + 543).ToString()
                            : "-";
                        worksheet.Cells[$"B{start_row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // 3. วัน/เดือน/ปี (ใช้วันที่อนุมัติ)
                        worksheet.Cells[$"C{start_row}"].Value = d.ECApprovalDate.HasValue
                            ? d.ECApprovalDate.Value.Year > 2500
                                ? d.ECApprovalDate.Value.AddYears(-543).ToString("dd/MM/yyyy")
                                : d.ECApprovalDate.Value.ToString("dd/MM/yyyy")
                            : "-";
                        worksheet.Cells[$"C{start_row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // 4. รหัสโครงการ
                        worksheet.Cells[$"D{start_row}"].Value = d.ProjectCode ?? "-";

                        // 5. ชื่อโครงการ
                        worksheet.Cells[$"E{start_row}"].Value = d.ProjectName ?? "-";

                        // 6. ประเภทการพิจารณาจาก EC
                        worksheet.Cells[$"F{start_row}"].Value = d.TypeEC_tbl?.ECTypeName ?? "-";

                        // 7. หัวหน้าโครงการ
                        worksheet.Cells[$"G{start_row}"].Value = d.Researcher_tbl != null
                            ? $"{d.Researcher_tbl.title} {d.Researcher_tbl.Name}"
                            : "-";

                        // 8. สำนักวิชา/หน่วยงาน ของหัวหน้าโครงการ
                        var headResearcher = researchers
                            .FirstOrDefault(r => r.ResearcherNumber == d.HeadResearcherId);
                        worksheet.Cells[$"H{start_row}"].Value = headResearcher != null
                            ? $"{(string.IsNullOrEmpty(headResearcher.Department) ? (headResearcher.OtherInfo ?? "-") : headResearcher.Department)} / {headResearcher.Division ?? "-"}"
                            : "-";

                        // 9. ผู้วิจัยร่วม/ผู้ช่วยวิจัย/ที่ปรึกษาโครงการวิจัย
                        var assistantNumbers = d.ResearchAssistant_tbl
                            .Select(a => a.ResearcherNumber)
                            .ToList();
                        var assistants = researchers
                            .Where(r => assistantNumbers.Contains(r.ResearcherNumber))
                            .Select(r => $"{r.title} {r.Name}")
                            .ToList();
                        worksheet.Cells[$"I{start_row}"].Value = assistants.Any()
                            ? string.Join("\n", assistants)
                            : "-";
                        worksheet.Cells[$"I{start_row}"].Style.WrapText = true;

                        //  fairness/หน่วยงาน ของผู้วิจัยร่วม
                        var assistantDepartments = researchers
                            .Where(r => assistantNumbers.Contains(r.ResearcherNumber))
                            .Select(r => $"{(string.IsNullOrEmpty(r.Department) ? (r.OtherInfo ?? "-") : r.Department)} / {r.Division ?? "-"}")
                            .ToList();
                        worksheet.Cells[$"J{start_row}"].Value = assistantDepartments.Any()
                            ? string.Join("\n", assistantDepartments)
                            : "-";
                        worksheet.Cells[$"J{start_row}"].Style.WrapText = true;

                        // 11. รหัสผ่านการรับรอง EC SUT
                        worksheet.Cells[$"K{start_row}"].Value = d.ECApprovalCode ?? "-";

                        // 12. วันที่รับรอง EC SUT
                        worksheet.Cells[$"L{start_row}"].Value = d.ECApprovalDate.HasValue
                            ? d.ECApprovalDate.Value.Year > 2500
                                ? d.ECApprovalDate.Value.AddYears(-543).ToString("dd/MM/yyyy")
                                : d.ECApprovalDate.Value.ToString("dd/MM/yyyy")
                            : "-";
                        worksheet.Cells[$"L{start_row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // 13. วันหมดอายุ EC SUT
                        worksheet.Cells[$"M{start_row}"].Value = d.ECExpirationDate.HasValue
                            ? d.ECExpirationDate.Value.Year > 2500
                                ? d.ECExpirationDate.Value.AddYears(-543).ToString("dd/MM/yyyy")
                                : d.ECExpirationDate.Value.ToString("dd/MM/yyyy")
                            : "-";
                        worksheet.Cells[$"M{start_row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // 14. วันที่อนุมัติ รพ.มทส
                        worksheet.Cells[$"N{start_row}"].Value = d.ResearchApprovalDate.HasValue
                            ? d.ResearchApprovalDate.Value.Year > 2500
                                ? d.ResearchApprovalDate.Value.AddYears(-543).ToString("dd/MM/yyyy")
                                : d.ResearchApprovalDate.Value.ToString("dd/MM/yyyy")
                            : "-";
                        worksheet.Cells[$"N{start_row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // 15. วันหมดอายุ รพ.มทส
                        worksheet.Cells[$"O{start_row}"].Value = d.ResearchExpirationDate.HasValue
                            ? d.ResearchExpirationDate.Value.Year > 2500
                                ? d.ResearchExpirationDate.Value.AddYears(-543).ToString("dd/MM/yyyy")
                                : d.ResearchExpirationDate.Value.ToString("dd/MM/yyyy")
                            : "-";
                        worksheet.Cells[$"O{start_row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // 16. สถานะ
                        worksheet.Cells[$"P{start_row}"].Value = d.StatusProject_tbl?.TypeStatus ?? "-";
                        worksheet.Cells[$"P{start_row}"].Style.WrapText = true;

                        // 17. รหัสทุน รพ.มทส
                        worksheet.Cells[$"Q{start_row}"].Value = d.sut_hospital_grant_code ?? "-";
                        worksheet.Cells[$"Q{start_row}"].Style.WrapText = true;

                        // 18. หมายเหตุ/เอกสารแนบ
                        worksheet.Cells[$"R{start_row}"].Value = d.Note ?? "-";
                        worksheet.Cells[$"R{start_row}"].Style.WrapText = true;

                        // ใส่สีพื้นหลังสำหรับข้อมูล
                        var dataRow = worksheet.Cells[$"A{start_row}:R{start_row}"];
                        dataRow.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        dataRow.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#D9EAD3"));

                        // ปรับความสูงแถวตามจำนวนผู้วิจัยร่วม (ถ้ามี)
                        if (assistants.Any())
                        {
                            int rowCount = Math.Max(assistants.Count, assistantDepartments.Count);
                            worksheet.Row(start_row).Height = 35 * rowCount;
                        }
                        else
                        {
                            worksheet.Row(start_row).Height = 20;
                        }

                        start_row++;
                    }

                    // ปรับความกว้างคอลัมน์
                    worksheet.Column(1).Width = 8;   // ลำดับ
                    worksheet.Column(2).Width = 12;  // ปีงบประมาณ
                    worksheet.Column(3).Width = 15;  // วัน/เดือน/ปี
                    worksheet.Column(4).Width = 15;  // รหัสโครงการ
                    worksheet.Column(5).Width = 60;  // ชื่อโครงการ
                    worksheet.Column(6).Width = 20;  // ประเภทการพิจารณาจาก EC
                    worksheet.Column(7).Width = 40;  // หัวหน้าโครงการ
                    worksheet.Column(8).Width = 40;  // สำนักวิชา/หน่วยงาน
                    worksheet.Column(9).Width = 50;  // ผู้วิจัยร่วม/ผู้ช่วยวิจัย/ที่ปรึกษาโครงการวิจัย
                    worksheet.Column(10).Width = 50; // สำนักวิชา/หน่วยงาน
                    worksheet.Column(11).Width = 25; // รหัสผ่านการรับรอง EC SUT
                    worksheet.Column(12).Width = 15; // วันที่รับรอง EC SUT
                    worksheet.Column(13).Width = 15; // วันหมดอายุ EC SUT
                    worksheet.Column(14).Width = 15; // วันที่อนุมัติ รพ.มทส
                    worksheet.Column(15).Width = 15; // วันหมดอายุ รพ.มทส
                    worksheet.Column(16).Width = 20; // สถานะ
                    worksheet.Column(17).Width = 25; // รหัสทุน รพ.มทส
                    worksheet.Column(18).Width = 100; // หมายเหตุ/เอกสารแนบ

                    // กรอบตาราง
                    var rangeData = worksheet.Cells[$"A1:R{start_row - 1}"];
                    rangeData.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    rangeData.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    rangeData.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    rangeData.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                    // Freeze หัวตาราง
                    worksheet.View.FreezePanes(5, 1);

                    // บันทึกไฟล์
                    package.Save();

                    // ส่งไฟล์กลับไปยังผู้ใช้
                    Response.Clear();
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", $"attachment; filename=ResearchProjects_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                    Response.BinaryWrite(package.GetAsByteArray());
                    Response.Flush();
                    Response.End();
                }

                return Json(new { returnMessage = "OK" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Exception deepestException = ex;
                string allExceptionMessages = ex.Message;
                while (deepestException.InnerException != null)
                {
                    deepestException = deepestException.InnerException;
                    allExceptionMessages += " -> " + deepestException.Message;
                }

                return Json(new { returnMessage = $"เกิดข้อผิดพลาด: {allExceptionMessages}" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}