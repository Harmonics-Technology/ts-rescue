using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class DashboardProjectManagementView
    {
        public int NoOfProject { get; set; }
        public int NoOfTask { get; set; }
        public double TotalHours { get; set; }
        public decimal TotalBudgetSpent { get; set; }
        public List<ProjectView> ProjectSummary { get; set; }
        public List<ProjectView> OverdueProjects { get; set; }
        public List<OprationTasksVsProjectTask> OprationalAndProjectTasksStats { get; set; }
        public List<BudgetBurnOutRate> BudgetBurnOutRates { get; set; }

    }

    public class OprationTasksVsProjectTask
    {
        public string Month { get; set; }
        public int OperationalTask { get; set; }
        public int ProjectTask { get; set; }
    }

    public class BudgetBurnOutRate
    {
        public string Month { get; set; }
        public decimal Rate { get; set; }
    }
}
