using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class ExpenseTypeRepository : BaseRepository<ExpenseType>, IExpenseTypeRepository
    {
        public ExpenseTypeRepository(AppDbContext context) : base(context)
        {
        }
    }
}