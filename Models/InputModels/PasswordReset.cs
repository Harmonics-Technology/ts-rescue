using System;
namespace TimesheetBE.Models.InputModels
{
    public class PasswordReset
    {
        public string Code { get; set; }
        public string NewPassword { get; set; }
    }
}