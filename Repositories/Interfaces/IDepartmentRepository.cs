using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IDepartmentRepository
    {
        Department CreateAndReturn(Department model);
        Department Update(Department model);
        void Delete(Department model);
        IQueryable<Department> Query();
    }
}
