using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.InputModels
{
    public class TimesheetRecordDownloadModel
    {
        public Guid? EmployeeInformationId { get; set; }
        public TimesheetRecordToDownload Record { get; set; }
        public List<string> rowHeaders { get; set; }
    }

    public enum TimesheetRecordToDownload
    {
        TimesheetApproved = 1,
        TeamMemberApproved
    }
}
