using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IPayrollRepository
    {
         Payroll CreateAndReturn(Payroll payroll);
         Payroll Update(Payroll payroll);
         IQueryable<Payroll> Query();
    }
}