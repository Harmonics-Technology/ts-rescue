using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Repositories
{
    public class EmployeeInformationRepository : BaseRepository<EmployeeInformation>, IEmployeeInformationRepository
    {
        public EmployeeInformationRepository(AppDbContext context) : base(context)
        {
        }
    }
}