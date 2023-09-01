﻿using System;

namespace TimesheetBE.Models.ViewModels
{
    public class ProjectTimesheetView
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? TaskId { get; set; }
        public Guid? SubTaskId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int PercentageOfCompletion { get; set; }
        public bool Billable { get; set; }
    }
}