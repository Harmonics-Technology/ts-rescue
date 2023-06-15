using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using TimesheetBE.Models.IdentityModels;

namespace TimesheetBE.Models.AppModels
{
    public class EmployeeInformation : BaseModel
    {
        public Guid UserId { get; set; }
        //[ForeignKey("UserId")]
        public User User { get; set; }
        public int PayRollTypeId { get; set; }
        public Guid? ClientId { get; set; }
        public User Client { get; set; }
        public Guid? SupervisorId { get; set; }
        public User Supervisor { get; set; }
        //public Guid? SuperAdminId { get; set; }
        //public User SuperAdmin { get; set; }
        public PayRollType PayrollType { get; set; }
        public double RatePerHour { get; set; }
        public string JobTitle { get; set; }
        public int HoursPerDay { get; set; }
        public string InCorporationDocumentUrl { get; set; }
        public string VoidCheckUrl { get; set; }
        public string InsuranceDocumentUrl { get; set; }
        public int HstNumber { get; set; }
        public Guid? PaymentPartnerId { get; set; }
        public User PaymentPartner { get; set; }
        public string PaymentRate { get; set; }
        public string Currency { get; set; }
        public bool FixedAmount { get; set; }
        public double? ClientRate { get; set; }
        public double? MonthlyPayoutRate { get; set; }
        public string PaymentFrequency { get; set; }
        public double OnBoradingFee { get; set; }
        public int? PayrollGroupId { get; set; }
        public PayrollGroup PayrollGroup { get; set; }
        public DateTime TimeSheetGenerationStartDate { get; set; }
        public bool? IsEligibleForLeave { get; set; }
        public int? NumberOfDaysEligible { get; set; }
        public int? NumberOfHoursEligible { get; set; }
        public int NumberOfEligibleLeaveDaysTaken { get; set; }
        public string? EmployeeType { get; set; }
        public string? InvoiceGenerationType { get; set; }
        public ICollection<Contract> Contracts { get; set; }
        

    }
}