using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface ITrainingRepository
    {
        Training CreateAndReturn(Training entity);
        Training Update(Training entity);
        void Delete(Training entity);
        IQueryable<Training> Query();
    }
}
