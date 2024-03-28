using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface ICountryRepository
    {
        Country CreateAndReturn(Country model);
        Country Update(Country model);
        void Delete(Country model);
        IQueryable<Country> Query();
    }
}
