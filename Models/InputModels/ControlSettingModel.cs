﻿using System;

namespace TimesheetBE.Models.InputModels
{
    public class ControlSettingModel
    {
        public Guid SuperAdminId { get; set; }
        public bool? TwoFactorEnabled { get; set; }
        public bool? AdminOBoarding { get; set; }
        public bool? AdminContractManagement { get; set; }
        public bool? AdminLeaveManagement { get; set; }
        public bool? AdminShiftManagement { get; set; }
        public bool? AdminReport { get; set; }
        public bool? AdminExpenseTypeAndHST { get; set; }
        public bool? AllowShiftSwapRequest { get; set; }
        public bool? AllowShiftSwapApproval { get; set; }
        public bool? AllowIneligibleLeaveCode { get; set; }
    }
}
