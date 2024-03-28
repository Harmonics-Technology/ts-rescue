using System;

namespace TimesheetBE.Models.InputModels
{
    public class PayScheduleGenerationModel
    {
        public Guid SuperAdminId { get; set; }
        public DateTime StartDate { get; set; }
        public int PaymentDateDays { get; set; }
    }
}
