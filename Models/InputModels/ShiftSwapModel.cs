using System;

namespace TimesheetBE.Models.InputModels
{
    public class ShiftSwapModel
    {
        public Guid ShiftId { get; set; }
        public Guid ShiftToSwapId { get; set; }
        public Guid SuperAdminId { get; set; }
    }
}
