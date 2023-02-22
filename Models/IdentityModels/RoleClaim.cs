using System;
using Microsoft.AspNetCore.Identity;

namespace TimesheetBE.Models.IdentityModels
{
    public class RoleClaim : IdentityRoleClaim<Guid>
    {
        public RoleClaim()
        {
        }
    }
}

