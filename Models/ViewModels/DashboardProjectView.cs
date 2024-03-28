using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class DashboardProjectView
    {
        public int Resources { get; set; }
        public int TotalTasks { get; set; }
        public double TotalHours { get; set; }
        //public decimal TotalBudgetSpent { get; set; }
        public List<ProjectTaskView> UpcomingDeadlines { get; set; }
        public BudgetSpentVsBudgetRemain BudgetSpentAndRemain { get; set; }
        public ProjectTaskStatusCount ProjectTaskStatus { get; set; }
        public List<MonthlyCompletedTask> MonthlyCompletedTasks { get; set; }
    }

    public class BudgetSpentVsBudgetRemain
    {
        public decimal Budget { get; set; }
        public decimal BudgetSpent { get; set; }
        public decimal BudgetRemain { get; set; }
    }

    public class ProjectTaskStatusCount
    {
        public int NotStarted { get; set; }
        public int Completed { get; set; }
        public int Ongoing { get; set; }
    }

    public class MonthlyCompletedTask
    {
        public string Month { get; set; }
        public int TaskCompleted { get; set; }
    }
}
