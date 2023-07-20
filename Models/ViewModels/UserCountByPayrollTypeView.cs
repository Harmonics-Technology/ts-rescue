namespace TimesheetBE.Models.ViewModels
{
    public class UserCountByPayrollTypeView
    {
        public string Month { get; set; }
        public int OnShore { get; set; }
        public int OffShore { get; set; }

    }

    public enum Month
    {
        January = 1,
        February = 2,
        March = 3,
        April = 4,
        May = 5,
        June = 6,
        July = 7,
        August = 8,
        September = 9,
        October = 10,
        November = 11,
        December = 12
    }
}
