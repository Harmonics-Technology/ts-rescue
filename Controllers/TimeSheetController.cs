using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Constants;

namespace TimesheetBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeSheetController : StandardControllerResponse
    {
        
        private readonly ITimeSheetService _timeSheetService;
        private readonly PagingOptions _defaultPagingOptions;

        public TimeSheetController(IOptions<PagingOptions> defaultPagingOptions, ITimeSheetService timeSheetService)
        {
            _defaultPagingOptions = defaultPagingOptions.Value;
            _timeSheetService = timeSheetService;
        }

        [HttpGet("history", Name = nameof(ListTimeSheetHistories))]
        public async Task<ActionResult<StandardResponse<PagedCollection<TimeSheetHistoryView>>>> ListTimeSheetHistories([FromQuery] PagingOptions pagingOptions, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _timeSheetService.ListTimeSheetHistories(pagingOptions, search, dateFilter));
        }

        [HttpGet("monthly", Name = nameof(GetTimeSheet))]
        public async Task<ActionResult<StandardResponse<IEnumerable<TimeSheetMonthlyView>>>> GetTimeSheet([FromQuery] Guid employeeInformationId, [FromQuery] DateTime date)
        {
            return Result(await _timeSheetService.GetTimeSheet(employeeInformationId, date));
        }

        [HttpGet("monthly2", Name = nameof(GetTimeSheet2))]
        public async Task<ActionResult<StandardResponse<IEnumerable<TimeSheetMonthlyView>>>> GetTimeSheet2([FromQuery] Guid employeeInformationId, [FromQuery] DateTime date)
        {
            return Result(await _timeSheetService.GetTimeSheet2(employeeInformationId, date));
        }

        [HttpPost("approve/monthly", Name = nameof(ApproveTimeSheetForAWholeMonth))]
        public async Task<ActionResult<StandardResponse<bool>>> ApproveTimeSheetForAWholeMonth([FromQuery] Guid employeeInformationId, [FromQuery] DateTime date)
        {
            return Result(await _timeSheetService.ApproveTimeSheetForAWholeMonth(employeeInformationId, date));
        }

        [HttpPost("approve/daily", Name = nameof(ApproveTimeSheetForADay))]
        public async Task<ActionResult<StandardResponse<bool>>> ApproveTimeSheetForADay([FromQuery] Guid employeeInformationId, [FromQuery] DateTime date)
        {
            return Result(await _timeSheetService.ApproveTimeSheetForADay(employeeInformationId, date));
        }

        [HttpPost("add-hour", Name = nameof(AddWorkHoursForADay))]
        public async Task<ActionResult<StandardResponse<bool>>> AddWorkHoursForADay([FromQuery] Guid employeeInformationId, [FromQuery] DateTime date, [FromQuery] int hours)
        {
            return Result(await _timeSheetService.AddWorkHoursForADay(employeeInformationId, date, hours));
        }

        [HttpGet("approved", Name = nameof(ListApprovedTimeSheet))]
        public async Task<ActionResult<StandardResponse<PagedCollection<TimeSheetApprovedView>>>> ListApprovedTimeSheet([FromQuery] PagingOptions pagingOptions, [FromQuery] string search = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _timeSheetService.GetApprovedTimeSheet(pagingOptions, search));
        }

        [HttpGet("team-member/approved", Name = nameof(ListTeamMemberApprovedTimeSheet))]
        public async Task<ActionResult<StandardResponse<PagedCollection<TimeSheetApprovedView>>>> ListTeamMemberApprovedTimeSheet([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid employeeInformationId)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _timeSheetService.GetApprovedTeamMemberTimeSheet(pagingOptions, employeeInformationId));
        }

        [HttpPost("reject", Name = nameof(RejectTimeSheetForADay))]
        public async Task<ActionResult<StandardResponse<bool>>> RejectTimeSheetForADay([FromBody] RejectTimeSheetModel model)
        {
            return Result(await _timeSheetService.RejectTimeSheetForADay(model));
        }

        [HttpPost("generate-payroll", Name = nameof(GeneratePayroll))]
        [Authorize(Roles = RoleConstants.SUPERVISOR_ROLES)]
        public async Task<ActionResult<StandardResponse<bool>>> GeneratePayroll([FromQuery] Guid employeeInformationId)
        {
            return Result(await _timeSheetService.GeneratePayroll(employeeInformationId));
        }

        [HttpGet("team-member/history", Name = nameof(GetTeamMemberTimeSheetHistory))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<PagedCollection<TimeSheetHistoryView>>>> GetTeamMemberTimeSheetHistory([FromQuery] PagingOptions pagingOptions)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _timeSheetService.GetTeamMemberTimeSheetHistory(pagingOptions));
        }

        [HttpGet("team-member/recent-timesheet", Name = nameof(GetTeamMemberRecentTimeSheet))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<PagedCollection<TimeSheetHistoryView>>>> GetTeamMemberRecentTimeSheet([FromQuery] PagingOptions pagingOptions, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _timeSheetService.GetTeamMemberRecentTimeSheet(pagingOptions, dateFilter));
        }

        [HttpGet("supervisees-timesheets", Name = nameof(GetSuperviseesTimeSheet))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<PagedCollection<TimeSheetHistoryView>>>> GetSuperviseesTimeSheet([FromQuery] PagingOptions pagingOptions, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _timeSheetService.GetSuperviseesTimeSheet(pagingOptions, search, dateFilter));
        }

        [HttpGet("supervisees-approved-timesheets", Name = nameof(GetSuperviseesApprovedTimeSheet))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<PagedCollection<TimeSheetApprovedView>>>> GetSuperviseesApprovedTimeSheet([FromQuery] PagingOptions pagingOptions, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _timeSheetService.GetSuperviseesApprovedTimeSheet(pagingOptions, search, dateFilter));
        }

        [HttpGet("client/team-members/approved", Name = nameof(GetApprovedClientTeamMemberSheet))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<PagedCollection<TimeSheetApprovedView>>>> GetApprovedClientTeamMemberSheet([FromQuery] PagingOptions pagingOptions, [FromQuery] string search = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _timeSheetService.GetApprovedClientTeamMemberTimeSheet(pagingOptions, search));
        }

        [HttpGet("client/team-members/history", Name = nameof(GetClientTimeSheetHistory))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<PagedCollection<TimeSheetHistoryView>>>> GetClientTimeSheetHistory([FromQuery] PagingOptions pagingOptions, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _timeSheetService.GetClientTimeSheetHistory(pagingOptions, search, dateFilter));
        }
    }
}