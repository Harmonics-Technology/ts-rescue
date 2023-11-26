using System.Collections.Generic;

namespace TimesheetBE.Models.InputModels
{
    public class BudgetRecordDownloadModel
    {
        public BudgetRecordToDownload Record { get; set; }
        public List<string> rowHeaders { get; set; }
    }

    public enum BudgetRecordToDownload
    {
        SummaryReport = 1
    }
}
