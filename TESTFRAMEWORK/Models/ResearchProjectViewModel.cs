using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace TESTFRAMEWORK.Models
{
    public class ResearchProjectViewModel
    {
        public ResearchProject_tbl ResearchProject { get; set; }

        public List<ResearchAssistantViewModel> ResearchAssistants { get; set; }
        public List<SelectListItem> HeadResearcherList { get; set; }
        public List<ResearchFile_tbl> AttachedFiles { get; set; }
    }
    public class ResearchAssistantViewModel
    {
        public int? ResearchAssistantID { get; set; }
        public int? ProjectID { get; set; }
        public string ResearcherNumber { get; set; }
        public string Name_assistant { get; set; }
        public string Faculty { get; set; }
        public string TypeResearch { get; set; }
        public string OtherInfo { get; set; }

    }
}