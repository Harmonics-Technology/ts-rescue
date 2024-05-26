using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class TrainingVideoProgressLogRepository : BaseRepository<TrainingVideoProgressLog>, ITrainingVideoProgressLogRepository
    {
        public TrainingVideoProgressLogRepository(AppDbContext context) : base(context)
        {
            
        }
    }
}
