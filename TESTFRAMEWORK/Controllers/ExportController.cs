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

        // GET: Export
        [AuthorizeUser]
        public ActionResult ExportAssetToExcel()
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
                    var rh1 = worksheet.Cells["A1:N1"];
                    rh1.Merge = true;
                    rh1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rh1.Style.Font.Size = 16;
                    rh1.Style.Font.Bold = true;
                    rh1.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rh1.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#0000C9"));
                    rh1.Style.Font.Color.SetColor(Color.White);
                    worksheet.Row(1).Height = 30;

                    worksheet.Cells["A2"].Value = "มหาวิทยาลัยเทคโนโลยีสุรนารี";
                    var rh2 = worksheet.Cells["A2:N2"];
                    rh2.Merge = true;
                    rh2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rh2.Style.Font.Size = 16;
                    rh2.Style.Font.Bold = true;
                    rh2.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rh2.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#0000C9"));
                    rh2.Style.Font.Color.SetColor(Color.White);
                    worksheet.Row(2).Height = 30;

                    worksheet.Cells["A3"].Value = $"ประจำวันที่ {DateTime.Now.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("th-TH"))}";
                    var rh3 = worksheet.Cells["A3:N3"];
                    rh3.Merge = true;
                    rh3.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rh3.Style.Font.Size = 16;
                    rh3.Style.Font.Bold = true;
                    rh3.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rh3.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#0000C9"));
                    rh3.Style.Font.Color.SetColor(Color.White);
                    worksheet.Row(3).Height = 30;

                    // หัวคอลัมน์ตามที่ต้องการ
                    worksheet.Cells["A4"].Value = "ลำดับ";
                    worksheet.Cells["B4"].Value = "วัน/เดือน/ปี";
                    worksheet.Cells["C4"].Value = "รหัสโครงการ";
                    worksheet.Cells["D4"].Value = "ชื่อโครงการ";
                    worksheet.Cells["E4"].Value = "ประเภทการพิจารณาจาก EC";
                    worksheet.Cells["F4"].Value = "หัวหน้าโครงการ";
                    worksheet.Cells["G4"].Value = "สำนักวิชา/หน่วยงาน";
                    worksheet.Cells["H4"].Value = "ผู้วิจัยร่วม/ผู้ช่วยวิจัย/\nที่ปรึกษาโครงการวิจัย";
                    worksheet.Cells["I4"].Value = "สำนักวิชา/หน่วยงาน";
                    worksheet.Cells["J4"].Value = "รหัสผ่านการรับรอง EC SUT";
                    worksheet.Cells["K4"].Value = "วันที่รับรอง EC SUT";
                    worksheet.Cells["L4"].Value = "วันหมดอายุ EC SUT";
                    worksheet.Cells["M4"].Value = "วันที่อนุมัติ รพ.มทส";
                    worksheet.Cells["N4"].Value = "วันหมดอายุ รพ.มทส";
                    worksheet.Cells["O4"].Value = "หมายเหตุ/เอกสารแนบ";

                    var rh4 = worksheet.Cells["A4:O4"];
                    rh4.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rh4.Style.Font.Size = 16;
                    rh4.Style.Font.Bold = true;
                    rh4.Style.WrapText = true;
                    rh4.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rh4.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#0000C9"));
                    rh4.Style.Font.Color.SetColor(Color.White);
                    worksheet.Row(4).Height = 45;

                    // ดึงข้อมูลจาก ResearchProject_tbl
                    var data = db.ResearchProject_tbl
                        .Include(p => p.TypeEC_tbl)
                        .Include(p => p.StatusProject_tbl)
                        .Include(p => p.Researcher_tbl)
                        .Include(p => p.ResearchAssistant_tbl)
                        .ToList();

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
                            r.OtherInfo // เพิ่มการดึง OtherInfo
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

                        // 2. วัน/เดือน/ปี (ใช้วันที่อนุมัติ)
                        worksheet.Cells[$"B{start_row}"].Value = d.ECApprovalDate.HasValue
                            ? d.ECApprovalDate.Value.Year > 2500
                                ? d.ECApprovalDate.Value.AddYears(-543).ToString("dd/MM/yyyy")
                                : d.ECApprovalDate.Value.ToString("dd/MM/yyyy")
                            : "-";
                        worksheet.Cells[$"B{start_row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // 3. รหัสโครงการ
                        worksheet.Cells[$"C{start_row}"].Value = d.ProjectCode ?? "-";

                        // 4. ชื่อโครงการ
                        worksheet.Cells[$"D{start_row}"].Value = d.ProjectName ?? "-";

                        // 5. ประเภทการพิจารณาจาก EC
                        worksheet.Cells[$"E{start_row}"].Value = d.TypeEC_tbl?.ECTypeName ?? "-";

                        // 6. หัวหน้าโครงการ
                        worksheet.Cells[$"F{start_row}"].Value = d.Researcher_tbl != null
                            ? $"{d.Researcher_tbl.title} {d.Researcher_tbl.Name}"
                            : "-";

                        // 7. สำนักวิชา/หน่วยงาน ของหัวหน้าโครงการ
                        var headResearcher = researchers
                            .FirstOrDefault(r => r.ResearcherNumber == d.HeadResearcherId);
                        worksheet.Cells[$"G{start_row}"].Value = headResearcher != null
                            ? $"{(string.IsNullOrEmpty(headResearcher.Department) ? (headResearcher.OtherInfo ?? "-") : headResearcher.Department)} / {headResearcher.Division ?? "-"}"
                            : "-";

                        // 8. ผู้วิจัยร่วม/ผู้ช่วยวิจัย/ที่ปรึกษาโครงการวิจัย
                        var assistantNumbers = d.ResearchAssistant_tbl
                            .Select(a => a.ResearcherNumber)
                            .ToList();
                        var assistants = researchers
                            .Where(r => assistantNumbers.Contains(r.ResearcherNumber))
                            .Select(r => $"{r.title} {r.Name}")
                            .ToList();
                        worksheet.Cells[$"H{start_row}"].Value = assistants.Any()
                            ? string.Join("\n", assistants)
                            : "-";
                        worksheet.Cells[$"H{start_row}"].Style.WrapText = true;

                        // 9. สำนักวิชา/หน่วยงาน ของผู้วิจัยร่วม
                        var assistantDepartments = researchers
                            .Where(r => assistantNumbers.Contains(r.ResearcherNumber))
                            .Select(r => $"{(string.IsNullOrEmpty(r.Department) ? (r.OtherInfo ?? "-") : r.Department)} / {r.Division ?? "-"}")
                            .ToList();
                        worksheet.Cells[$"I{start_row}"].Value = assistantDepartments.Any()
                            ? string.Join("\n", assistantDepartments)
                            : "-";
                        worksheet.Cells[$"I{start_row}"].Style.WrapText = true;

                        // 10. รหัสผ่านการรับรอง EC SUT
                        worksheet.Cells[$"J{start_row}"].Value = d.ECApprovalCode ?? "-";

                        // 11. วันที่รับรอง EC SUT
                        worksheet.Cells[$"K{start_row}"].Value = d.ECApprovalDate.HasValue
                            ? d.ECApprovalDate.Value.Year > 2500
                                ? d.ECApprovalDate.Value.AddYears(-543).ToString("dd/MM/yyyy")
                                : d.ECApprovalDate.Value.ToString("dd/MM/yyyy")
                            : "-";
                        worksheet.Cells[$"K{start_row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // 12. วันหมดอายุ EC SUT
                        worksheet.Cells[$"L{start_row}"].Value = d.ECExpirationDate.HasValue
                            ? d.ECExpirationDate.Value.Year > 2500
                                ? d.ECExpirationDate.Value.AddYears(-543).ToString("dd/MM/yyyy")
                                : d.ECExpirationDate.Value.ToString("dd/MM/yyyy")
                            : "-";
                        worksheet.Cells[$"L{start_row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // 13. วันที่อนุมัติ รพ.มทส
                        worksheet.Cells[$"M{start_row}"].Value = d.ResearchApprovalDate.HasValue
                            ? d.ResearchApprovalDate.Value.Year > 2500
                                ? d.ResearchApprovalDate.Value.AddYears(-543).ToString("dd/MM/yyyy")
                                : d.ResearchApprovalDate.Value.ToString("dd/MM/yyyy")
                            : "-";
                        worksheet.Cells[$"M{start_row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // 14. วันหมดอายุ รพ.มทส
                        worksheet.Cells[$"N{start_row}"].Value = d.ResearchExpirationDate.HasValue
                            ? d.ResearchExpirationDate.Value.Year > 2500
                                ? d.ResearchExpirationDate.Value.AddYears(-543).ToString("dd/MM/yyyy")
                                : d.ResearchExpirationDate.Value.ToString("dd/MM/yyyy")
                            : "-";
                        worksheet.Cells[$"N{start_row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // 15. หมายเหตุ/เอกสารแนบ
                        worksheet.Cells[$"O{start_row}"].Value = d.Note ?? "-";
                        worksheet.Cells[$"O{start_row}"].Style.WrapText = true;

                        // ใส่สีพื้นหลังสำหรับข้อมูล
                        var dataRow = worksheet.Cells[$"A{start_row}:O{start_row}"];
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
                    worksheet.Column(2).Width = 15;  // วัน/เดือน/ปี
                    worksheet.Column(3).Width = 15;  // รหัสโครงการ
                    worksheet.Column(4).Width = 60;  // ชื่อโครงการ
                    worksheet.Column(5).Width = 20;  // ประเภทการพิจารณาจาก EC
                    worksheet.Column(6).Width = 40;  // หัวหน้าโครงการ
                    worksheet.Column(7).Width = 40;  // สำนักวิชา/หน่วยงาน
                    worksheet.Column(8).Width = 50;  // ผู้วิจัยร่วม/ผู้ช่วยวิจัย/ที่ปรึกษาโครงการวิจัย
                    worksheet.Column(9).Width = 50;  // สำนักวิชา/หน่วยงาน
                    worksheet.Column(10).Width = 25; // รหัสผ่านการรับรอง EC SUT
                    worksheet.Column(11).Width = 15; // วันที่รับรอง EC SUT
                    worksheet.Column(12).Width = 15; // วันหมดอายุ EC SUT
                    worksheet.Column(13).Width = 15; // วันที่อนุมัติ รพ.มทส
                    worksheet.Column(14).Width = 15; // วันหมดอายุ รพ.มทส
                    worksheet.Column(15).Width = 100; // หมายเหตุ/เอกสารแนบ

                    // กรอบตาราง
                    var rangeData = worksheet.Cells[$"A1:O{start_row - 1}"];
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