using ClosedXML.Excel;
using System.Collections.Generic;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Models.InputModels;

namespace TimesheetBE.Utilities.Abstrctions
{
    public interface IDataExport
    {
        byte[] ExportAdminUsers(RecordsToDownload recordType, List<User> record);
    }
}
