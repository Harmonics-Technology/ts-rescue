using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class ProjectListView
    {
        public int NotStarted { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public List<ProjectView> projects { get; set; }
    }
}
