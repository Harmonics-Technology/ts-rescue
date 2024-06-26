﻿using System;

namespace TimesheetBE.Models.ViewModels
{
    public class StrippedUserView
    {
        public Guid Id { get; set; }
        public Guid? EmployeeInformationId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Token { get; set; }
        public string Email { get; set; }
        public string ProfilePicture { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public string Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Guid? ClientId { get; set; }
        public string PhoneNumber { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationEmail { get; set; }
        public string OrganizationPhone { get; set; }
        public string OrganizationAddress { get; set; }
        public string ClientName { get; set; }
        public string PayrollType { get; set; }
        public string PayrollGroup { get; set; }
    }
}
