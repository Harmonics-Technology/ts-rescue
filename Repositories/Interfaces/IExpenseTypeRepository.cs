using System;
using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IExpenseTypeRepository
    {
         ExpenseType CreateAndReturn(ExpenseType type);
         ExpenseType GetById(Guid id);
         IQueryable<ExpenseType> Query();
         ExpenseType Update(ExpenseType type);
    }
}