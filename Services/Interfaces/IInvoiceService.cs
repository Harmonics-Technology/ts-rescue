using System;
using System.Threading.Tasks;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Utilities;

namespace TimesheetBE.Services.Interfaces
{
    public interface IInvoiceService
    {
         Task<StandardResponse<bool>> GenerateExpenseInvoices(Guid[] expenseIds);
         Task<StandardResponse<bool>> GenerateInvoiceFromPayroll(Guid[] payrollIds);
         Task<StandardResponse<PagedCollection<InvoiceView>>> ListInvoices(PagingOptions pagingOptions, Guid superAdminId, string search = null, DateFilter dateFilter = null);
         Task<StandardResponse<PagedCollection<InvoiceView>>> ListInvoicesByPaymentPartner(PagingOptions pagingOptions, string search = null, Guid? payrollGroupId = null, DateFilter dateFilter = null);
         Task<StandardResponse<PagedCollection<InvoiceView>>> ListTeamMemberInvoices(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null, int? status = null);
         Task<StandardResponse<bool>> SubmitInvoice(Guid invoiceId);
         Task<StandardResponse<PagedCollection<InvoiceView>>> ListSubmittedOnshoreInvoices(PagingOptions pagingOptions, Guid superAdminId, string search = null, DateFilter dateFilter = null);
         Task<StandardResponse<bool>> TreatSubmittedInvoice(Guid invoiceId, double rate);
         Task<StandardResponse<PagedCollection<InvoiceView>>> ListTeamMemberSubmittedInvoices(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null);
         Task<StandardResponse<PagedCollection<InvoiceView>>> ListInvoicedInvoices(PagingOptions pagingOptions, Guid superAdminId, string search = null, DateFilter dateFilter = null, int? payrollTypeFilter = null);
         Task<StandardResponse<PagedCollection<InvoiceView>>> ListSubmittedOffshoreInvoices(PagingOptions pagingOptions, Guid superAdminId, string search = null, DateFilter dateFilter = null);
         Task<StandardResponse<PagedCollection<InvoiceView>>> ListSubmittedInvoices(PagingOptions pagingOptions, Guid superAdminId, string search = null, 
             DateFilter dateFilter = null, int? payrollTypeFilter = null, bool? convertedInvoices = null);
         Task<StandardResponse<PagedCollection<InvoiceView>>> ListPendingInvoiceForPaymentPartner(PagingOptions pagingOptions, string search = null, Guid? payrollGroupId = null, DateFilter dateFilter = null);
         Task<StandardResponse<PagedCollection<InvoiceView>>> ListClientTeamMemberInvoices(PagingOptions pagingOptions, Guid clientId, string search = null, DateFilter dateFilter = null);
         Task<StandardResponse<InvoiceView>> CreatePaymentPartnerInvoice(PaymentPartnerInvoiceModel model);
         Task<StandardResponse<bool>> RejectPaymentPartnerInvoice(RejectPaymentPartnerInvoiceModel model);
         Task<StandardResponse<PagedCollection<InvoiceView>>> ListPaymentPartnerInvoices(PagingOptions pagingOptions, Guid superAdminId, string search = null, Guid? payrollGroupId = null, DateFilter dateFilter = null);
         Task<StandardResponse<PagedCollection<InvoiceView>>> ListClientInvoices(PagingOptions pagingOptions, Guid? clientId = null, string search = null, DateFilter dateFilter = null);
         Task<StandardResponse<PagedCollection<InvoiceView>>> ListAllClientInvoices(PagingOptions pagingOptions, Guid superAdminId, string search = null, DateFilter dateFilter = null);
         Task<StandardResponse<PagedCollection<InvoiceView>>> ListPayrollGroupInvoices(Guid superAdminId, PagingOptions pagingOptions, Guid? payrollGroupId = null, string search = null, DateFilter dateFilter = null);
         Task<StandardResponse<PagedCollection<InvoiceView>>> ListInvoicesHistories(PagingOptions pagingOptions, Guid superAdminId, string search = null, DateFilter dateFilter = null);
         Task<StandardResponse<PagedCollection<InvoiceView>>> ListPaymentPartnerInvoicesForPayrollManagers(PagingOptions pagingOptions, string search = null, Guid? payrollGroupId = null, DateFilter dateFilter = null);
         Task<StandardResponse<PagedCollection<InvoiceView>>> ListPendingInvoicedInvoicesForPaymentPartner(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null);
         StandardResponse<byte[]> ExportInvoiceRecord(InvoiceRecordDownloadModel model, DateFilter dateFilter);
    }
}