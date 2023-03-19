using System;
using System.Linq;
using TimesheetBE.Context;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Models.SeederModels
{
    public class OnboardingFeeTypeSeeder
    {
        private readonly AppDbContext _context;
        public OnboardingFeeTypeSeeder(AppDbContext context)
        {
            _context = context;
        }

        public void SeedData()
        {
            foreach (int app in Enum.GetValues(typeof(OnboradingFeeTypes)))
            {
                if (!_context.OnboardingFeeTypes.Any(sp => sp.Name == Enum.GetName(typeof(OnboradingFeeTypes), app)))
                {

                    var status = new OnboardingFeeType
                    {
                        Name = Enum.GetName(typeof(OnboradingFeeTypes), app),
                        // Description = Enum.GetName(typeof(Statuses), app)
                    };
                    _context.OnboardingFeeTypes.Add(status);
                }
            }
            _context.SaveChanges();
        }
    }
}
