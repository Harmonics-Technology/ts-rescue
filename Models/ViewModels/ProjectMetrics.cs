namespace TimesheetBE.Models.ViewModels
{
    public class ProjectMetrics
    {
        public decimal TotalBudget { get; set; }
        public decimal TotalBudgetSpent { get; set; }
        public decimal CurrentBalance { get; set; }
        public double TotalHourSpent { get; set; }
    }
}
