using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.InputModels
{
    public class ExpenseRecordDownloadModel
    {
        public ExpenseRecordsToDownload Record { get; set; }
        public Guid SuperAdminId { get; set; }
        public List<string> rowHeaders { get; set; }
    }

    public enum ExpenseRecordsToDownload
    {
        ReviwedExpenses = 1,
        ApprovedExpenses
    }
}
