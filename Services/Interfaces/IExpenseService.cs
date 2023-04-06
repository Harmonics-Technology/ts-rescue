using System;
using System.Threading.Tasks;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Utilities;

namespace TimesheetBE.Services.Interfaces
{
    public interface IExpenseService
    {
        Task<StandardResponse<ExpenseView>> AddExpense(ExpenseModel expense);
        Task<StandardResponse<PagedCollection<ExpenseView>>> ListExpenses(PagingOptions pagingOptions, Guid? employeeInformationId = null, string search = null, DateFilter dateFilter = null);
        Task<StandardResponse<PagedCollection<ExpenseView>>> ListReviewedExpenses(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null);
        Task<StandardResponse<ExpenseView>> ReviewExpense(Guid expenseId);
        Task<StandardResponse<ExpenseView>> ApproveExpense(Guid expenseId);
        Task<StandardResponse<ExpenseView>> DeclineExpense(Guid expenseId);
        Task<StandardResponse<PagedCollection<ExpenseView>>> ListApprovedExpenses(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null);
        Task<StandardResponse<PagedCollection<ExpenseView>>> ListAllApprovedExpenses(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null);
        Task<StandardResponse<PagedCollection<ExpenseView>>> ListExpensesForPaymentPartner(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null);
        Task<StandardResponse<PagedCollection<ExpenseView>>> ListSuperviseesExpenses(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null);
        Task<StandardResponse<PagedCollection<ExpenseView>>> ListClientTeamMembersExpenses(PagingOptions pagingOptions, string search = null, DateFilter dateFilter = null);
        StandardResponse<byte[]> ExportExpenseRecord(ExpenseRecordDownloadModel model, DateFilter dateFilter);

    }
}