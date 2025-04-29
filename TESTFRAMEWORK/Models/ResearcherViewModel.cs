using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TESTFRAMEWORK.Models
{
    public class ResearcherViewModel
    {
        public string ResearcherNumber { get; set; }
        public string Title { get; set; }
        public string TitleCustom { get; set; }
        public string Name { get; set; }

        public int? WorkGroupId { get; set; }
        public string WorkGroupName { get; set; }
        public int? StatusWorkGroup { get; set; }

        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int? StatusDepartment { get; set; }

        public int? DivisionId { get; set; }
        public string DivisionName { get; set; }
        public int? StatusDivision { get; set; }

        public int? TypeResearchId { get; set; }
        public string TypeResearchName { get; set; }

        public string OtherInfo { get; set; }

        public List<DivisionViewModel> AllDivisions { get; set; }

        [Required(ErrorMessage = "กรุณาเลือกประเภทผู้ใช้")]
        public string UserType { get; set; } // "HospitalStaff" or "Student"
    }

}