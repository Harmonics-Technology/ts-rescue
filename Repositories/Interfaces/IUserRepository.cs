using System;
using System.Linq;
using System.Threading.Tasks;
using TimesheetBE.Models.IdentityModels;

namespace TimesheetBE.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<(bool Succeeded, string ErrorMessage, User LoggedInUser)> Authenticate(User UserToLogin,bool skipPasswordCheck = false);
        Task<(bool Succeeded, string ErrorMessage, User CreatedUser)> CreateUser(User newUser);
        public Task<(bool Succeeded, IQueryable<User> Users)> ListUsers();
        User LoggedInUser();
        Task<(bool Suceeded, string ErrorMessage)> DeleteUser(User user);
        IQueryable<User> Query();
    }
}

