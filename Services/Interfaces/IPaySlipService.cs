using System;
using System.Threading.Tasks;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Utilities;

namespace TimesheetBE.Services.Interfaces
{
    public interface IPaySlipService
    {
        Task<StandardResponse<PagedCollection<PaySlipView>>> GetTeamMembersPaySlips(Guid EmployeeInformationId, PagingOptions options, string search = null, DateFilter dateFilter = null, int? payrollTypeFilter = null);
        Task<StandardResponse<PagedCollection<PaySlipView>>> GetAllPaySlips(PagingOptions options, Guid superAdminId, string search = null, DateFilter dateFilter = null, int? payrollTypeFilter = null);
        StandardResponse<byte[]> ExportPayslipRecord(PayslipRecordDownloadModel model, DateFilter dateFilter, Guid superAdminId);
    }
}