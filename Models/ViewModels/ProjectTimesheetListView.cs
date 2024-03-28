using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class ProjectTimesheetListView
    {
        public List<ProjectTimesheetView> ProjectTimesheets { get; set; }
        public double Billable { get; set; }
        public double NonBillable { get; set; }
    }
}
