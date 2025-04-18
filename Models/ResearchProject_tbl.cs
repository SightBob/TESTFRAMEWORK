using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class ResearchProject_tbl
    {
        [Required(ErrorMessage = "กรุณาเลือกวันที่อนุมัติ EC")]
        [Display(Name = "วันที่อนุมัติ EC")]
        [DataType(DataType.Date)]
        public DateTime? ECApprovalDate { get; set; }

        [Required(ErrorMessage = "กรุณาเลือกวันหมดอายุ EC")]
        [Display(Name = "วันหมดอายุ EC")]
        [DataType(DataType.Date)]
        public DateTime? ECExpirationDate { get; set; }
    }
} 