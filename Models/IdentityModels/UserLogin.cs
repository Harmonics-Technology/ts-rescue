using System;
using Microsoft.AspNetCore.Identity;

namespace TimesheetBE.Models.IdentityModels
{
    public class UserLogin : IdentityUserLogin<Guid>
    {
        public UserLogin()
        {
        }
    }
}

