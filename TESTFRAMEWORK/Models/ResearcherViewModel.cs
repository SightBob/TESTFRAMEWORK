using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TESTFRAMEWORK.Models
{
    public class ResearcherViewModel
    {
        [Required(ErrorMessage = "กรุณากรอกรหัสนักวิจัย")]
        public string ResearcherNumber { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อนำหน้า")]
        public string Title { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อนักวิจัย")]
        public string Name { get; set; }

        public int? WorkGroupId { get; set; }
        public string WorkGroupName { get; set; }

        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }

        public int? DivisionId { get; set; }
        public string DivisionName { get; set; }

        public int? TypeResearchId { get; set; }
        public string TypeResearchName { get; set; }
        public string OtherInfo { get; set; }
    }
}