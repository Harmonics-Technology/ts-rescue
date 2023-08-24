using System;

namespace TimesheetBE.Models.ViewModels
{
    public class ProjectTaskView
    {
        public Guid Id { get; set; }
        public Guid SuperAdminId { get; set; }
        public Guid? ProjectId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TaskPriority { get; set; }
        public string Note { get; set; }
        public double? HoursSpent { get; set; }
        public int? SubTaskCount { get; set;}
        public string Status { get; set; }
    }
}
