namespace TimesheetBE.Models.ViewModels
{
    public class BudgetSummaryReportView
    {
        public int NoOfUsers { get; set; }
        public double TotalHours { get; set; }
        public double BillableHours { get; set; }
        public double NonBillableHours { get; set; }
        public decimal Amount { get; set; }
    }
}
