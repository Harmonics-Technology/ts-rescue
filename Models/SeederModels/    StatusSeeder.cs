using System;
using System.Linq;
using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Models.SeederModels
{
    public class StatusSeeder
    {
        private readonly AppDbContext _context;

        public StatusSeeder(AppDbContext context)
        {
            _context = context;
        }

        public void SeedData()
        {
            foreach (int app in Enum.GetValues(typeof(Statuses)))
            {
                if (!_context.Statuses.Any(sp => sp.Name == Enum.GetName(typeof(Statuses), app)))
                {

                    var status = new Status
                    {
                        Name = Enum.GetName(typeof(Statuses), app),
                        // Description = Enum.GetName(typeof(Statuses), app)
                    };
                    _context.Statuses.Add(status);
                }
            }
            _context.SaveChanges();
        }
    }
}
