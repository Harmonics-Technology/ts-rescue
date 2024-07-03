using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Utilities;

namespace TimesheetBE.Services.Interfaces
{
    public interface IExpenseTypeService
    {
         Task<StandardResponse<ExpenseTypeView>> CreateExpenseType(Guid superAdminId, string name);
         Task<StandardResponse<IEnumerable<ExpenseTypeView>>> ListExpenseTypes(Guid superAdminId);
         Task<StandardResponse<ExpenseTypeView>> ToggleStatus(Guid expenseTypeId);
    }
}