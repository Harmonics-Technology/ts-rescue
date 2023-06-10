using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class ClientSubscriptionResponseViewModel
    {
        public Data data { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class AddOn
    {
        public string addOnSubscriptionId { get; set; }
        public AddOnSubscription addOnSubscription { get; set; }
        public string clientSubscriptionId { get; set; }
        public decimal addOnTotalAmount { get; set; }
    }

    public class AddOnSubscription
    {
        public string id { get; set; }
        public int subscriptionTypeId { get; set; }
        public object subscriptionType { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string recommendedFor { get; set; }
        public string features { get; set; }
        public decimal monthlyAmount { get; set; }
        public decimal? monthlyDiscount { get; set; }
        public decimal yearlyAmount { get; set; }
        public decimal? yearlyDiscount { get; set; }
        public decimal totalMonthlyAmount { get; set; }
        public decimal totalYearlyAmount { get; set; }
        public decimal? addonAmount { get; set; }
        public bool hasFreeTrial { get; set; }
        public object freeTrialDuration { get; set; }
        public string discountType { get; set; }
    }

    public class BaseSubscription
    {
        public string id { get; set; }
        public int subscriptionTypeId { get; set; }
        public object subscriptionType { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string recommendedFor { get; set; }
        public string features { get; set; }
        public decimal monthlyAmount { get; set; }
        public decimal monthlyDiscount { get; set; }
        public decimal yearlyAmount { get; set; }
        public decimal yearlyDiscount { get; set; }
        public decimal totalMonthlyAmount { get; set; }
        public decimal totalYearlyAmount { get; set; }
        public object addonAmount { get; set; }
        public bool hasFreeTrial { get; set; }
        public int freeTrialDuration { get; set; }
        public string discountType { get; set; }
    }

    public class Data
    {
        public string id { get; set; }
        public string clientId { get; set; }
        public object client { get; set; }
        public object freeTrialStartDate { get; set; }
        public DateTime startDate { get; set; }
        public int duration { get; set; }
        public string baseSubscriptionId { get; set; }
        public BaseSubscription baseSubscription { get; set; }
        public bool annualBilling { get; set; }
        public string status { get; set; }
        public DateTime endDate { get; set; }
        public decimal totalAmount { get; set; }
        public List<AddOn> addOns { get; set; }
    }
}
