using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;

namespace TimesheetBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LeaveController : StandardControllerResponse
    {
        private readonly ILeaveService _leaveService;
        private readonly PagingOptions _defaultPagingOptions;
        public LeaveController(ILeaveService leaveService, IOptions<PagingOptions> defaultPagingOptions)
        {
            _leaveService = leaveService;
            _defaultPagingOptions = defaultPagingOptions.Value;
        }

        [HttpPost("leave-type", Name = nameof(AddLeaveType))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<LeaveTypeView>>> AddLeaveType(LeaveTypeModel model)
        {
            return Result(await _leaveService.AddLeaveType(model));
        }

        [HttpPost("leave-type/update", Name = nameof(UpdateLeaveType))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<LeaveTypeView>>> UpdateLeaveType([FromQuery] Guid id, LeaveTypeModel model)
        {
            return Result(await _leaveService.UpdateLeaveType(id, model));
        }

        [HttpPost("leave-type/delete", Name = nameof(DeleteLeaveType))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<bool>>> DeleteLeaveType([FromQuery] Guid id)
        {
            return Result(await _leaveService.DeleteLeaveType(id));
        }

        [HttpGet("leave-types", Name = nameof(LeaveTypes))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<LeaveTypeView>>>> LeaveTypes([FromQuery] PagingOptions pagingOptions)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _leaveService.LeaveTypes(pagingOptions));
        }

        [HttpGet("leaves", Name = nameof(ListLeaves))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<LeaveView>>>> ListLeaves([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid? supervisorId = null, [FromQuery] Guid? employeeInformationId = null, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _leaveService.ListLeaves(pagingOptions, supervisorId, employeeInformationId, search, dateFilter));
        }

        [HttpGet("treated-leaves", Name = nameof(ListAllPendingLeaves))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<LeaveView>>>> ListAllPendingLeaves([FromQuery] PagingOptions pagingOptions)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _leaveService.ListAllPendingLeaves(pagingOptions));
        }

        [HttpPost("leave", Name = nameof(CreateLeave))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<LeaveView>>> CreateLeave(LeaveModel model)
        {
            return Result(await _leaveService.CreateLeave(model));
        }

        [HttpPost("leave/treat", Name = nameof(TreatLeave))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<bool>>> TreatLeave([FromQuery] Guid id, [FromQuery] LeaveStatuses status)
        {
            return Result(await _leaveService.TreatLeave(id, status));
        }

        [HttpPost("leave/delete", Name = nameof(DeleteLeave))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<bool>>> DeleteLeave([FromQuery] Guid id)
        {
            return Result(await _leaveService.DeleteLeave(id));
        }
    }
}
