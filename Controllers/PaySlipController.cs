using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;

namespace TimesheetBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaySlipController : StandardControllerResponse
    {
        private readonly IPaySlipService _paySlipService;
        private readonly PagingOptions _defaultPagingOptions;

        public PaySlipController(IPaySlipService paySlipService, IOptions<PagingOptions> defaultPagingOptions)
        {
            _paySlipService = paySlipService;
            _defaultPagingOptions = defaultPagingOptions.Value;
        }

        [HttpGet("team-members/{employeeInformationId}", Name = nameof(GetTeamMembersPaySlips))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<StandardResponse<PagedCollection<PayslipUserView>>>> GetTeamMembersPaySlips(Guid employeeInformationId, [FromQuery] PagingOptions pagingOptions, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null, [FromQuery] int? payrollTypeFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _paySlipService.GetTeamMembersPaySlips(employeeInformationId, pagingOptions, search, dateFilter, payrollTypeFilter));
        }

        [HttpGet("all", Name = nameof(GetAllPaySlips))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<StandardResponse<PagedCollection<PayslipUserView>>>> GetAllPaySlips([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid superAdminId, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null, [FromQuery] int? payrollTypeFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _paySlipService.GetAllPaySlips(pagingOptions, superAdminId, search, dateFilter, payrollTypeFilter));
        }
    }
}