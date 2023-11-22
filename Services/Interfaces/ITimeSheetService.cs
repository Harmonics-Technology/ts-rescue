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
        Task<StandardResponse<PagedCollection<TimeSheetHistoryView>>> ListTimeSheetHistories(PagingOptions pagingOptions, Guid superAdminId, string search = null, DateFilter dateFilter = null, TimesheetFilterByUserPayrollType? userFilter = null);
        Task<StandardResponse<TimeSheetMonthlyView>> GetTimeSheet(Guid employeeInformationId, DateTime date, DateTime? endDate);
        Task<StandardResponse<TimeSheetMonthlyView>> GetTimeSheet2(Guid employeeInformationId, DateTime date);
        Task<StandardResponse<bool>> ApproveTimeSheetForAWholeMonth(Guid employeeInformationId, DateTime date);
        Task<StandardResponse<bool>> ApproveTimeSheetForADay(List<TimesheetHoursApprovalModel> model, Guid employeeInformationId, DateTime date);
        Task<StandardResponse<bool>> AddWorkHoursForADay(List<TimesheetHoursAdditionModel> model, Guid employeeInformationId, DateTime date);
        Task<StandardResponse<PagedCollection<TimeSheetApprovedView>>> GetApprovedTimeSheet(PagingOptions pagingOptions, Guid superAdminId, string search = null, TimesheetFilterByUserPayrollType? userFilter = null);
        Task<StandardResponse<PagedCollection<TimeSheetApprovedView>>> GetApprovedTeamMemberTimeSheet(PagingOptions pagingOptions, Guid employeeInformationId);
        Task<StandardResponse<bool>> RejectTimeSheetForADay(RejectTimesheetModel model, Guid employeeInformationId, DateTime date);
        Task<StandardResponse<bool>> GeneratePayroll(Guid employeeInformationId);
        Task<StandardResponse<PagedCollection<TimeSheetHistoryView>>> GetTeamMemberTimeSheetHistory(PagingOptions pagingOptions);
        Task<StandardResponse<PagedCollection<TimeSheetHistoryView>>> GetSuperviseesTimeSheet(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null, TimesheetFilterByUserPayrollType? userFilter = null);
        Task<StandardResponse<PagedCollection<TimeSheetApprovedView>>> GetApprovedClientTeamMemberTimeSheet(PagingOptions pagingOptions, string search = null);
        Task<StandardResponse<PagedCollection<TimeSheetHistoryView>>> GetClientTimeSheetHistory(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null);
        Task<StandardResponse<PagedCollection<TimeSheetApprovedView>>> GetSuperviseesApprovedTimeSheet(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null, TimesheetFilterByUserPayrollType? userFilter = null);
        Task<StandardResponse<PagedCollection<RecentTimeSheetView>>> GetTeamMemberRecentTimeSheet(PagingOptions pagingOptions, Guid employeeInformationId, DateFilter dateFilter = null);
        double? GetOffshoreTeamMemberTotalPay(Guid? employeeInformationId, DateTime startDate, DateTime endDate, int totalHoursworked, int invoiceType);
        TimeSheetApprovedView GetPendingApprovalTimeSheet(User user, Guid superAdminId);
        Task<StandardResponse<bool>> CreateTimeSheetForADay(DateTime date, Guid? employeeInformationId = null);
        Task<StandardResponse<TimeSheetMonthlyView>> GetTimesheetByPaySchedule(Guid employeeInformationId, DateTime startDate, DateTime endDate);
        Task<StandardResponse<bool>> AddProjectManagementTimeSheet(Guid userId, DateTime startDate, DateTime endDate);
        double? GetTeamMemberPayPerHour(Guid userId, DateTime date);
        StandardResponse<byte[]> ExportTimesheetRecord(TimesheetRecordDownloadModel model, DateFilter dateFilter, Guid superAdminId);
        Task<StandardResponse<bool>> TreatProjectManagementTimeSheet(Guid userId, bool isApproved, DateTime startDate, DateTime endDate, string reason = null);
        TimeSheetHistoryView GetTimeSheetHistory(User user, DateFilter dateFilter = null);
    }
}