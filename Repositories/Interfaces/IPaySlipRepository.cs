using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IPaySlipRepository
    {
        PaySlip CreateAndReturn(PaySlip paySlip);
        PaySlip Update(PaySlip paySlip);
        IQueryable<PaySlip> Query();
    }
}
