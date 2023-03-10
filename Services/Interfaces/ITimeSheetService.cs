using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Utilities;

namespace TimesheetBE.Services.Interfaces
{
    public interface ITimeSheetService
    {
        Task<StandardResponse<PagedCollection<TimeSheetHistoryView>>> ListTimeSheetHistories(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null);
        Task<StandardResponse<TimeSheetMonthlyView>> GetTimeSheet(Guid employeeInformationId, DateTime date);
        Task<StandardResponse<TimeSheetMonthlyView>> GetTimeSheet2(Guid employeeInformationId, DateTime date);
        Task<StandardResponse<bool>> ApproveTimeSheetForAWholeMonth(Guid employeeInformationId, DateTime date);
        Task<StandardResponse<bool>> ApproveTimeSheetForADay(Guid employeeInformationId, DateTime date);
        Task<StandardResponse<bool>> AddWorkHoursForADay(Guid employeeInformationId, DateTime date, int hours);
        Task<StandardResponse<PagedCollection<TimeSheetApprovedView>>> GetApprovedTimeSheet(PagingOptions pagingOptions, string search = null);
        Task<StandardResponse<PagedCollection<TimeSheetApprovedView>>> GetApprovedTeamMemberTimeSheet(PagingOptions pagingOptions, Guid employeeInformationId);
        Task<StandardResponse<bool>> RejectTimeSheetForADay(RejectTimeSheetModel model);
        Task<StandardResponse<bool>> GeneratePayroll(Guid employeeInformationId);
        Task<StandardResponse<PagedCollection<TimeSheetHistoryView>>> GetTeamMemberTimeSheetHistory(PagingOptions pagingOptions);
        Task<StandardResponse<PagedCollection<TimeSheetHistoryView>>> GetSuperviseesTimeSheet(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null);
        Task<StandardResponse<PagedCollection<TimeSheetApprovedView>>> GetApprovedClientTeamMemberTimeSheet(PagingOptions pagingOptions, string search = null);
        Task<StandardResponse<PagedCollection<TimeSheetHistoryView>>> GetClientTimeSheetHistory(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null);
        Task<StandardResponse<PagedCollection<TimeSheetApprovedView>>> GetSuperviseesApprovedTimeSheet(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null);
        Task<StandardResponse<PagedCollection<RecentTimeSheetView>>> GetTeamMemberRecentTimeSheet(PagingOptions pagingOptions, DateFilter dateFilter = null);
        double? GetOffshoreTeamMemberTotalPay(Guid? employeeInformationId, DateTime startDate, DateTime endDate, int totalHoursworked);
        TimeSheetApprovedView GetRecentlyApprovedTimeSheet(User user);
    }
}