using System;
using System.Linq;
using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Models.SeederModels
{
    public class PayrollTypeSeeder
    {
        private readonly AppDbContext _context;

        public PayrollTypeSeeder(AppDbContext context)
        {
            _context = context;
        }

        public void SeedData()
        {
            foreach (int app in Enum.GetValues(typeof(PayrollTypes)))
            {
                if (!_context.PayRollTypes.Any(sp => sp.Name == Enum.GetName(typeof(PayrollTypes), app)))
                {

                    var status = new PayRollType
                    {
                        Name = Enum.GetName(typeof(PayrollTypes), app),
                        // Description = Enum.GetName(typeof(Statuses), app)
                    };
                    _context.PayRollTypes.Add(status);
                }
            }
            _context.SaveChanges();
        }
    }
}