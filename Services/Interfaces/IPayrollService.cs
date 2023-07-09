using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.InputModels;
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
        Task<StandardResponse<object>> GenerateCustomMonthlyPaymentScheduleWeekPeriod(PayScheduleGenerationModel model);
        Task<StandardResponse<object>> GenerateCustomFullMonthPaymentSchedule(int paymentDay, Guid superAdminId);
        Task<StandardResponse<object>> GenerateBiWeeklyPaymentSchedule(int year);
        Task<StandardResponse<object>> GenerateCustomBiWeeklyPaymentSchedule(PayScheduleGenerationModel model);
        Task<StandardResponse<object>> GenerateWeeklyPaymentSchedule(int year);
        Task<StandardResponse<List<PaymentSchedule>>> GetPaymentSchedule(Guid employeeInformationId);
        Task<StandardResponse<List<EmployeePayScheduleView>>> GetEmployeePaySchedule(Guid employeeInformationId);
        Task<StandardResponse<List<AdminPaymentScheduleView>>> GetPaymentSchedules(Guid superAdminId);
        Task<StandardResponse<object>> GetMonthlyPaySchedule(Guid superAdminId);
        Task<StandardResponse<object>> GetBiWeeklyPaySchedule(Guid superAdminId);
        Task<StandardResponse<object>> GetWeeklyPaySchedule();
    }
}
