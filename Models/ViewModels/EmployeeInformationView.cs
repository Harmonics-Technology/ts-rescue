using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class EmployeeInformationView
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public StrippedUserView User { get; set; }
        public Guid? ClientId { get; set; }
        public UserView Client {get;set;}
        public Guid SupervisorId { get; set; }
        public UserView Supervisor { get; set; }
        public string PayrollType { get; set; }
        public string PayrollGroup { get; set; }
        public int PayrollGroupId { get; set; }
        public double RatePerHour { get; set; }
        public string JobTitle { get; set; }
        public int HoursPerDay { get; set; }
        public string InCorporationDocumentUrl { get; set; }
        public string VoidCheckUrl { get; set; }
        public string InsuranceDocumentUrl { get; set; }
        public int HstNumber { get; set; }
        public Guid? PaymentPartnerId { get; set; }
        public UserView PaymentPartner { get; set; }
        public string PaymentRate { get; set; }
        public string Currency { get; set; }
        public bool FixedAmount { get; set; }
        public double? ClientRate { get; set; }
        public double? MonthlyPayoutRate { get; set; }
        public string PaymentFrequency { get; set; }
        public double OnBoradingFee { get; set; }
        //public DateTime TimeSheetGenerationStartDate { get; set; }
        public bool? IsEligibleForLeave { get; set; }
        public int? NumberOfDaysEligible { get; set; }
        public int? NumberOfHoursEligible { get; set; }
        public int NumberOfLeaveDaysTaken { get; set; }
        public string? EmployeeType { get; set; }
        public string? InvoiceGenerationType { get; set; }
        public bool EnableFinancials { get; set; }
        public IEnumerable<ContractView> Contracts {get;set;}
        public DateTime DateCreated { get; set; }
        public string Department { get; set; }
        public string EmploymentContractType { get; set; }
        public string TimesheetFrequency { get; set; }
        public string PayrollStructure { get; set; }
        public double Rate { get; set; }
        public string? RateType { get; set; }
        public string TaxType { get; set; }
        public string? StandardCanadianSystem { get; set; }
        public double Tax { get; set; }
        public string PayrollProcessingType { get; set; }
        public bool NewPayrollStructureEnabled { get; set; }
    }
}