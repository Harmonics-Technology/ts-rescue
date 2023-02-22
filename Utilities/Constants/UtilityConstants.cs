using System;

namespace TimesheetBE.Utilities.Constants
{
    public class UtilityConstants
    {
        public static string SendGridApiKey = Environment.GetEnvironmentVariable("SendGridApiKey");
        public static string MailGunApiKey = Environment.GetEnvironmentVariable("MailGunApiKey");
        public static string MailGunDomain = Environment.GetEnvironmentVariable("MailGunBaseUrl");

    }
}