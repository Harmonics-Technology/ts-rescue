﻿using System;
using TimesheetBE.Models.ViewModels;

namespace TimesheetBE.Models.AppModels
{
    public class ProjectTimesheet : BaseModel
    {
        public Guid ProjectTaskAsigneeId { get; set; }
        public ProjectTaskAsignee ProjectTaskAsignee { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? ProjectTaskId { get; set; }
        public ProjectTask ProjectTask { get; set; }
        public Guid? ProjectSubTaskId { get; set; }
        public ProjectSubTask ProjectSubTask { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double PercentageOfCompletion { get; set; }
        public bool Billable { get; set; }
        public double TotalHours { get; set; }
    }
}
