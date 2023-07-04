using System;

namespace TimesheetBE.Models.AppModels
{
    public class ShiftType : BaseModel
    {
        public Guid? SuperAdminId { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }
        public string Color { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
    }
}
