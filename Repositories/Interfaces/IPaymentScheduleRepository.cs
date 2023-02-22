using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IPaymentScheduleRepository
    {
        PaymentSchedule CreateAndReturn(PaymentSchedule model);
        IQueryable<PaymentSchedule> Query();
        PaymentSchedule Update(PaymentSchedule model);
    }
}