using System;

namespace TimesheetBE.Models.AppModels
{
    public class ControlSetting : BaseModel
    {
        public Guid SuperAdminId { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool AdminOBoardibg { get; set; }
        public bool AdminContractManagement { get; set; }
        public bool AdminLeaveManagement { get; set; }
        public bool AdminShiftManagement { get; set; }
        public bool AdminReport { get; set; }
        public bool AdminExpenseTypeAndHST { get; set; }
    }
}
