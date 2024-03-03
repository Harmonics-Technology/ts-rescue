using System;

namespace TimesheetBE.Models.InputModels
{
    public class ControlSettingModel
    {
        public Guid SuperAdminId { get; set; }
        public bool? TwoFactorEnabled { get; set; }
        public bool? AdminOBoarding { get; set; }
        public bool? AdminContractManagement { get; set; }
        public bool? AdminLeaveManagement { get; set; }
        public bool? AdminShiftManagement { get; set; }
        public bool? AdminReport { get; set; }
        public bool? AdminExpenseTypeAndHST { get; set; }
        public bool? AllowShiftSwapRequest { get; set; }
        public bool? AllowShiftSwapApproval { get; set; }
        public bool? AllowIneligibleLeaveCode { get; set; }
        public DateTime? WeeklyBeginingPeriodDate { get; set; }
        public int? WeeklyPaymentPeriod { get; set; }
        public DateTime? BiWeeklyBeginingPeriodDate { get; set; }
        public int? BiWeeklyPaymentPeriod { get; set; }
        public bool IsMonthlyPayScheduleFullMonth { get; set; }
        public DateTime? MontlyBeginingPeriodDate { get; set; }
        public int? MonthlyPaymentPeriod { get; set; }
        public int? TimesheetFillingReminderDay { get; set; }
        public int? TimesheetOverdueReminderDay { get; set; }
        public bool? AllowUsersTofillFutureTimesheet { get; set; }
        public bool? AdminCanApproveExpense { get; set; }
        public bool? AdminCanApproveTimesheet { get; set; }
        public bool? AdminCanApprovePayrolls { get; set; }
        public bool? AdminCanViewPayrolls { get; set; }
        public bool? AdminCanViewTeamMemberInvoice { get; set; }
        public bool? AdminCanViewPaymentPartnerInvoice { get; set; }
        public bool? AdminCanViewClientInvoice { get; set; }
        public string? OrganizationDefaultCurrency { get; set; }
    }
}
