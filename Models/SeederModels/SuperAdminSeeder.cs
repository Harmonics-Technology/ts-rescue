using Microsoft.AspNetCore.Identity;
using System.Linq;
using TimesheetBE.Context;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Services.Abstractions;

namespace TimesheetBE.Models.SeederModels
{
    public class SuperAdminSeeder
    {
        private readonly AppDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        public readonly RoleManager<Role> _roleManager;
        private readonly IUserService _userService;
        public SuperAdminSeeder(AppDbContext context, IUserRepository userRepository, UserManager<User> userManager, RoleManager<Role> roleManager , IUserService userService)
        {
            _context = context;
            _userRepository = userRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _userService = userService;
        }

        public void SeedData()
         {
            //if (!_userRepository.Query().Where(x => x.Email.ToLower() == "ade.adeyemi@proinsight.ca").Any())
            //{

            //    var roleExist = AscertainRoleExists("Super Admin");
            //    var adminUser = new RegisterModel
            //    {
            //        FirstName = "SuperAdmin",
            //        LastName = "SuperAdmin",
            //        Email = "ade.adeyemi@proinsight.ca",
            //        OrganizationEmail = "ade.adeyemi@proinsight.ca",
            //        OrganizationName = "Proinsight",
            //        Password = "#1234567@#",
            //        Role = "Super Admin"
            //    };

            //    var Result = _userService.CreateUser(adminUser).Result;
            //}

            if (!_userRepository.Query().Where(x => x.Email.ToLower() == "adelowomi@harmonicstechnology.com").Any())
            {

                var roleExist = AscertainRoleExists("Super Admin");
                var adminUser = new RegisterModel
                {
                    FirstName = "SuperAdmin",
                    LastName = "SuperAdmin",
                    Email = "adelowomi@harmonicstechnology.com",
                    OrganizationEmail = "adelowomi@harmonicstechnology.com",
                    OrganizationName = "Proinsight",
                    Password = "#1234567@#",
                    Role = "Super Admin"
                };

                var Result = _userService.CreateUser(adminUser).Result;
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
