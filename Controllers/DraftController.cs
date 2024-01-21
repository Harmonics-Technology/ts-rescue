using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Services;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;
using Microsoft.AspNetCore.Authorization;

namespace TimesheetBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DraftController : StandardControllerResponse
    {
        private readonly IUserDraftService _userDraftService;
        private readonly PagingOptions _defaultPagingOptions;
        public DraftController(IUserDraftService userDraftService, IOptions<PagingOptions> defaultPagingOptions)
        {
            _userDraftService = userDraftService;
            _defaultPagingOptions = defaultPagingOptions.Value;
        }

        [HttpPost("save-user-draft", Name = nameof(CreateDraft))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<bool>>> CreateDraft(UserDraftModel model)
        {
            return Result(await _userDraftService.CreateDraft(model));
        }

        [HttpGet("user-drafts", Name = nameof(ListDrafts))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<UserDraftView>>>> ListDrafts([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid superAdminId, [FromQuery] string role)
        {
            return Result(await _userDraftService.ListDrafts(pagingOptions, superAdminId, role));
        }

        [HttpPost("update-user-draft", Name = nameof(UpdateDraft))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<bool>>> UpdateDraft(UserDraftModel model)
        {
            return Result(await _userDraftService.UpdateDraft(model));
        }

        [HttpPost("delete-user-draft", Name = nameof(DeleteDraft))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<bool>>> DeleteDraft([FromQuery] Guid id)
        {
            return Result(await _userDraftService.DeleteDraft(id));
        }
    }
}
