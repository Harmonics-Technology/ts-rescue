using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IExpenseRepository
    {
         Expense CreateAndReturn(Expense expense);
         Expense Update(Expense expense);
         IQueryable<Expense> Query();
    }
}