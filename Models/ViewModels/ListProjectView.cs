using System;
using System.Collections.Generic;

namespace TimesheetBE;

public class ListProjectView
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Budget { get; set; }
    public string Note { get; set; }
    public string? DocumentURL { get; set; }
    public double? Progress { get; set; }
    public string Status { get; set; }
    public bool IsCompleted { get; set; }
    public decimal BudgetThreshold { get; set; }
    public Guid? ProjectManagerId { get; set; }
    public string? Currency { get; set; }
    public ICollection<StrippedProjectAssignee> Assignees { get; set; }

}

public class StrippedProjectAssignee
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public bool Disabled { get; set; }
}