using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface ITrainingFileRepository
    {
        TrainingFile CreateAndReturn(TrainingFile entity);
        TrainingFile Update(TrainingFile entity);
        void Delete(TrainingFile entity);
        IQueryable<TrainingFile> Query();
    }
}
