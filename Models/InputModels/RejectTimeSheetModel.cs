using System.Collections.Generic;

namespace TimesheetBE.Models.InputModels
{
    public class RejectTimesheetModel
    {
        public List<RejectTimeSheetData> timeSheets { get; set; }
        public string Reason { get; set; }

    }
}
