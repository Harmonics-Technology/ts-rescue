using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface ITrainingAssigneeRepository
    {
        TrainingAssignee CreateAndReturn(TrainingAssignee entity);
        TrainingAssignee Update(TrainingAssignee entity);
        void Delete(TrainingAssignee entity);
        IQueryable<TrainingAssignee> Query();
    }
}
