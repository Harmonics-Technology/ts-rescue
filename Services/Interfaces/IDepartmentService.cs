using System.Threading.Tasks;
using System;
using TimesheetBE.Utilities;
using System.Collections.Generic;
using TimesheetBE.Models.ViewModels;

namespace TimesheetBE.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<StandardResponse<bool>> CreateDepartment(Guid superAdminId, string name);
        Task<StandardResponse<List<DepartmentView>>> ListDepartments(Guid SuperAdminId);
        Task<StandardResponse<bool>> DeleteDepartment(Guid departmentId);
    }
}
