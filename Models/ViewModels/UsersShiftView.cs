using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class UsersShiftView
    {
        public string FullName { get; set; }
        public int TotalHours { get; set; }
        public List<ShiftView> Shift { get; set; }
    }
}
