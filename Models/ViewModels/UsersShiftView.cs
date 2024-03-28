using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class UsersShiftView
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public double TotalHours { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        //public List<ShiftView> Shift { get; set; }
    }
}
