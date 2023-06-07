using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.InputModels;

namespace TimesheetBE.Utilities.Abstrctions
{
    public interface IUtilityMethods
    {
        string RandomCode(int size);

        string FormattedDate(DateTime thisDate);

        string GetUniqueFileName(string fileName);
        string GetMonthName(int month);
        IQueryable<T> ApplyFilter<T> (IQueryable<T> query, FilterOptions options) where T : BaseModel;
    }
}
