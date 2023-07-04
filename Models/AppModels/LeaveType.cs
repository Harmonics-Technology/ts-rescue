using System;

namespace TimesheetBE.Models.AppModels
{
    public class LeaveType : BaseModel
    {
        public Guid? superAdminId { get; set; }
        public string Name { get; set; }
        public string? LeaveTypeIcon { get; set; }
    }
}
