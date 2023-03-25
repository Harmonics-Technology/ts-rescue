

using System;

namespace TimesheetBE.Utilities
{
    public class Globals
    {
        public string Secret { get; set; }
        public string ConnectionString { get; set; }

        public string SendGridApiKey { get; set; }

        public string SenderEmail { get; set; }

        public static string FrontEndBaseUrl = Environment.GetEnvironmentVariable("FrontEndBaseUrl");
        public string EmailVerificationUrl { get; set; }
        public string PasswordResetUrl { get; set; }
        public string CompletePasswordResetUrl { get; set; }
        public string ActivateTeamMemberUrl { get; set; }
        public string GoogleCredentialsFIle { get; set; }
        public string GoogleStorageBucket { get; set; }
        public string UploadDrive { get; set; }
        public string DriveName { get; set; }
        public string SendersName { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        public string Instagram { get; set; }
        public string LOGO { get; set; }
        public string MailGunBaseUrl { get; set; }
        public string MailGunApiKey { get; set; }
        public int PasswordResetExpiry { get; set; }
    }


}
