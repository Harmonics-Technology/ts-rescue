using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class ProjectListView
    {
        public int NotStarted { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public ProjectView ProjectView { get; set; }
        public double? Progress { get; set; }
        //public List<ProjectProgressView> projects { get; set; }
    }
}
