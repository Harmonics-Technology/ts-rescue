using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IEmployeeInformationRepository
    {
        EmployeeInformation CreateAndReturn(EmployeeInformation employeeInformation);
        IQueryable<EmployeeInformation> Query();
        EmployeeInformation Update(EmployeeInformation employeeInformation);
    }
}