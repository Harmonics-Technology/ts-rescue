namespace TimesheetBE.Models.ViewModels
{
    public class ControlSettingView
    {
        public bool TwoFactorEnabled { get; set; }
        public bool AdminOBoardibg { get; set; }
        public bool AdminContractManagement { get; set; }
        public bool AdminLeaveManagement { get; set; }
        public bool AdminShiftManagement { get; set; }
        public bool AdminReport { get; set; }
        public bool AdminExpenseTypeAndHST { get; set; }
    }
}
