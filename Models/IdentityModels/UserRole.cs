using System;
using Microsoft.AspNetCore.Identity;

namespace TimesheetBE.Models.IdentityModels
{
    public class UserRole : IdentityUserRole<Guid>
    {
        public UserRole()
        {
        }
    }
}

