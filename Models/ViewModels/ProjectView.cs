﻿using System;

namespace TimesheetBE.Models.ViewModels
{
    public class ProjectView
    {
        public Guid Id { get; set; }
        public Guid SuperAdminId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public decimal Budget { get; set; }
        public string Note { get; set; }
        public string? DocumentURL { get; set; }
    }
}
