using System;

namespace TimesheetBE.Models.InputModels
{
    public class TeamMemberModel : RegisterModel
    {
        public Guid Id { get; set; }
        public int PayRollTypeId { get; set; }
        public int? PayrollGroupId { get; set; }
        public Guid? SupervisorId { get; set; }
        public Guid? ClientId { get; set; }
        public double RatePerHour { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string JobTitle { get; set; }
        public int HoursPerDay { get; set; }
        public string InCorporationDocumentUrl { get; set; }
        public string VoidCheckUrl { get; set; }
        public string InsuranceDocumentUrl { get; set; }
        public int HstNumber { get; set; }
        public Guid? PaymentPartnerId { get; set; }
        public string PaymentRate { get; set; }
        public string Currency { get; set; }
        public bool FixedAmount { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Document { get; set; }
        public double? ClientRate { get; set; }
        public double? MonthlyPayoutRate { get; set; }
        public string PaymentFrequency { get; set; }
        public bool IsActive { get; set; }
        public double onBordingFee { get; set; }
    }
}