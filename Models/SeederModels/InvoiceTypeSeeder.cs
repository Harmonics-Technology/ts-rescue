using System;
using System.Linq;
using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Models.SeederModels
{
    public class InvoiceTypeSeeder
    {
        private readonly AppDbContext _context;

        public InvoiceTypeSeeder(AppDbContext context)
        {
            _context = context;
        }

        public void SeedData()
        {
            foreach (int app in Enum.GetValues(typeof(InvoiceTypes)))
            {
                if (!_context.InvoiceTypes.Any(sp => sp.Name == Enum.GetName(typeof(InvoiceTypes), app)))
                {

                    var status = new InvoiceType
                    {
                        Name = Enum.GetName(typeof(InvoiceTypes), app),
                        // Description = Enum.GetName(typeof(Statuses), app)
                    };
                    _context.InvoiceTypes.Add(status);
                }
            }
            _context.SaveChanges();
        }
        
    }
}