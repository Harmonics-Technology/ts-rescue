using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Services;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;

namespace TimesheetBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingController : StandardControllerResponse
    {
        private readonly ITrainingService _trainingService;
        private readonly PagingOptions _defaultPagingOptions;
        public TrainingController(ITrainingService trainingService, IOptions<PagingOptions> defaultPagingOptions)
        {
            _trainingService = trainingService;
            _defaultPagingOptions = defaultPagingOptions.Value;
        }

        [HttpPost("add-new", Name = nameof(AddTraining))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> AddTraining(TrainingModel model)
        {
            return Result(await _trainingService.AddTraining(model));
        }

        [HttpGet("trainings", Name = nameof(ListTraining))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<PagedCollection<TrainingView>>>> ListTraining([FromQuery] PagingOptions options, [FromQuery] Guid superAdminId, [FromQuery] Guid? trainingId = null, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            options.Replace(_defaultPagingOptions);
            return Ok(await _trainingService.ListTraining(options, superAdminId, trainingId, search, dateFilter));
        }

        [HttpGet("user-training-status", Name = nameof(ListTrainingStatus))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<PagedCollection<TrainingAssigneeView>>>> ListTrainingStatus([FromQuery] PagingOptions options, [FromQuery] Guid trainingId)
        {
            options.Replace(_defaultPagingOptions);
            return Ok(await _trainingService.ListTrainingStatus(options, trainingId));
        }

        [HttpPost("delete-training", Name = nameof(DeleteTraining))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> DeleteTraining([FromQuery] Guid trainingId)
        {
            return Result(await _trainingService.DeleteTraining(trainingId));
        }

        [HttpPost("delete-file", Name = nameof(DeleteTrainingFile))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> DeleteTrainingFile([FromQuery] Guid fileId)
        {
            return Result(await _trainingService.DeleteTrainingFile(fileId));
        }

        [HttpPost("add-file", Name = nameof(AddNewFile))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> AddNewFile(NewTrainingFileModel model)
        {
            return Result(await _trainingService.AddNewFile(model));
        }

        [HttpPost("unassign-user", Name = nameof(DeleteAssignedUser))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> DeleteAssignedUser([FromQuery] Guid userId, [FromQuery] Guid trainingId)
        {
            return Result(await _trainingService.DeleteAssignedUser(userId, trainingId));
        }

        [HttpPost("assign-user", Name = nameof(AssignNewUser))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> AssignNewUser([FromQuery] Guid userId, [FromQuery] Guid trainingId)
        {
            return Result(await _trainingService.AssignNewUser(userId, trainingId));
        }

        [HttpPost("start-training", Name = nameof(StartTraining))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> StartTraining([FromQuery] Guid userId, [FromQuery] Guid trainingId, [FromQuery] Guid fileId)
        {
            return Result(await _trainingService.StartTraining(userId, trainingId, fileId));
        }

        [HttpPost("complete-training", Name = nameof(CompleteTraining))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<bool>>> CompleteTraining([FromQuery] Guid userId, [FromQuery] Guid trainingId, [FromQuery] Guid fileId)
        {
            return Result(await _trainingService.CompleteTraining(userId, trainingId, fileId));
        }

        [HttpGet("user-training", Name = nameof(ListUserTrainings))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<PagedCollection<TrainingMaterialView>>>> ListUserTrainings([FromQuery] PagingOptions options, [FromQuery] Guid userId)
        {
            options.Replace(_defaultPagingOptions);
            return Ok(await _trainingService.ListUserTrainings(options, userId));
        }

        [HttpGet("user-training-material", Name = nameof(ListUserTrainingMaterials))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<List<TrainingAssigneeView>>>> ListUserTrainingMaterials([FromQuery] Guid userId, [FromQuery] Guid trainingId)
        {
            return Ok(await _trainingService.ListUserTrainingMaterials(userId, trainingId));
        }

        [HttpPost("update-video-progress", Name = nameof(CreateOrUpdateVideoRecordProgress))]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<ActionResult<StandardResponse<bool>>> CreateOrUpdateVideoRecordProgress(TrainingVideoProgressLogModel model)
        {
            return Ok(await _trainingService.CreateOrUpdateVideoRecordProgress(model));
        }
    }
}
