using System;

namespace TimesheetBE.Models.ViewModels
{
    public class Enable2FAView
    {
        public string QrCodeUrl { get; set; }
        public string AlternativeKey { get; set; }
        public Guid SecretKey { get; set; }
        public bool Enable2FA { get; set; }
    }
}