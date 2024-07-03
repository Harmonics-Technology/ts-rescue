using System;

namespace TimesheetBE.Models.AppModels
{
    public class ShiftType : BaseModel
    {
        public Guid? SuperAdminId { get; set; }
        public string Name { get; set; }
        public double Duration { get; set; }
        public string Color { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
    }
}
