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

        [HttpPost("create-task", Name = nameof(CreateTask))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> CreateTask(ProjectTaskModel model)
        {
            return Result(await _projectManagementService.CreateTask(model));
        }

        [HttpPost("create-subtask", Name = nameof(CreateSubTask))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> CreateSubTask(ProjectSubTaskModel model)
        {
            return Result(await _projectManagementService.CreateSubTask(model));
        }

        [HttpPost("fill-timesheet", Name = nameof(FillTimesheetForProject))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> FillTimesheetForProject(ProjectTimesheetModel model)
        {
            return Result(await _projectManagementService.FillTimesheetForProject(model));
        }

        [HttpGet("projects", Name = nameof(ListProject))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ProjectView>>>> ListProject([FromQuery] PagingOptions options, [FromQuery] Guid superAdminId, [FromQuery] ProjectStatus? status, [FromQuery] string search = null)
        {
            options.Replace(_defaultPagingOptions);
            return Ok(await _projectManagementService.ListProject(options, superAdminId, status, search));
        }

        [HttpGet("tasks", Name = nameof(ListTasks))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ProjectTaskView>>>> ListTasks([FromQuery] PagingOptions options, [FromQuery] Guid superAdminId, [FromQuery] ProjectStatus? status, [FromQuery] string search = null)
        {
            options.Replace(_defaultPagingOptions);
            return Ok(await _projectManagementService.ListTasks(options, superAdminId, status, search));
        }

        [HttpGet("subtasks", Name = nameof(ListSubTasks))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ProjectSubTaskView>>>> ListSubTasks([FromQuery] PagingOptions options, [FromQuery] Guid taskId, [FromQuery] ProjectStatus? status, [FromQuery] string search = null)
        {
            options.Replace(_defaultPagingOptions);
            return Ok(await _projectManagementService.ListSubTasks(options, taskId, status, search));
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
        public async Task<ActionResult<StandardResponse<ProjectProgressCountView>>> GetStatusCountForProject([FromQuery] Guid superAdminId)
        {
            return Ok(await _projectManagementService.GetStatusCountForProject(superAdminId));
        }
    }
}
