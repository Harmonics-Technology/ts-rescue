using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.ViewModels;
using System.Collections.Generic;

namespace TimesheetBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectManagementController : StandardControllerResponse
    {
        private readonly IProjectManagementService _projectManagementService;
        private readonly PagingOptions _defaultPagingOptions;
        public ProjectManagementController(IProjectManagementService projectManagementService, IOptions<PagingOptions> defaultPagingOptions)
        {
            _projectManagementService = projectManagementService;
            _defaultPagingOptions = defaultPagingOptions.Value;
        }

        [HttpPost("create-project", Name = nameof(CreateProject))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> CreateProject(ProjectModel model)
        {
            return Result(await _projectManagementService.CreateProject(model));
        }

        [HttpPost("update-project", Name = nameof(UpdateProject))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> UpdateProject(ProjectModel model)
        {
            return Result(await _projectManagementService.UpdateProject(model));
        }

        [HttpPost("create-task", Name = nameof(CreateTask))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> CreateTask(ProjectTaskModel model)
        {
            return Result(await _projectManagementService.CreateTask(model));
        }

        [HttpPost("update-task", Name = nameof(UpdateTask))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> UpdateTask(ProjectTaskModel model)
        {
            return Result(await _projectManagementService.UpdateTask(model));
        }

        [HttpPost("create-subtask", Name = nameof(CreateSubTask))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> CreateSubTask(ProjectSubTaskModel model)
        {
            return Result(await _projectManagementService.CreateSubTask(model));
        }

        [HttpPost("update-subtask", Name = nameof(UpdateSubTask))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> UpdateSubTask(ProjectSubTaskModel model)
        {
            return Result(await _projectManagementService.UpdateSubTask(model));
        }

        [HttpPost("fill-timesheet", Name = nameof(FillTimesheetForProject))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> FillTimesheetForProject(ProjectTimesheetModel model)
        {
            return Result(await _projectManagementService.FillTimesheetForProject(model));
        }

        [HttpPost("update-timesheet", Name = nameof(UpdateFilledTimesheet))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> UpdateFilledTimesheet(UpdateProjectTimesheet model)
        {
            return Result(await _projectManagementService.UpdateFilledTimesheet(model));
        }

        [HttpPost("treat-timesheet", Name = nameof(TreatTimesheet))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> TreatTimesheet(ProjectTimesheetApprovalModel model)
        {
            return Result(await _projectManagementService.TreatTimesheet(model));
        }

        [HttpGet("projects", Name = nameof(ListProject))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ProjectView>>>> ListProject([FromQuery] PagingOptions options, [FromQuery] Guid superAdminId, [FromQuery] ProjectStatus? status = null, [FromQuery] Guid? userId = null, [FromQuery] string search = null)
        {
            options.Replace(_defaultPagingOptions);
            return Ok(await _projectManagementService.ListProject(options, superAdminId, status, userId, search));
        }

        [HttpGet("projects-assignees", Name = nameof(ListProjectAssigneeDetail))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ProjectView>>>> ListProjectAssigneeDetail([FromQuery] PagingOptions options, [FromQuery] Guid projectId)
        {
            options.Replace(_defaultPagingOptions);
            return Ok(await _projectManagementService.ListProjectAssigneeDetail(options, projectId));
        }

        [HttpGet("tasks", Name = nameof(ListTasks))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ProjectTaskView>>>> ListTasks([FromQuery] PagingOptions options, [FromQuery] Guid superAdminId, [FromQuery] Guid? projectId = null, [FromQuery] ProjectStatus? status = null, [FromQuery] Guid? userId = null, [FromQuery] string search = null)
        {
            options.Replace(_defaultPagingOptions);
            return Ok(await _projectManagementService.ListTasks(options, superAdminId, projectId, status, userId, search));
        }

        [HttpGet("operational-tasks", Name = nameof(ListOperationalTasks))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ProjectTaskView>>>> ListOperationalTasks([FromQuery] PagingOptions options, [FromQuery] Guid superAdminId, [FromQuery] ProjectStatus? status = null, [FromQuery] Guid? userId = null, [FromQuery] string search = null)
        {
            options.Replace(_defaultPagingOptions);
            return Ok(await _projectManagementService.ListOperationalTasks(options, superAdminId, status, userId, search));
        }

        [HttpGet("subtasks", Name = nameof(ListSubTasks))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ProjectSubTaskView>>>> ListSubTasks([FromQuery] PagingOptions options, [FromQuery] Guid? taskId = null, [FromQuery] ProjectStatus? status = null, [FromQuery] string search = null)
        {
            options.Replace(_defaultPagingOptions);
            return Ok(await _projectManagementService.ListSubTasks(options, taskId, status, search));
        }

        [HttpGet("user-tasks", Name = nameof(GetUserTasks))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ProjectTaskAsigneeView>>>> GetUserTasks([FromQuery] PagingOptions options, [FromQuery] Guid userId, [FromQuery] Guid projectId)
        {
            options.Replace(_defaultPagingOptions);
            return Ok(await _projectManagementService.GetUserTasks(options, userId, projectId));
        }

        [HttpGet("project", Name = nameof(GetProject))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<ProjectView>>> GetProject([FromQuery] Guid projectId)
        {
            return Ok(await _projectManagementService.GetProject(projectId));
        }

        [HttpGet("task", Name = nameof(GetTask))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<ProjectTaskView>>> GetTask([FromQuery] Guid taskId)
        {
            return Ok(await _projectManagementService.GetTask(taskId));
        }

        [HttpGet("subtask", Name = nameof(GetSubTask))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<ProjectSubTaskView>>> GetSubTask([FromQuery] Guid subtaskId)
        {
            return Ok(await _projectManagementService.GetSubTask(subtaskId));
        }

        [HttpGet("project/status-count", Name = nameof(GetStatusCountForProject))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<ProjectProgressCountView>>> GetStatusCountForProject([FromQuery] Guid superAdminId, [FromQuery] Guid? userId = null)
        {
            return Ok(await _projectManagementService.GetStatusCountForProject(superAdminId, userId));
        }

        [HttpGet("user-timesheets", Name = nameof(ListUserProjectTimesheet))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<ProjectTimesheetListView>>> ListUserProjectTimesheet([FromQuery] Guid employeeId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] Guid? projectId = null)
        {
            return Ok(await _projectManagementService.ListUserProjectTimesheet(employeeId, startDate, endDate, projectId));
        }

        [HttpGet("supervisor-timesheets", Name = nameof(ListSupervisorProjectTimesheet))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<ProjectTimesheetListView>>> ListSupervisorProjectTimesheet([FromQuery] Guid supervisorId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            return Ok(await _projectManagementService.ListSupervisorProjectTimesheet(supervisorId, startDate, endDate));
        }

        [HttpGet("project-assignee-tasks", Name = nameof(ListProjectAssigneeTasks))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ProjectTaskAsigneeView>>>> ListProjectAssigneeTasks([FromQuery] PagingOptions options, [FromQuery] Guid superAdminId, [FromQuery] Guid projectId, [FromQuery] string search = null)
        {
            options.Replace(_defaultPagingOptions);
            return Ok(await _projectManagementService.ListProjectAssigneeTasks(options, superAdminId, projectId, search));
        }

        [HttpGet("summary-report", Name = nameof(GetSummaryReport))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<BudgetSummaryReportView>>> GetSummaryReport([FromQuery] Guid superAdminId, [FromQuery] DateFilter dateFilter)
        {
            return Ok(await _projectManagementService.GetSummaryReport(superAdminId, dateFilter));
        }

        [HttpPost("completed", Name = nameof(MarkProjectOrTaskAsCompleted))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> MarkProjectOrTaskAsCompleted(MarkAsCompletedModel model)
        {
            return Result(await _projectManagementService.MarkProjectOrTaskAsCompleted(model));
        }
    }
}
