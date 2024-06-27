using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Services.Abstractions;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;

namespace TimesheetBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExportController : StandardControllerResponse
    {
        private readonly IUserService _userService;
        private readonly IInvoiceService _invoiceService;
        private readonly IExpenseService _expenseService;
        private readonly IPaySlipService _paySlipService;
        private readonly ITimeSheetService _timeSheetService;
        private readonly IProjectManagementService _projectManagementService;
        public ExportController(IUserService userService, IInvoiceService invoiceService, IExpenseService expenseService, IPaySlipService paySlipService, 
            ITimeSheetService timeSheetService, IProjectManagementService projectManagementService)
        {
            _userService = userService;
            _invoiceService = invoiceService;
            _expenseService = expenseService;
            _paySlipService = paySlipService;
            _timeSheetService = timeSheetService;
            _projectManagementService = projectManagementService;
        }

        [HttpGet("users", Name = nameof(ExportUserRecord))]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public ActionResult ExportUserRecord([FromQuery] UserRecordDownloadModel model, [FromQuery] DateFilter dateFilter )
        {
           var result = _userService.ExportUserRecord(model, dateFilter);
            if (result.Status)
            {
                return File(
                        result.Data,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"{model.Record.ToString()} for {dateFilter.StartDate:D} to {dateFilter.EndDate:D}.xlsx"
                        );
            }
            return BadRequest(result);
            
        }

        [HttpGet("invoice", Name = nameof(ExportInvoiceRecord))]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public ActionResult ExportInvoiceRecord([FromQuery] InvoiceRecordDownloadModel model, [FromQuery] DateFilter dateFilter)
        {
            var result = _invoiceService.ExportInvoiceRecord(model, dateFilter);
            if (result.Status)
            {
                return File(
                        result.Data,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"{model.Record.ToString()} for {dateFilter.StartDate:D} to {dateFilter.EndDate:D}.xlsx"
                        );
            }
            return BadRequest(result);

        }

        [HttpGet("expense", Name = nameof(ExportExpenseRecord))]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public ActionResult ExportExpenseRecord([FromQuery] ExpenseRecordDownloadModel model, [FromQuery] DateFilter dateFilter)
        {
            var result = _expenseService.ExportExpenseRecord(model, dateFilter);
            if (result.Status)
            {
                return File(
                        result.Data,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"{model.Record.ToString()} for {dateFilter.StartDate:D} to {dateFilter.EndDate:D}.xlsx"
                        );
            }
            return BadRequest(result);

        }

        [HttpGet("payslip", Name = nameof(ExportPayslipRecord))]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public ActionResult ExportPayslipRecord([FromQuery] PayslipRecordDownloadModel model, [FromQuery] DateFilter dateFilter, [FromQuery] Guid superAdminId)
        {
            var result = _paySlipService.ExportPayslipRecord(model, dateFilter, superAdminId);
            if (result.Status)
            {
                return File(
                        result.Data,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"{model.Record.ToString()} for {dateFilter.StartDate:D} to {dateFilter.EndDate:D}.xlsx"
                        );
            }
            return BadRequest(result);

        }

        [HttpGet("timesheet", Name = nameof(ExportTimesheetRecord))]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public ActionResult ExportTimesheetRecord([FromQuery] TimesheetRecordDownloadModel model, [FromQuery] DateFilter dateFilter, [FromQuery] Guid superAdminId)
        {
            var result = _timeSheetService.ExportTimesheetRecord(model, dateFilter, superAdminId);
            if (result.Status)
            {
                return File(
                        result.Data,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"{model.Record.ToString()} for {dateFilter.StartDate:D} to {dateFilter.EndDate:D}.xlsx"
                        );
            }
            return BadRequest(result);

        }

        [HttpGet("summary-report", Name = nameof(ExportSummaryReportRecord))]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public ActionResult ExportSummaryReportRecord([FromQuery] BudgetRecordDownloadModel model, [FromQuery] DateFilter dateFilter, [FromQuery] Guid superAdminId)
        {
            var result = _projectManagementService.ExportSummaryReportRecord(model, dateFilter, superAdminId);
            if (result.Status)
            {
                return File(
                        result.Data,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"{model.Record.ToString()} for {dateFilter.StartDate:D} to {dateFilter.EndDate:D}.xlsx"
                        );
            }
            return BadRequest(result);

        }
    }
}
