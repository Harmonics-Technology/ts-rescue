using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class TrainingFileRepository : BaseRepository<TrainingFile>, ITrainingFileRepository
    {
        public TrainingFileRepository(AppDbContext context) : base(context)
        {
            
        }
    }
}
