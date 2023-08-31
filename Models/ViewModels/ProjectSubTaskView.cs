using System;
using System.Collections.Generic;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Models.ViewModels
{
    public class ProjectSubTaskView
    {
        public Guid Id { get; set; }
        public Guid ProjectTaskId { get; set; }
        public Guid AssigneeId { get; set; }
        public UserView Assignee { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public double? HoursSpent { get; set; }
        public string Priority { get; set; }
        public string Note { get; set; }
    }
}
