using Microsoft.AspNetCore.Identity;
using TimesheetBE.Context;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Services.Abstractions;

namespace TimesheetBE.Models.SeederModels
{
    public class SeedData
    {
        private readonly AppDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        public readonly RoleManager<Role> _roleManager;
        private readonly IUserService _userService;

        public SeedData(AppDbContext context, IUserRepository userRepository, UserManager<User> userManager, RoleManager<Role> roleManager, IUserService userService)
        {
            _context = context;
            _userRepository = userRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _userService = userService;

        }

        public void SeedInitialData()
        {
            new StatusSeeder(_context).SeedData();
            new PayrollTypeSeeder(_context).SeedData();
            new InvoiceTypeSeeder(_context).SeedData();
            //new OnboardingFeeTypeSeeder(_context).SeedData();
            new PayrollGroupSeeder(_context).SeedData();
            new SuperAdminSeeder(_context, _userRepository, _userManager, _roleManager, _userService).SeedData();  
        }
    }
}