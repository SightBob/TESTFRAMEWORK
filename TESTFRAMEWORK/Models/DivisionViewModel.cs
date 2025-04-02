using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TESTFRAMEWORK.Models
{
 public class DivisionViewModel
{
    public int Id { get; set; }
    public string DivisionName { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; }

    public int WorkGroupId { get; set; } // ✅ ID ของกลุ่มงาน
    public string WorkGroupName { get; set; } // ✅ ชื่อกลุ่มงาน

    public int StatusId { get; set; }
    public int StatusDepartment { get; set; }

    public int DivisionId { get; set; }
   public string StatusName { get; set; }
}

}