using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface ITrainingVideoProgressLogRepository
    {
        TrainingVideoProgressLog CreateAndReturn(TrainingVideoProgressLog entity);
        TrainingVideoProgressLog Update(TrainingVideoProgressLog entity);
        void Delete(TrainingVideoProgressLog entity);
        IQueryable<TrainingVideoProgressLog> Query();
    }
}
