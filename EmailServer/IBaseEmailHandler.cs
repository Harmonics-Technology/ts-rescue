using System.Threading.Tasks;

namespace TimesheetBE.Utilities.Abstrctions
{
    public interface IBaseEmailHandler
    {
        Task<bool> SendEmail(string email, string subject, string message, string sendersName);
    }
}