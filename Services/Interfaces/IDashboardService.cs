using System;
using System.Threading.Tasks;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Utilities;

namespace TimesheetBE.Services.Interfaces
{
    public interface IDashboardService
    {
         Task<StandardResponse<DashboardView>> GetDashBoardMetrics(Guid superAminId);
         Task<StandardResponse<DashboardTeamMemberView>> GetTeamMemberDashBoard(Guid employeeInformationId);
         Task<StandardResponse<DashboardPaymentPartnerView>> GetPaymentPartnerDashboard();
        Task<StandardResponse<DashboardClientView>> GetClientDashBoard();
        Task<StandardResponse<DashboardClientView>> GetSupervisorDashBoard();
    }
}