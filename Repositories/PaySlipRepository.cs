using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class PaySlipRepository : BaseRepository<PaySlip>, IPaySlipRepository
    {
        public PaySlipRepository(AppDbContext context) : base(context)
        {

        }
    }
}
