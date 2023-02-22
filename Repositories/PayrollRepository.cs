using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class PayrollRepository : BaseRepository<Payroll>, IPayrollRepository
    {
        public PayrollRepository(AppDbContext context) : base(context)
        {
        }
    }
}