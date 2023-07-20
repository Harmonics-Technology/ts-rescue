using System;

namespace TimesheetBE.Models.ViewModels
{
    public class ShiftUsersListView
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public int TotalHours { get; set; }
    }
}
