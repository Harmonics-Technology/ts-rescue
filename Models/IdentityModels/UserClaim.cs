using System;
using Microsoft.AspNetCore.Identity;

namespace TimesheetBE.Models.IdentityModels
{
    public class UserClaim : IdentityUserClaim<Guid>
    {
        public UserClaim()
        {
        }
    }
}

