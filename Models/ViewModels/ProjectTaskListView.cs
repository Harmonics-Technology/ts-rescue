using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class ProjectTaskListView
    {
        public ProjectTaskView Tasks { get; set; }
        public List<Guid> Assignees { get; set; }
        public double HoursSpent { get; set; }
        public int SubTasks { get; set; }
        public string Status { get; set; }
    }
}
