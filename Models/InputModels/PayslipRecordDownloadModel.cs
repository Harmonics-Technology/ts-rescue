using System.Collections.Generic;

namespace TimesheetBE.Models.InputModels
{
    public class PayslipRecordDownloadModel
    {
        public PayslipRecordToDownload Record { get; set; }
        public List<string> rowHeaders { get; set; }
    }
    public enum PayslipRecordToDownload
    {
        Payslips = 1
    }
}
