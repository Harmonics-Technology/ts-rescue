using System;
using System.ComponentModel.DataAnnotations;

namespace TimesheetBE.Models.ViewModels
{
    public class UserDraftView
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Guid? ClientId { get; set; }
        [Required]
        public string Role { get; set; }
        public string? PhoneNumber { get; set; }
        public string? OrganizationName { get; set; }
        public string? OrganizationEmail { get; set; }
        public string? OrganizationPhone { get; set; }
        public string? OrganizationAddress { get; set; }
        public string? InvoiceGenerationFrequency { get; set; }
        public int? Term { get; set; }
        public Guid? ClientSubscriptionId { get; set; }
        public Guid? CommandCenterClientId { get; set; }
        public Guid? SuperAdminId { get; set; }
        public int? PayRollTypeId { get; set; }
        public int? PayrollGroupId { get; set; }
        public Guid? SupervisorId { get; set; }
        public double? RatePerHour { get; set; }
        public string? JobTitle { get; set; }
        public int? HoursPerDay { get; set; }
        public string? InCorporationDocumentUrl { get; set; }
        public string? VoidCheckUrl { get; set; }
        public string? InsuranceDocumentUrl { get; set; }
        public int? HstNumber { get; set; }
        public Guid? PaymentPartnerId { get; set; }
        public string? PaymentRate { get; set; }
        public string? Currency { get; set; }
        public bool? FixedAmount { get; set; }
        public string? Title { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Document { get; set; }
        public double? ClientRate { get; set; }
        public double? MonthlyPayoutRate { get; set; }
        public string? PaymentFrequency { get; set; }
        public bool? IsActive { get; set; }
        public double? OnBoradingFee { get; set; }
        public bool? IsEligibleForLeave { get; set; }
        public int? NumberOfDaysEligible { get; set; }
        public int? NumberOfHoursEligible { get; set; }
        public string? EmployeeType { get; set; }
        public string? InvoiceGenerationType { get; set; }
        public bool? EnableFinancials { get; set; }
    }
}
