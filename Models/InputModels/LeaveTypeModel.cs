using System;

namespace TimesheetBE.Models.InputModels
{
    public class LeaveTypeModel
    {
        public Guid? superAdminId { get; set; }
        public string Name { get; set; }
        public string? LeaveTypeIcon { get; set; }
    }
}
