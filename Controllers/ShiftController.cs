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
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;

namespace TimesheetBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ShiftController : StandardControllerResponse
    {
        private readonly IShiftService _shiftService;
        private readonly PagingOptions _defaultPagingOptions;
        public ShiftController(IShiftService shiftService, IOptions<PagingOptions> defaultPagingOptions)
        {
            _shiftService = shiftService;
            _defaultPagingOptions = defaultPagingOptions.Value;
        }

        [HttpPost("add-shift", Name = nameof(AddShift))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<ShiftView>>> AddShift(ShiftModel model)
        {
            return Result(await _shiftService.CreateShift(model));
        }

        [HttpGet("shifts", Name = nameof(ListUsersShift))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<List<ShiftView>>>> ListUsersShift([FromQuery] UsersShiftModel model, [FromQuery] bool? isPublished = null)
        {
            return Result(await _shiftService.ListUsersShift(model, isPublished));
        }

        [HttpGet("users/shifts", Name = nameof(GetUsersShift))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<UsersShiftView>>>> GetUsersShift([FromQuery] PagingOptions pagingOptions, [FromQuery] UsersShiftModel model)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _shiftService.GetUsersShift(pagingOptions, model));
        }

        [HttpGet("user/shifts", Name = nameof(GetUserShift))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<UsersShiftView>>>> GetUserShift([FromQuery] PagingOptions pagingOptions, [FromQuery] UsersShiftModel model)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _shiftService.GetUserShift(pagingOptions, model));
        }

        [HttpPost("shifts/publish", Name = nameof(PublishShifts))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<bool>>> PublishShifts([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            return Result(await _shiftService.PublishShifts(startDate, endDate));
        }

        [HttpPost("shift/delete", Name = nameof(DeleteShift))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<bool>>> DeleteShift([FromQuery] Guid id)
        {
            return Result(await _shiftService.DeleteShift(id));
        }

        [HttpPost("swap", Name = nameof(SwapShift))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<bool>>> SwapShift([FromQuery] ShiftSwapModel model)
        {
            return Result(await _shiftService.SwapShift(model));
        }

        [HttpPost("shift/treat-swap", Name = nameof(ApproveSwap))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<bool>>> ApproveSwap([FromQuery] Guid id, [FromQuery] int action)
        {
            return Result(await _shiftService.ApproveSwap(id, action));
        }

        [HttpGet("user/swaps", Name = nameof(GetUserSwapShifts))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ShiftView>>>> GetUserSwapShifts([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid userId)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _shiftService.GetUserSwapShifts(pagingOptions, userId));
        }

        [HttpGet("swaps", Name = nameof(GetAllSwapShifts))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ShiftView>>>> GetAllSwapShifts([FromQuery] PagingOptions pagingOptions)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _shiftService.GetAllSwapShifts(pagingOptions));
        }
    }
}
