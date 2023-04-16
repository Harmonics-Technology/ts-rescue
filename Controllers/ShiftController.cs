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
        public async Task<ActionResult<StandardResponse<PagedCollection<UsersShiftView>>>> ListUsersShift([FromQuery] PagingOptions pagingOptions, [FromQuery] UsersShiftModel model, [FromQuery] Guid? filterUserId = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _shiftService.ListUsersShift(pagingOptions, model, filterUserId));
        }
    }
}
