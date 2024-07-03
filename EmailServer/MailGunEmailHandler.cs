using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Abstrctions;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Threading.Tasks;
using TimesheetBE.Utilities.Constants;

namespace TimesheetBE.EmailServer
{
    public class MailGunEmailHandler : IBaseEmailHandler
    {
        public Globals _globals;
        public MailGunEmailHandler(Globals globals)
        {
            _globals = globals;
        }

        public async Task<bool> SendEmail(string email, string subject, string message, string sendersName)
        {
            try
            {
                var restClient = new RestClient(UtilityConstants.MailGunDomain);
                restClient.Authenticator = new HttpBasicAuthenticator("api", UtilityConstants.MailGunApiKey);

                var restRequest = new RestRequest();
                restRequest.Resource = "messages";
                restRequest.AddParameter("from", $"{_globals.SendersName} <info@support.timba.ca>");
                restRequest.AddParameter("to", email);
                restRequest.AddParameter("subject", subject);
                restRequest.AddParameter("html", message);
                restRequest.Method = Method.Post;
                var restResponse = restClient.ExecuteAsync(restRequest).Result;

                if (restResponse.StatusCode != HttpStatusCode.OK)
                    return false;

                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }
}