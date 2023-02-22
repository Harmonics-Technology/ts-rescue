using System.Linq;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IInvoiceRepository
    {
         Invoice CreateAndReturn(Invoice invoice);
         Invoice Update(Invoice invoice);
         IQueryable<Invoice> Query();
    }
}