using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Utilities;

namespace TimesheetBE.Services.Interfaces
{
    public interface IProjectManagementService
    {
        Task<StandardResponse<bool>> CreateProject(ProjectModel model);
        Task<StandardResponse<bool>> UpdateProject(ProjectModel model);
        Task<StandardResponse<bool>> CreateTask(ProjectTaskModel model);
        Task<StandardResponse<bool>> UpdateTask(ProjectTaskModel model);
        Task<StandardResponse<bool>> CreateSubTask(ProjectSubTaskModel model);
        Task<StandardResponse<bool>> UpdateSubTask(ProjectSubTaskModel model);
        Task<StandardResponse<bool>> FillTimesheetForProject(ProjectTimesheetModel model);
        Task<StandardResponse<bool>> UpdateFilledTimesheet(UpdateProjectTimesheet model);
        Task<StandardResponse<bool>> TreatTimesheet(ProjectTimesheetApprovalModel model);
        Task<StandardResponse<ProjectView>> GetProject(Guid projectId);
        Task<StandardResponse<ProjectTaskView>> GetTask(Guid taskId);
        Task<StandardResponse<ProjectSubTaskView>> GetSubTask(Guid subTaskId);
        Task<StandardResponse<PagedCollection<ProjectView>>> ListProject(PagingOptions pagingOptions, Guid superAdminId, ProjectStatus? status = null, Guid? userId = null, string search = null);
        Task<StandardResponse<PagedCollection<ProjectTaskView>>> ListTasks(PagingOptions pagingOptions, Guid superAdminId, Guid? projectId = null, ProjectStatus? status = null, Guid? userId = null, string search = null);
        Task<StandardResponse<PagedCollection<ProjectTaskView>>> ListOperationalTasks(PagingOptions pagingOptions, Guid superAdminId, ProjectStatus? status = null, Guid? userId = null, string search = null);
        Task<StandardResponse<PagedCollection<ProjectSubTaskView>>> ListSubTasks(PagingOptions pagingOptions, Guid? taskId = null, ProjectStatus? status = null, string search = null);
        Task<StandardResponse<PagedCollection<ProjectTaskAsigneeView>>> GetUserTasks(PagingOptions pagingOptions, Guid userId, Guid projectId);
        Task<StandardResponse<ProjectProgressCountView>> GetStatusCountForProject(Guid superAdminId, Guid? userId = null);
        Task<StandardResponse<ProjectTimesheetListView>> ListUserProjectTimesheet(Guid employeeId, DateTime startDate, DateTime endDate, Guid? projectId = null);
        Task<StandardResponse<ProjectTimesheetListView>> ListSupervisorProjectTimesheet(Guid supervisorId, DateTime startDate, DateTime endDate);
        Task<StandardResponse<PagedCollection<ProjectTaskAsigneeView>>> ListProjectAssigneeTasks(PagingOptions pagingOptions, Guid superAdminId, Guid projectId, string search = null);
        Task<StandardResponse<BudgetSummaryReportView>> GetSummaryReport(Guid superAdminId, DateFilter dateFilter);
        StandardResponse<byte[]> ExportSummaryReportRecord(BudgetRecordDownloadModel model, DateFilter dateFilter, Guid superAdminId);
        Task<StandardResponse<bool>> MarkProjectOrTaskAsCompleted(MarkAsCompletedModel model);
        double GetHoursSpentOnTask(Guid taskId);
        double? GetTaskPercentageOfCompletion(Guid taskId);
        double? GetProjectPercentageOfCompletion(Guid projectId);
        Task<StandardResponse<PagedCollection<ProjectTaskAsigneeView>>> ListProjectAssigneeDetail(PagingOptions pagingOptions, Guid projectId, string search = null);
        Task<StandardResponse<PagedCollection<ResourceCapacityView>>> GetResourcesCapacityOverview(PagingOptions pagingOptions, Guid superAdminId, DateFilter dateFilter = null);
        Task<StandardResponse<PagedCollection<ResourceCapacityDetailView>>> GetResourceDetails(PagingOptions pagingOptions, Guid userId, Guid? projectId = null, ProjectStatus? status = null, string search = null);
    }
}
