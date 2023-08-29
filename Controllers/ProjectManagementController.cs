﻿using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("projects", Name = nameof(ListProject))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<PagedCollection<UserView>>>> ListProject([FromQuery] PagingOptions options, [FromQuery] Guid superAdminId, [FromQuery] ProjectStatus? status, [FromQuery] string search = null)
        {
            options.Replace(_defaultPagingOptions);
            return Ok(await _projectManagementService.ListProject(options, superAdminId, status, search));
        }
    }
}
