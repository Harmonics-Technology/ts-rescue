using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class PaymentScheduleRepository : BaseRepository<PaymentSchedule>, IPaymentScheduleRepository
    {
        public PaymentScheduleRepository(AppDbContext context) : base(context)
        {
        }
    }
}