using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Constants;

namespace TimesheetBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : StandardControllerResponse
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("admin-metrics", Name = nameof(GetAdminMetrics))]
        [Authorize(Roles = "Super Admin, Admin, Payroll Manager, Internal Payroll Manager")]
        public async Task<ActionResult<StandardResponse<DashboardView>>> GetAdminMetrics([FromQuery] Guid superAminId)
        {
            return Result(await _dashboardService.GetDashBoardMetrics(superAminId));
        }

        [HttpGet("team-member-metrics", Name = nameof(GetTeamMemberMetrics))]
        [Authorize(Roles = RoleConstants.TEAM_MEMBER_ROLES)]
        public async Task<ActionResult<StandardResponse<DashboardTeamMemberView>>> GetTeamMemberMetrics([FromQuery] Guid employeeInformationId)
        {
            return Result(await _dashboardService.GetTeamMemberDashBoard(employeeInformationId));
        }

        [HttpGet("payment-partner-metrics", Name = nameof(GetPayrollManagerMetrics))]
        [Authorize(Roles = "Payment Partner")]
        public async Task<ActionResult<StandardResponse<DashboardPaymentPartnerView>>> GetPayrollManagerMetrics()
        {
            return Result(await _dashboardService.GetPaymentPartnerDashboard());
        }

        [HttpGet("client-metrics", Name = nameof(GetClientMetrics))]
        [Authorize(Roles = "client")]
        public async Task<ActionResult<StandardResponse<DashboardPaymentPartnerView>>> GetClientMetrics()
        {
            return Result(await _dashboardService.GetClientDashBoard());
        }

        [HttpGet("supervisor-metrics", Name = nameof(GetSupervisorMetrics))]
        [Authorize(Roles = RoleConstants.SUPERVISOR_ROLES)]
        public async Task<ActionResult<StandardResponse<DashboardPaymentPartnerView>>> GetSupervisorMetrics()
        {
            return Result(await _dashboardService.GetSupervisorDashBoard());
        }

        [HttpGet("project-management-metrics", Name = nameof(GetProjectManagementDashboard))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<DashboardProjectManagementView>>> GetProjectManagementDashboard([FromQuery] Guid superAminId)
        {
            return Result(await _dashboardService.GetProjectManagementDashboard(superAminId));
        }

        [HttpGet("project-metrics", Name = nameof(GetProjectDashboard))]
        [Authorize]
        public async Task<ActionResult<StandardResponse<DashboardProjectView>>> GetProjectDashboard([FromQuery] Guid projectId)
        {
            return Result(await _dashboardService.GetProjectDashboard(projectId));
        }
    }
}