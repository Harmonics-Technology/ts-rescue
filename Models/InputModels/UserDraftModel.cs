using System;

namespace TimesheetBE.Models.InputModels
{
    public class UserDraftModel
    {
        public Guid? Id { get; set; }
        public Guid SuperAdminId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public Guid? TeammemberId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? OrganizationName { get; set; }
        public string? ContactFirstName { get; set; }
        public string? ContactLastName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhoneNumber { get; set; }
        public string? Frequency { get; set; }
        public int? Term { get; set; }
        public Guid? ClientId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool? ProfileStatus { get; set; }
        public string? JobTitle { get; set; }
        public Guid? SupervisorId { get; set; }
        public bool? EnableFinancials { get; set; }
        public int? HoursPerDay { get; set; }
        public string? EmployeeType { get; set; }
        public string? ContractTitle { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Document { get; set; }
        public bool? IsEligibleForLeave { get; set; }
        public int? NumberOfDaysEligible { get; set; }
        public int? NumberOfHoursEligible { get; set; }
    }
}
