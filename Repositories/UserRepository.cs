using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Repositories.Interfaces;
using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Constants;
using TimesheetBE.Utilities.Extentions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace TimesheetBE.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly Context.AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly Globals _appSettings;
        public readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(Context.AppDbContext context, UserManager<User> userManager, SignInManager<User> signInManager, IOptions<Globals> appSettings, IHttpContextAccessor httpContextAccessor, ILogger<UserRepository> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _appSettings = appSettings.Value;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<(bool Succeeded, string ErrorMessage, User CreatedUser)> CreateUser(User newUser)
        {
            try
            {
                var Password = newUser.Password;
                newUser.Password = "";
                newUser.UserName = newUser.Email;
                newUser.DateCreated = DateTime.Now;
                newUser.DateModified = DateTime.Now;
                IdentityResult Created = await _userManager.CreateAsync(newUser, Password);
                if (!Created.Succeeded)
                {
                    var FirstError = Created.Errors.FirstOrDefault()?.Description;
                    return (false, FirstError, null);
                }

                var CreatedUser = _userManager.FindByEmailAsync(newUser.Email).Result;
                return (true, null, CreatedUser);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return (false, null, null);
            }
        }
        public async Task<(bool Succeeded, IQueryable<User> Users)> ListUsers()
        {
            var users = _context.Users;
            return (true, users);
        }

        public async Task<(bool Suceeded, string ErrorMessage)> DeleteUser(User user)
        {
            try
            {
                var Result = await _userManager.DeleteAsync(user);
                if (!Result.Succeeded)
                    return (false, null);

                return (true, null);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return (false, null);
            }
        }

        public User LoggedInUser()
        {
            int Id = _httpContextAccessor.HttpContext.User.GetLoggedInUserId<int>();
            if (Id <= 0)
                return null;

            User User = _userManager.FindByIdAsync(Id.ToString()).Result;
            return User;
        }

        public async Task<(bool Succeeded, string ErrorMessage, User LoggedInUser)> Authenticate(User UserToLogin)
        {
            try
            {
                var result =
                    await _signInManager.PasswordSignInAsync(UserToLogin.UserName, UserToLogin.Password, false, false);

                if (!result.Succeeded && !result.RequiresTwoFactor)
                {
                    return (false, Constants.LOGIN_CREDENTIALS_INCORRECT, null);
                }

                User user = await _userManager.FindByEmailAsync(UserToLogin.Email);
                var TokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.GivenName, $"{user.FirstName} {user.LastName}"),
                };

                var roles = await _userManager.GetRolesAsync(user);

                AddRolesToClaims(claims, roles);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = TokenHandler.CreateToken(tokenDescriptor);
                user.Token = TokenHandler.WriteToken(token);
                return (true, null, user);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return (false, null, null);
            }
        }

        private void AddRolesToClaims(List<Claim> claims, IEnumerable<string> roles)
        {
            foreach (var role in roles)
            {
                var roleClaim = new Claim(ClaimTypes.Role, role);
                claims.Add(roleClaim);
            }
        }

        public IQueryable<User> Query()
        {
            return _context.Set<User>();
        }


    }
}

