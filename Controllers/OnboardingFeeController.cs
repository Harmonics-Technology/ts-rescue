using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Net.NetworkInformation;
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
    public class OnboardingFeeController : StandardControllerResponse
    {
        private readonly IOnboardingFeeService _onboradingFeeService;
        private readonly PagingOptions _defaultPagingOptions;
        public OnboardingFeeController(IOnboardingFeeService onboradingFeeService, IOptions<PagingOptions> defaultPagingOptions)
        {
            _onboradingFeeService = onboradingFeeService;
            _defaultPagingOptions = defaultPagingOptions.Value;
        }

        [HttpPost("fee", Name = nameof(AddFee))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<OnboardingFeeModel>>> AddFee(OnboardingFeeModel fee)
        {
            return Result(await _onboradingFeeService.AddOnboardingFee(fee));
        }

        [HttpPost("delete/{feeId}", Name = nameof(DeleteFee))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<bool>>> DeleteFee([FromRoute] Guid feeId)
        {
            return Result(await _onboradingFeeService.RemoveOnboardingFee(feeId));
        }

        [HttpGet("percentage-fees", Name = nameof(ListPercentageOnboardingFees))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<OnboardingFeeView>>>> ListPercentageOnboardingFees([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid paymentPartnerId)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _onboradingFeeService.GetPercentageOnboardingFees(pagingOptions, paymentPartnerId));
        }

        [HttpGet("fixed-fees", Name = nameof(ListFixedAmountFee))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<OnboardingFeeView>>>> ListFixedAmountFee([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid paymentPartnerId)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _onboradingFeeService.ListFixedAmountFee(pagingOptions, paymentPartnerId));
        }

        [HttpGet("hst", Name = nameof(GetHST))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<OnboardingFeeView>>> GetHST([FromQuery] Guid superAdminId)
        {
            return Result(await _onboradingFeeService.GetHST(superAdminId));
        }

    }
}
