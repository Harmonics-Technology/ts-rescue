using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Constants;

namespace TimesheetBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FinancialController : StandardControllerResponse
    {
        private readonly IExpenseService _expenseService;
        private readonly IPayrollService _payrollService;
        private readonly IInvoiceService _invoiceService;
        private readonly PagingOptions _defaultPagingOptions;

        public FinancialController(IExpenseService expenseService, IPayrollService payrollService, IInvoiceService invoiceService, IOptions<PagingOptions> defaultPagingOptions)
        {
            _expenseService = expenseService;
            _payrollService = payrollService;
            _invoiceService = invoiceService;
            _defaultPagingOptions = defaultPagingOptions.Value;
        }

        [HttpPost("expense", Name = nameof(AddExpense))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<ExpenseView>>> AddExpense(ExpenseModel expense)
        {
            return Result(await _expenseService.AddExpense(expense));
        }

        [HttpGet("expenses", Name = nameof(ListExpenses))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ExpenseView>>>> ListExpenses([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid superAdminId, [FromQuery] Guid? employeeInformationId = null, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _expenseService.ListExpenses(pagingOptions, superAdminId, employeeInformationId, search, dateFilter));
        }

        [HttpGet("supervisees-expenses", Name = nameof(ListSuperviseesExpenses))]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ExpenseView>>>> ListSuperviseesExpenses([FromQuery] PagingOptions pagingOptions, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            return Result(await _expenseService.ListSuperviseesExpenses(pagingOptions, search, dateFilter));
        }

        [HttpGet("client/team-member-expenses", Name = nameof(ListClientTeamMembersExpenses))]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ExpenseView>>>> ListClientTeamMembersExpenses([FromQuery] PagingOptions pagingOptions, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            return Result(await _expenseService.ListClientTeamMembersExpenses(pagingOptions, search, dateFilter));
        }

        [HttpGet("expenses/reviewed", Name = nameof(ListReviewedExpenses))]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ExpenseView>>>> ListReviewedExpenses([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid superAdminId, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _expenseService.ListReviewedExpenses(pagingOptions, superAdminId, search, dateFilter));
        }

        [HttpPost("expense/{expenseId}/review", Name = nameof(ReviewExpense))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<ExpenseView>>> ReviewExpense([FromRoute] Guid expenseId)
        {
            return Result(await _expenseService.ReviewExpense(expenseId));
        }

        [HttpPost("expense/{expenseId}/approve", Name = nameof(ApproveExpense))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<ExpenseView>>> ApproveExpense([FromRoute] Guid expenseId)
        {
            return Result(await _expenseService.ApproveExpense(expenseId));
        }

        [HttpPost("expense/{expenseId}/decline", Name = nameof(DeclineExpense))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<ExpenseView>>> DeclineExpense([FromRoute] Guid expenseId)
        {
            return Result(await _expenseService.DeclineExpense(expenseId));
        }

        [HttpGet("payrolls", Name = nameof(ListPayrolls))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<PayrollView>>>> ListPayrolls([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid? employeeInformationId = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _payrollService.ListPayrolls(pagingOptions, employeeInformationId));
        }

        [HttpGet("payrolls/pending", Name = nameof(ListPendingPayrolls))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<PayrollView>>>> ListPendingPayrolls([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid? employeeInformationId = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _payrollService.ListPendingPayrolls(pagingOptions, employeeInformationId));
        }

        [HttpGet("payrolls/approved", Name = nameof(ListApprovedPayrolls))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<PayrollView>>>> ListApprovedPayrolls([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid? employeeInformationId = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _payrollService.ListApprovedPayrolls(pagingOptions, employeeInformationId));
        }

        [HttpPost("payroll/approve", Name = nameof(ApprovePayroll))]
        public async Task<ActionResult<StandardResponse<bool>>> ApprovePayroll([FromQuery] Guid payrollId)
        {
            return Result(await _payrollService.ApprovePayroll(payrollId));
        }

        [HttpGet("payslips", Name = nameof(ListPaySlips))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<PaySlipView>>>> ListPaySlips([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid? employeeInformationId = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _payrollService.ListPaySlips(pagingOptions, employeeInformationId, dateFilter));
        }

        [HttpPost("generate-payslip", Name = nameof(GeneratePaySlip))]
        public async Task<ActionResult<StandardResponse<bool>>> GeneratePaySlip([FromQuery] Guid payrollId)
        {
            return Result(await _payrollService.GeneratePaySlip(payrollId));
        }

        [HttpPost("expenses/approved", Name = nameof(ListApprovedExpenses))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ExpenseView>>>> ListApprovedExpenses([FromQuery] PagingOptions pagingOptions, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            return Result(await _expenseService.ListApprovedExpenses(pagingOptions, search, dateFilter));
        }

        [HttpPost("invoice/payroll/generate", Name = nameof(GenerateInvoicePayroll))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<bool>>> GenerateInvoicePayroll([FromBody] Guid[] payrollIds)
        {
            return Result(await _invoiceService.GenerateInvoiceFromPayroll(payrollIds));
        }

        [HttpPost("invoice/expense/generate", Name = nameof(GenerateInvoiceExpense))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<bool>>> GenerateInvoiceExpense([FromBody] Guid[] expenseIds)
        {
            return Result(await _invoiceService.GenerateExpenseInvoices(expenseIds));
        }

        [HttpGet("expenses/approved/all", Name = nameof(ListAllApprovedExpenses))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ExpenseView>>>> ListAllApprovedExpenses([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid superAdminId, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _expenseService.ListAllApprovedExpenses(pagingOptions, superAdminId, search, dateFilter));
        }

        [HttpGet("invoices/list", Name = nameof(ListInvoices))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<InvoiceView>>>> ListInvoices([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid superAdminId, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _invoiceService.ListInvoices(pagingOptions, superAdminId, search, dateFilter));
        }

        [HttpGet("invoices/onshore/submitted", Name = nameof(ListSubmittedOnshoreInvoices))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<InvoiceView>>>> ListSubmittedOnshoreInvoices([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid superAdminId, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _invoiceService.ListSubmittedOnshoreInvoices(pagingOptions, superAdminId, search, dateFilter));
        }

        [HttpGet("invoices/offshore/submitted", Name = nameof(ListSubmittedOffshoreInvoices))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<InvoiceView>>>> ListSubmittedOffshoreInvoices([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid superAdminId, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _invoiceService.ListSubmittedOffshoreInvoices(pagingOptions, superAdminId, search, dateFilter));
        }

        [HttpGet("invoices/submitted", Name = nameof(ListSubmittedInvoices))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<InvoiceView>>>> ListSubmittedInvoices([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid superAdminId, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null, [FromQuery] int? payrollTypeFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _invoiceService.ListSubmittedInvoices(pagingOptions, superAdminId, search, dateFilter, payrollTypeFilter));
        }

        [HttpGet("invoices/payment-partner/pending", Name = nameof(ListPendingInvoiceForPaymentPartner))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<InvoiceView>>>> ListPendingInvoiceForPaymentPartner([FromQuery] PagingOptions pagingOptions, [FromQuery] string search = null, [FromQuery] Guid? payrollGroupId = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _invoiceService.ListPendingInvoiceForPaymentPartner(pagingOptions, search, payrollGroupId, dateFilter));
        }

        [HttpGet("invoices/payment-partner/pending-invoiced", Name = nameof(ListPendingInvoicedInvoicesForPaymentPartner))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<InvoiceView>>>> ListPendingInvoicedInvoicesForPaymentPartner([FromQuery] PagingOptions pagingOptions, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _invoiceService.ListPendingInvoicedInvoicesForPaymentPartner(pagingOptions, search, dateFilter));
        }

        [HttpGet("invoices/invoiced", Name = nameof(ListInvoicedInvoices))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<InvoiceView>>>> ListInvoicedInvoices([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid superAdminId, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null, [FromQuery] int? payrollTypeFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _invoiceService.ListInvoicedInvoices(pagingOptions, superAdminId, search, dateFilter, payrollTypeFilter));
        }

        [HttpGet("invoices/team-member/submitted", Name = nameof(ListTeamMemberSubmittedInvoices))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<InvoiceView>>>> ListTeamMemberSubmittedInvoices([FromQuery] PagingOptions pagingOptions, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _invoiceService.ListTeamMemberSubmittedInvoices(pagingOptions, search, dateFilter));
        }

        [HttpGet("invoices/team-member", Name = nameof(ListTeamMemberInvoices))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<InvoiceView>>>> ListTeamMemberInvoices([FromQuery] PagingOptions pagingOptions, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null, [FromQuery] int? status = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _invoiceService.ListTeamMemberInvoices(pagingOptions, search, dateFilter, status));
        }

        [HttpPost("invoice/submit", Name = nameof(SubmitInvoice))]
        public async Task<ActionResult<StandardResponse<bool>>> SubmitInvoice([FromQuery] Guid invoiceId)
        {
            return Result(await _invoiceService.SubmitInvoice(invoiceId));
        }

        [HttpPost("invoice/treat", Name = nameof(TreatSubmittedInvoice))]
        public async Task<ActionResult<StandardResponse<bool>>> TreatSubmittedInvoice([FromQuery] Guid invoiceId)
        {
            return Result(await _invoiceService.TreatSubmittedInvoice(invoiceId));
        }

        [HttpPost("invoice/payment-partner/reject", Name = nameof(RejectPaymentPartnerInvoice))]
        public async Task<ActionResult<StandardResponse<bool>>> RejectPaymentPartnerInvoice(RejectPaymentPartnerInvoiceModel model)
        {
            return Result(await _invoiceService.RejectPaymentPartnerInvoice(model));
        }

        [HttpGet("payrolls/paymentpartner", Name = nameof(ListPayrollsByPaymentPartner))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<PayrollView>>>> ListPayrollsByPaymentPartner([FromQuery] PagingOptions pagingOptions)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _payrollService.ListPayrollsByPaymentPartner(pagingOptions));
        }

        [HttpGet("payrolls/client-team-members", Name = nameof(ListClientTeamMembersPayroll))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<PayrollView>>>> ListClientTeamMembersPayroll([FromQuery] PagingOptions pagingOptions)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _payrollService.ListClientTeamMembersPayroll(pagingOptions));
        }

        [HttpGet("paymentpartner/invoices", Name = nameof(ListInvoicesByPaymentPartner))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<InvoiceView>>>> ListInvoicesByPaymentPartner([FromQuery] PagingOptions pagingOptions, [FromQuery] string search = null, [FromQuery] Guid? payrollGroupId = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _invoiceService.ListInvoicesByPaymentPartner(pagingOptions, search, payrollGroupId, dateFilter));
        }

        [HttpGet("paymentpartner/expenses", Name = nameof(ListExpensesByPaymentPartner))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ExpenseView>>>> ListExpensesByPaymentPartner([FromQuery] PagingOptions pagingOptions, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _expenseService.ListExpensesForPaymentPartner(pagingOptions, search, dateFilter));
        }

        [HttpGet("team-member/payslips", Name = nameof(ListPaySlipsByTeamMember))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<PaySlipView>>>> ListPaySlipsByTeamMember([FromQuery] PagingOptions pagingOptions, [FromQuery] string? search = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _payrollService.ListPaySlipsByTeamMember(pagingOptions, search));
        }

        [HttpGet("schedule/{year}", Name = nameof(GeneratePaymentSchedule))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<ActionResult<StandardResponse<bool>>> GeneratePaymentSchedule(int year)
        {
            return Result(await _payrollService.GenerateMonthlyPaymentSchedule(year));
        }

        [HttpPost("monthly/week-period", Name = nameof(GenerateCustomMonthlyPaymentScheduleWeekPeriod))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<ActionResult<StandardResponse<bool>>> GenerateCustomMonthlyPaymentScheduleWeekPeriod(PayScheduleGenerationModel model)
        {
            return Result(await _payrollService.GenerateCustomMonthlyPaymentScheduleWeekPeriod(model));
        }

        [HttpPost("monthly/full-month", Name = nameof(GenerateCustomFullMonthPaymentSchedule))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<ActionResult<StandardResponse<bool>>> GenerateCustomFullMonthPaymentSchedule([FromQuery] int paymentDay, [FromQuery] Guid superAdminId)
        {
            return Result(await _payrollService.GenerateCustomFullMonthPaymentSchedule(paymentDay, superAdminId));
        }

        [HttpGet("schedule/biweekly/{year}", Name = nameof(GenerateBiweeklyPaymentSchedule))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<ActionResult<StandardResponse<bool>>> GenerateBiweeklyPaymentSchedule(int year)
        {
            return Result(await _payrollService.GenerateBiWeeklyPaymentSchedule(year));
        }

        [HttpGet("biweekly", Name = nameof(GenerateCustomBiWeeklyPaymentSchedule))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<ActionResult<StandardResponse<bool>>> GenerateCustomBiWeeklyPaymentSchedule(PayScheduleGenerationModel model)
        {
            return Result(await _payrollService.GenerateCustomBiWeeklyPaymentSchedule(model));
        }

        [HttpGet("schedule/weekly/{year}", Name = nameof(GenerateWeeklyPaymentSchedule))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<ActionResult<StandardResponse<bool>>> GenerateWeeklyPaymentSchedule(int year)
        {
            return Result(await _payrollService.GenerateWeeklyPaymentSchedule(year));
        }

        [HttpGet("employee/schedule", Name = nameof(GetEmployeePaymentSchedule))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<ActionResult<StandardResponse<List<PaymentSchedule>>>> GetEmployeePaymentSchedule([FromQuery]Guid employeeInformationId)
        {
            return Result(await _payrollService.GetPaymentSchedule(employeeInformationId));
        }

        [HttpGet("admin/schedules", Name = nameof(GetPaymentSchedules))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<ActionResult<StandardResponse<List<AdminPaymentScheduleView>>>> GetPaymentSchedules()
        {
            return Result(await _payrollService.GetPaymentSchedules());
        }

        [HttpPost("payment-partner/invoice/create", Name = nameof(CreatePaymentPartnerInvoice))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Payment Partner")]
        public async Task<ActionResult<StandardResponse<InvoiceView>>> CreatePaymentPartnerInvoice([FromBody] PaymentPartnerInvoiceModel model)
        {
            return Result(await _invoiceService.CreatePaymentPartnerInvoice(model));
        }

        [HttpGet("payment-partner/invoices", Name = nameof(ListPaymentPartnerInvoices))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Payment Partner")]
        public async Task<ActionResult<StandardResponse<PagedCollection<InvoiceView>>>> ListPaymentPartnerInvoices([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid superAdminId, [FromQuery] string search = null, [FromQuery] Guid? payrollGroupId = null,[FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _invoiceService.ListPaymentPartnerInvoices(pagingOptions, superAdminId, search, payrollGroupId, dateFilter));
        }

        [HttpGet("payroll-group/invoices", Name = nameof(ListPayrollGroupInvoices))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [Authorize]
        public async Task<ActionResult<StandardResponse<PagedCollection<InvoiceView>>>> ListPayrollGroupInvoices([FromQuery] Guid payrollGroupId, [FromQuery] PagingOptions pagingOptions, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _invoiceService.ListPayrollGroupInvoices(payrollGroupId, pagingOptions, search, dateFilter));
        }

        [HttpGet("client/invoices", Name = nameof(ListClientInvoices))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [Authorize]
        public async Task<ActionResult<StandardResponse<PagedCollection<InvoiceView>>>> ListClientInvoices([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid? clientId = null, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _invoiceService.ListClientInvoices(pagingOptions, clientId, search, dateFilter));
        }

        [HttpGet("invoices/history", Name = nameof(ListInvoicesHistories))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [Authorize]
        public async Task<ActionResult<StandardResponse<PagedCollection<InvoiceView>>>> ListInvoicesHistories([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid superAdminId, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _invoiceService.ListInvoicesHistories(pagingOptions, superAdminId, search, dateFilter));
        }

        [HttpGet("payroll-manager-payment-partner/invoices", Name = nameof(ListPaymentPartnerInvoicesForPayrollManagers))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [Authorize]
        public async Task<ActionResult<StandardResponse<PagedCollection<InvoiceView>>>> ListPaymentPartnerInvoicesForPayrollManagers([FromQuery] PagingOptions pagingOptions, [FromQuery] string search = null, [FromQuery] Guid? payrollGroupId = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _invoiceService.ListPaymentPartnerInvoicesForPayrollManagers(pagingOptions, search, payrollGroupId, dateFilter));
        }

        [HttpGet("payroll-manager-client/invoices", Name = nameof(ListAllClientInvoices))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [Authorize]
        public async Task<ActionResult<StandardResponse<PagedCollection<InvoiceView>>>> ListAllClientInvoices([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid superAdminId, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _invoiceService.ListAllClientInvoices(pagingOptions, superAdminId, search, dateFilter));
        }

        [HttpGet("client/team-members/invoices", Name = nameof(ListClientTeamMemberInvoices))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [Authorize]
        public async Task<ActionResult<StandardResponse<PagedCollection<InvoiceView>>>> ListClientTeamMemberInvoices([FromQuery] PagingOptions pagingOptions, [FromQuery] Guid clientId, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _invoiceService.ListClientTeamMemberInvoices(pagingOptions, clientId, search, dateFilter));
        }
    }
}