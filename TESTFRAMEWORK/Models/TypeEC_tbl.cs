//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TESTFRAMEWORK.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class TypeEC_tbl
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TypeEC_tbl()
        {
            this.ResearchProject_tbl = new HashSet<ResearchProject_tbl>();
        }
    
        public int TypeECID { get; set; }
        public string ECTypeName { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ResearchProject_tbl> ResearchProject_tbl { get; set; }
    }
}
