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

                    worksheet.Cells["A2"].Value = "มหาวิทยาลัยเทคโนโลยีสุรนารี";
                    var rh2 = worksheet.Cells["A2:N2"];
                    rh2.Merge = true;
                    rh2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rh2.Style.Font.Size = 16;
                    rh2.Style.Font.Bold = true;

                    worksheet.Cells["A3"].Value = $"ประจำวันที่ {DateTime.Now.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("th-TH"))}";
                    var rh3 = worksheet.Cells["A3:N3"];
                    rh3.Merge = true;
                    rh3.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rh3.Style.Font.Size = 16;
                    rh3.Style.Font.Bold = true;

                    // หัวคอลัมน์
                    worksheet.Cells["A4"].Value = "ลำดับ";
                    worksheet.Cells["B4"].Value = "รหัส/เลขที่ใบอนุญาต";
                    worksheet.Cells["C4"].Value = "ชื่อโครงการวิจัย";
                    worksheet.Cells["D4"].Value = "ประเภท EC";
                    worksheet.Cells["E4"].Value = "สถานะ";
                    worksheet.Cells["F4"].Value = "หัวหน้าโครงการ/ผู้วิจัยหลัก";
                    worksheet.Cells["G4"].Value = "วันที่อนุมัติ EC";
                    worksheet.Cells["H4"].Value = "วันที่หมดอายุ EC";
                    worksheet.Cells["I4"].Value = "วันที่เริ่มต้นโครงการ/วันที่ได้รับอนุมัติ";
                    worksheet.Cells["J4"].Value = "วันที่สิ้นสุดโครงการ";
                    worksheet.Cells["K4"].Value = "ผู้ประสานงานโรงพยาบาล SUT";
                    worksheet.Cells["L4"].Value = "ผู้วิจัยร่วม";
                    worksheet.Cells["M4"].Value = "หน่วยงานที่เกี่ยวข้อง/ที่ตั้งโครงการ";
                    worksheet.Cells["N4"].Value = "หมายเหตุ EC";
                    var rh4 = worksheet.Cells["A4:N4"];
                    rh4.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rh4.Style.Font.Size = 16;
                    rh4.Style.Font.Bold = true;

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
                                .FirstOrDefault()
                        })
                        .ToList();

                    int row = 0;
                    int start_row = 5;

                    foreach (var d in data)
                    {
                        row++;
                        worksheet.Cells[$"A{start_row}"].Value = row;
                        worksheet.Cells[$"A{start_row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[$"B{start_row}"].Value = d.ProjectCode ?? "-";
                        worksheet.Cells[$"C{start_row}"].Value = d.ProjectName ?? "-";
                        worksheet.Cells[$"D{start_row}"].Value = d.TypeEC_tbl?.ECTypeName ?? "-";
                        worksheet.Cells[$"E{start_row}"].Value = d.StatusProject_tbl?.TypeStatus ?? "-";
                        worksheet.Cells[$"F{start_row}"].Value = d.Researcher_tbl != null
                            ? $"{d.Researcher_tbl.title} {d.Researcher_tbl.Name}"
                            : "-";

                        // วันที่อนุมัติ EC
                        worksheet.Cells[$"G{start_row}"].Value = d.ECApprovalDate.HasValue
                            ? d.ECApprovalDate.Value.Year > 2500
                                ? d.ECApprovalDate.Value.AddYears(-543).ToString("dd/MM/yyyy")
                                : d.ECApprovalDate.Value.ToString("dd/MM/yyyy")
                            : "-";
                        worksheet.Cells[$"G{start_row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // วันที่หมดอายุ EC
                        worksheet.Cells[$"H{start_row}"].Value = d.ECExpirationDate.HasValue
                            ? d.ECExpirationDate.Value.Year > 2500
                                ? d.ECExpirationDate.Value.AddYears(-543).ToString("dd/MM/yyyy")
                                : d.ECExpirationDate.Value.ToString("dd/MM/yyyy")
                            : "-";
                        worksheet.Cells[$"H{start_row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // วันที่เริ่มต้นโครงการ/วันที่ได้รับอนุมัติ
                        worksheet.Cells[$"I{start_row}"].Value = d.ResearchApprovalDate.HasValue
                            ? d.ResearchApprovalDate.Value.Year > 2500
                                ? d.ResearchApprovalDate.Value.AddYears(-543).ToString("dd/MM/yyyy")
                                : d.ResearchApprovalDate.Value.ToString("dd/MM/yyyy")
                            : "-";
                        worksheet.Cells[$"I{start_row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // วันที่สิ้นสุดโครงการ
                        worksheet.Cells[$"J{start_row}"].Value = d.ResearchExpirationDate.HasValue
                            ? d.ResearchExpirationDate.Value.Year > 2500
                                ? d.ResearchExpirationDate.Value.AddYears(-543).ToString("dd/MM/yyyy")
                                : d.ResearchExpirationDate.Value.ToString("dd/MM/yyyy")
                            : "-";
                        worksheet.Cells[$"J{start_row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // ผู้ประสานงานโรงพยาบาล SUT
                        worksheet.Cells[$"K{start_row}"].Value = d.sut_hospital_grant_code ?? "-";

                        // ผู้วิจัยร่วม (รวมในช่องเดียว โดยคั่นด้วย \n)
                        var assistantNumbers = d.ResearchAssistant_tbl
                            .Select(a => a.ResearcherNumber)
                            .ToList();
                        var assistants = researchers
                            .Where(r => assistantNumbers.Contains(r.ResearcherNumber))
                            .Select(r => $"{r.title} {r.Name}")
                            .ToList();
                        worksheet.Cells[$"L{start_row}"].Value = assistants.Any()
                            ? string.Join("\n", assistants) // ใช้ \n คั่นระหว่างชื่อ
                            : "-";
                        worksheet.Cells[$"L{start_row}"].Style.WrapText = true; // เปิดการตัดคำเพื่อให้แสดงหลายบรรทัดในช่องเดียว

                        // หน่วยงานที่เกี่ยวข้อง/ที่ตั้งโครงการ
                        var headResearcher = researchers
                            .FirstOrDefault(r => r.ResearcherNumber == d.HeadResearcherId);
                        worksheet.Cells[$"M{start_row}"].Value = headResearcher != null
                            ? $"{headResearcher.Department ?? "-"} / {headResearcher.Division ?? "-"}"
                            : "-";

                        // หมายเหตุ EC
                        worksheet.Cells[$"N{start_row}"].Value = d.Note ?? "-";

                        start_row++;
                    }

                    // ปรับความกว้างคอลัมน์
                    worksheet.Column(1).Width = 10;  // ลำดับ
                    worksheet.Column(2).Width = 20;  // รหัส/เลขที่ใบอนุญาต
                    worksheet.Column(3).Width = 40;  // ชื่อโครงการวิจัย
                    worksheet.Column(4).Width = 20;  // ประเภท EC
                    worksheet.Column(5).Width = 20;  // สถานะ
                    worksheet.Column(6).Width = 25;  // หัวหน้าโครงการ/ผู้วิจัยหลัก
                    worksheet.Column(7).Width = 20;  // วันที่อนุมัติ EC
                    worksheet.Column(8).Width = 20;  // วันที่หมดอายุ EC
                    worksheet.Column(9).Width = 20;  // วันที่เริ่มต้นโครงการ
                    worksheet.Column(10).Width = 20; // วันที่สิ้นสุดโครงการ
                    worksheet.Column(11).Width = 25; // ผู้ประสานงานโรงพยาบาล SUT
                    worksheet.Column(12).Width = 30; // ผู้วิจัยร่วม
                    worksheet.Column(13).Width = 30; // หน่วยงานที่เกี่ยวข้อง
                    worksheet.Column(14).Width = 30; // หมายเหตุ EC

                    // กรอบตาราง
                    var rangeData = worksheet.Cells[$"A1:N{start_row}"];
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