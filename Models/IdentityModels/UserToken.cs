using System;
using Microsoft.AspNetCore.Identity;

namespace TimesheetBE.Models.IdentityModels
{
    public class UserToken : IdentityUserToken<Guid>
    {
        public UserToken()
        {
        }
    }
}

