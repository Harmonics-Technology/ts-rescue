using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class ProjectTaskListView
    {
        public ProjectTaskView Tasks { get; set; }
        public List<string> Assignees { get; set; }
    }
}
