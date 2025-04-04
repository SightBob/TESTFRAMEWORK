﻿using System;
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
        public int? StatusWorkGroup { get; set; }  // เพิ่ม Status ของ WorkGroup

        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int? StatusDepartment { get; set; } // เพิ่ม Status ของ Department

        public int? DivisionId { get; set; }
        public string DivisionName { get; set; }
        public int? StatusDivision { get; set; }  // เพิ่ม Status ของ Division

        public int? TypeResearchId { get; set; }
        public string TypeResearchName { get; set; }

        public string OtherInfo { get; set; }
    }

}