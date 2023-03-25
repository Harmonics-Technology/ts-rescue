using TimesheetBE.Models.AppModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimesheetBE.Models.ViewModels;

namespace TimesheetBE.Models.IdentityModels
{
    public class User : IdentityUser<Guid>
    {
        public User()
        {
        }

        [Required]
        [MaxLength(60)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(60)]
        public string LastName { get; set; }
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
        public string OtherNames { get; set; }
        public DateTime DateOfBirth { get; set; }
        [NotMapped]
        public string Password { get; set; }
        [NotMapped]
        public string Token { get; set; }
        public string ProfilePicture { get; set; }
        public string Role { get; set; }
        public string Address { get; set; }
        public string  OrganizationName { get; set; }
        public string OrganizationEmail { get; set; }
        public string OrganizationPhone { get; set; }
        public string OrganizationAddress { get; set; }
        public Guid? ClientId { get; set; }
        public User Client { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool IsActive { get; set; }
        public Guid? EmployeeInformationId { get; set;}
        public EmployeeInformation EmployeeInformation { get; set; }
        public string InvoiceGenerationFrequency { get; set; }
        public int? Term { get; set; }
        public Guid? CreatedById { get; set; }
        public User CreatedBy { get; set; }
        public Guid? TwoFactorCode { get; set; }
        public ICollection<User> Supervisors { get; set; }
        public ICollection<EmployeeInformation> Supervisees { get; set; }
        public ICollection<EmployeeInformation> TeamMembers { get; set; }
        public ICollection<EmployeeInformation> Payees { get; set; }
    }
}

