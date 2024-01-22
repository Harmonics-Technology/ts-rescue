using System.Threading.Tasks;
using System;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Abstrctions;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using TimesheetBE.Utilities.Constants;
using TimesheetBE.Services.Interfaces;

namespace TimesheetBE.Services
{
    public class UtilityService : IUtilityService
    {
        private readonly Globals _appSettings;
        private readonly IEmailHandler _emailHandler;
        public UtilityService(IEmailHandler emailHandler, IOptions<Globals> appSettings)
        {
            _emailHandler = emailHandler;
            _appSettings = appSettings.Value;
        }

        public async Task<StandardResponse<bool>> SendContactMessage(ContactMessageModel model)
        {
            try
            {
                List<KeyValuePair<string, string>> EmailParameters = new()
                                {
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_LOGO_URL, _appSettings.LOGO),
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_CONTACTSUBJECT, model.Subject),
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_CONTACTFULLNAME, model.FullName),
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_CONTACTEMAIL, model.Email),
                                    new KeyValuePair<string, string>(Constants.EMAIL_STRING_REPLACEMENTS_CONTACTMESSAGE, model.Message),
                                };

                var EmailTemplate = _emailHandler.ComposeFromTemplate(Constants.CONTACT_US_FILENAME, EmailParameters);
                var SendEmail = _emailHandler.SendEmail(_appSettings.ContactUsEmail, "New Message", EmailTemplate, "");

                return StandardResponse<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return StandardResponse<bool>.Error("Error creating subtask");
            }
        }
    }
}
