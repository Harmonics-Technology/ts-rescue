using ClosedXML.Excel;
using System.Collections.Generic;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Models.InputModels;

namespace TimesheetBE.Utilities.Abstrctions
{
    public interface IDataExport
    {
        byte[] ExportAdminUsers(RecordsToDownload recordType, List<User> record, List<string> rowHeaders);
        byte[] ExportInvoiceRecords(InvoiceRecord recordType, List<Invoice> record, List<string> rowHeaders);
        byte[] ExportExpenseRecords(ExpenseRecordsToDownload recordType, List<Expense> record, List<string> rowHeaders);
        byte[] ExportPayslipRecords(PayslipRecordToDownload recordType, List<PaySlip> record, List<string> rowHeaders);
    }
}
