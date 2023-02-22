using System.Collections.Generic;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Models.ViewModels
{
    public class AdminPaymentScheduleView
    {
        public string ScheduleType { get; set; }
        public List<PaymentSchedule> Schedules { get; set; }
    }
}