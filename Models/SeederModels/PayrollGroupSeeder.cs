using System;
using System.Linq;
using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Models.SeederModels
{
    public class PayrollGroupSeeder
    {
        private readonly AppDbContext _context;
        public PayrollGroupSeeder(AppDbContext context)
        {
            _context = context;
        }

        public void SeedData()
        {
            foreach (int app in Enum.GetValues(typeof(PayrollGroups)))
            {
                if (!_context.PayrollGroups.Any(sp => sp.Name == Enum.GetName(typeof(PayrollGroups), app)))
                {

                    var status = new PayrollGroup
                    {
                        Name = Enum.GetName(typeof(PayrollGroups), app),
                        // Description = Enum.GetName(typeof(Statuses), app)
                    };
                    _context.PayrollGroups.Add(status);
                }
            }
            _context.SaveChanges();
        }
    }
}
