using System;

namespace TimesheetBE.Models.InputModels
{
    public class UpdateUserModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationEmail { get; set; }
        public string OrganizationPhone { get; set; }
        public string OrganizationAddress { get; set; }
        public string ProfilePicture { get; set; }
        public string InvoiceGenerationFrequency { get; set; }
        public int? Term { get; set; }
    }
}