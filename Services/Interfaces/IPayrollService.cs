using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Utilities;

namespace TimesheetBE.Services.Interfaces
{
    public interface IPayrollService
    {
        Task<StandardResponse<PagedCollection<PayrollView>>> ListPayrolls(PagingOptions pagingOptions, Guid? employeeInformationId = null);
        Task<StandardResponse<PagedCollection<PayrollView>>> ListPendingPayrolls(PagingOptions pagingOptions, Guid? employeeInformationId = null);
        Task<StandardResponse<PagedCollection<PayrollView>>> ListApprovedPayrolls(PagingOptions pagingOptions, Guid? employeeInformationId = null);
        Task<StandardResponse<bool>> ApprovePayroll(Guid payrollId);
        Task<StandardResponse<bool>> GeneratePaySlip(Guid payrollId);
        Task<StandardResponse<PagedCollection<PaySlipView>>> ListPaySlips(PagingOptions pagingOptions, Guid? employeeInformationId = null, DateFilter dateFilter = null);
        Task<StandardResponse<PagedCollection<PayrollView>>> ListPayrollsByPaymentPartner(PagingOptions pagingOptions);
        Task<StandardResponse<PagedCollection<PaySlipView>>> ListPaySlipsByTeamMember(PagingOptions pagingOptions, string search = null);
        Task<StandardResponse<PagedCollection<PayrollView>>> ListClientTeamMembersPayroll(PagingOptions pagingOptions);
        Task<StandardResponse<object>> GenerateMonthlyPaymentSchedule(int year);
        Task<StandardResponse<object>> GenerateBiWeeklyPaymentSchedule(int year);
        Task<StandardResponse<object>> GenerateWeeklyPaymentSchedule(int year);
        Task<StandardResponse<List<PaymentSchedule>>> GetPaymentSchedule(Guid EmployeeInformationId);
        Task<StandardResponse<List<AdminPaymentScheduleView>>> GetPaymentSchedules();
    }
}
