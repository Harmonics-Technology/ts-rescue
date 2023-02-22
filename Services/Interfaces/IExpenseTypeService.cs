using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Utilities;

namespace TimesheetBE.Services.Interfaces
{
    public interface IExpenseTypeService
    {
         Task<StandardResponse<ExpenseTypeView>> CreateExpenseType(string name);
         Task<StandardResponse<IEnumerable<ExpenseTypeView>>> ListExpenseTypes();
         Task<StandardResponse<ExpenseTypeView>> ToggleStatus(Guid expenseTypeId);
    }
}