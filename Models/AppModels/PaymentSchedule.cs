using System;

namespace TimesheetBE.Models.AppModels
{
    public class PaymentSchedule : BaseModel
    {
        public int Cycle { get; set; }
        public DateTime WeekDate { get; set; }
        public DateTime LastWorkDayOfCycle { get; set; }
        public DateTime ApprovalDate { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? CycleType { get; set; }
    }
}