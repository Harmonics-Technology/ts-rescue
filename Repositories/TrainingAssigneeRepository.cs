using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class TrainingAssigneeRepository : BaseRepository<TrainingAssignee>, ITrainingAssigneeRepository
    {
        public TrainingAssigneeRepository(AppDbContext context) : base(context)
        {
            
        }
    }
}
