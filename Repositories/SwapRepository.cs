using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class SwapRepository : BaseRepository<Swap>, ISwapRepository
    {
        public SwapRepository(AppDbContext context) : base(context)
        {

        }
    }
}
