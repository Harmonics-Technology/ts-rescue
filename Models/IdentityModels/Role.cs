using System;
using Microsoft.AspNetCore.Identity;

namespace TimesheetBE.Models.IdentityModels
{
    public class Role : IdentityRole<Guid>
    {
        public Role()
        {
        }
    }
}

