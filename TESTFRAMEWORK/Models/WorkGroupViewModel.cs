using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TESTFRAMEWORK.Models
{
    public class WorkGroupViewModel
    {
        public int Id { get; set; }
        public string WorkGroupName { get; set; }

        public int StatusId { get; set; }

        public string StatusName { get; set; }
    }
}