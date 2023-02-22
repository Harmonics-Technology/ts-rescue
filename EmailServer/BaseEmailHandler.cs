using TimesheetBE.EmailServer;
using TimesheetBE.Utilities.Abstrctions;
using Microsoft.Extensions.Logging;
using SendGrid;

namespace TimesheetBE.Utilities
{
    public class BaseEmailHandler
    {
        public static IBaseEmailHandler Build(EmailHandlerTypeEnum type, Globals globals, ISendGridClient _sendGridClient, ILogger<EmailHandler> logger)
        {
            switch (type)
            {
                case EmailHandlerTypeEnum.SENDGRID:
                    return new SendGridEmailHandler(globals, _sendGridClient,logger);

                case EmailHandlerTypeEnum.MAILGUN:
                    return new MailGunEmailHandler(globals);

                default:
                    return new MailGunEmailHandler(globals);
            }
        }
        public enum EmailHandlerTypeEnum
        {
            SENDGRID = 1,
            MAILGUN
        }
    }
}