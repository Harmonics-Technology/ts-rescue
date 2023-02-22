using TimesheetBE.Context;

namespace TimesheetBE.Models.SeederModels
{
    public class SeedData
    {
        private readonly AppDbContext _context;

        public SeedData(AppDbContext context)
        {
            _context = context;
        }

        public void SeedInitialData()
        {
            new StatusSeeder(_context).SeedData();
            new PayrollTypeSeeder(_context).SeedData();
            new InvoiceTypeSeeder(_context).SeedData();
            new OnboardingFeeTypeSeeder(_context).SeedData();
            new PayrollGroupSeeder(_context).SeedData();
        }
    }
}