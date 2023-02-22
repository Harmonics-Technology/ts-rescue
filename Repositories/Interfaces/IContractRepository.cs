using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IContractRepository
    {
        Contract CreateAndReturn(Contract contract);
        IQueryable<Contract> Query();
        Contract Update(Contract contract);
    }
}