using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.ViewModels;

namespace TimesheetBE.Utilities.Abstrctions
{
    public interface IDataExport
    {
        byte[] ExportAdminUsers(RecordsToDownload recordType, List<User> record, List<string> rowHeaders);
        byte[] ExportInvoiceRecords(InvoiceRecord recordType, List<Invoice> record, List<string> rowHeaders);
        byte[] ExportExpenseRecords(ExpenseRecordsToDownload recordType, List<Expense> record, List<string> rowHeaders);
        byte[] ExportPayslipRecords(PayslipRecordToDownload recordType, List<PaySlip> record, List<string> rowHeaders);
        byte[] ExportTimesheetRecords(TimesheetRecordToDownload recordType, List<TimeSheetApprovedView> record, List<string> rowHeaders);
        byte[] ExportTeamMemberTimesheetRecords(TimesheetRecordToDownload recordType, List<RecentTimeSheetView> record, List<string> rowHeaders);
        byte[] ExportTimesheetHistoryRecords(TimesheetRecordToDownload recordType, List<TimeSheetHistoryView> record, List<string> rowHeaders);
    }
}
