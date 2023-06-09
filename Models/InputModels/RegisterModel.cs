using System;
using System.ComponentModel.DataAnnotations;

namespace TimesheetBE.Models.InputModels
{
    public class RegisterModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Guid? ClientId { get; set; }
        [Required]
        public string Role { get; set; }
        public string PhoneNumber { get; set; }
        public string  OrganizationName { get; set; }
        public string OrganizationEmail { get; set; }
        public string OrganizationPhone { get; set; }
        public string OrganizationAddress { get; set; }
        public string InvoiceGenerationFrequency { get; set; }
        public int? Term { get; set; }
        public Guid? ClientSubscriptionId { get; set; }
        public Guid? CommandCenterClientId { get; set; }
        public Guid? SuperAdminId { get; set; }
    }
}

