using Microsoft.AspNetCore.Identity;
using System.Linq;
using TimesheetBE.Context;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Repositories.Interfaces;

namespace TimesheetBE.Models.SeederModels
{
    public class SuperAdminSeeder
    {
        private readonly AppDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        public readonly RoleManager<Role> _roleManager;
        public SuperAdminSeeder(AppDbContext context, IUserRepository userRepository, UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _context = context;
            _userRepository = userRepository;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void SeedData()
         {
            if (!_userRepository.Query().Where(x => x.Role.ToLower() == "super admin").Any())
            {

                var roleExist = AscertainRoleExists("Super Admin");
                var adminUser = new User
                {
                    FirstName = "SuperAdmin",
                    LastName = "SuperAdmin",
                    Email = "ade.adeyemi@proinsight.ca",
                    OrganizationEmail = "ade.adeyemi@proinsight.ca",
                    OrganizationName = "Proinsight",
                    IsActive = true,
                    Password = "#1234567@#",
                    Role = "Super Admin"
                };

                var Result = _userRepository.CreateUser(adminUser).Result;
                var result = _userManager.AddToRoleAsync(Result.CreatedUser, "Super Admin").Result.Succeeded;
            }
            _context.SaveChanges();
        }

        private bool AscertainRoleExists(string roleName)
        {
            var roleExists = _roleManager.RoleExistsAsync(roleName).Result;
            if (!roleExists)
            {
                var role = new Role()
                {
                    Name = roleName
                };
                var roleCreated = _roleManager.CreateAsync(role).Result;
                return roleExists;
            }
            return true;
        }
    }
}
