using System;

namespace TimesheetBE.Models.InputModels
{
    public class TimesheetHoursApprovalModel
    {
        public DateTime Date { get; set; }
        public double Hours { get; set; }
        public bool Approve { get; set; }
    }
}
