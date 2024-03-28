using System;
using System.Collections.Generic;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.IdentityModels;

namespace TimesheetBE.Models.ViewModels
{
    public class UserView
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
        public EmployeeInformationView EmployeeInformation { get; set; }
        public Guid? ClientId { get; set; }
        public UserView Client { get; set; }
        public Guid? SuperAdminId { get; set; }
        public string PhoneNumber { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationEmail { get; set; }
        public string OrganizationPhone { get; set; }
        public string OrganizationAddress { get; set; }
        public string ClientName { get; set; }
        public string PayrollType { get; set; }
        public string PayrollGroup { get; set; }
        public int PayrollGroupId { get; set; }
        public string InvoiceGenerationFrequency { get; set; }
        public int? Term { get; set; }
        public Guid TwoFactorCode { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public int? NumberOfDaysEligible { get; set; }
        public int? NumberOfLeaveDaysTaken { get; set; }
        public int? NumberOfHoursEligible { get; set; }
        public string? EmployeeType { get; set; }
        public double? HoursPerDay { get; set; }
        public string? InvoiceGenerationType { get; set; }
        public Guid? ClientSubscriptionId { get; set; }
        public bool? IsOrganizationProjectManager { get; set; }
        public ControlSettingView ControlSettingView { get; set; }
        public ClientSubscriptionResponseViewModel SubscriptiobDetails { get; set; }
        public string Currency { get; set; }
        public bool IsBirthDayToday { get; set; }
        public bool IsAnniversaryToday { get; set; }
        public DateTime? ContractStartDate { get; set; }
    }
}