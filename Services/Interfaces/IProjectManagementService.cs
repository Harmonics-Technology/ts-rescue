﻿using System;
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
        Task<StandardResponse<bool>> CreateTask(ProjectTaskModel model);
        Task<StandardResponse<bool>> CreateSubTask(ProjectSubTaskModel model);
        Task<StandardResponse<bool>> FillTimesheetForProject(ProjectTimesheetModel model);
        Task<StandardResponse<ProjectView>> GetProject(Guid projectId);
        Task<StandardResponse<ProjectTaskView>> GetTask(Guid taskId);
        Task<StandardResponse<ProjectSubTaskView>> GetSubTask(Guid subTaskId);
        Task<StandardResponse<PagedCollection<ProjectView>>> ListProject(PagingOptions pagingOptions, Guid superAdminId, ProjectStatus? status, string search = null);
        Task<StandardResponse<PagedCollection<ProjectTaskView>>> ListTasks(PagingOptions pagingOptions, Guid superAdminId, ProjectStatus? status, string search = null);
        Task<StandardResponse<PagedCollection<ProjectSubTaskView>>> ListSubTasks(PagingOptions pagingOptions, Guid taskId, ProjectStatus? status, string search = null);
    }
}
