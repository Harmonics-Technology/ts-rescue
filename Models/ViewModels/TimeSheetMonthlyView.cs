using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class TimeSheetMonthlyView
    {
        public List<TimeSheetView> TimeSheet { get; set; }
        public double? ExpectedPay { get; set; }
        public double ExpectedWorkHours { get; set; }
        public double TotalHoursWorked { get; set; }
        public double TotalApprovedHours { get; set; }
        public string FullName { get; set; }
        public string Currency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}