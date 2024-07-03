using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class SubscriptionHistoryView
    {
        public object href { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
        public Subscriptions data { get; set; }
        public int statusCode { get; set; }
        public object errors { get; set; }
        public object self { get; set; }
    }

    public class AddOnDetails
    {
        public string addOnSubscriptionId { get; set; }
        public AddOnSubscriptionDetails addOnSubscription { get; set; }
        public string clientSubscriptionId { get; set; }
        public int addOnTotalAmount { get; set; }
    }

    public class AddOnSubscriptionDetails
    {
        public string id { get; set; }
        public int subscriptionTypeId { get; set; }
        public SubscriptionType subscriptionType { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string recommendedFor { get; set; }
        public string features { get; set; }
        public int monthlyAmount { get; set; }
        public object monthlyDiscount { get; set; }
        public int yearlyAmount { get; set; }
        public object yearlyDiscount { get; set; }
        public int totalMonthlyAmount { get; set; }
        public int totalYearlyAmount { get; set; }
        public int addonAmount { get; set; }
        public bool hasFreeTrial { get; set; }
        public object freeTrialDuration { get; set; }
        public string discountType { get; set; }
    }

    public class BaseSubscriptionDetails
    {
        public string id { get; set; }
        public int subscriptionTypeId { get; set; }
        public SubscriptionType subscriptionType { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string recommendedFor { get; set; }
        public string features { get; set; }
        public int monthlyAmount { get; set; }
        public int monthlyDiscount { get; set; }
        public int yearlyAmount { get; set; }
        public int yearlyDiscount { get; set; }
        public int totalMonthlyAmount { get; set; }
        public int totalYearlyAmount { get; set; }
        public object addonAmount { get; set; }
        public bool hasFreeTrial { get; set; }
        public int freeTrialDuration { get; set; }
        public string discountType { get; set; }
    }

    public class Client
    {
        public string id { get; set; }
        public string companyName { get; set; }
        public string name { get; set; }
        public string companyEmail { get; set; }
        public string email { get; set; }
        public string companyPhoneNumber { get; set; }
        public string phoneNumber { get; set; }
        public string address { get; set; }
        public string companyAddress { get; set; }
        public DateTime startDate { get; set; }
        public int durationInMonths { get; set; }
        public string subscriptionId { get; set; }
        public object subscription { get; set; }
        public bool isAnnualBilling { get; set; }
        public object subscriptionStatus { get; set; }
        public DateTime dateCreated { get; set; }
    }

    public class Subscriptions
    {
        public int offset { get; set; }
        public int limit { get; set; }
        public int size { get; set; }
        public First first { get; set; }
        public Self self { get; set; }
        public List<Value> value { get; set; }
    }

    public class First
    {
        public string href { get; set; }
        public List<string> rel { get; set; }
    }

    public class Self
    {
        public string href { get; set; }
        public List<string> rel { get; set; }
    }

    public class SubscriptionType
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Value
    {
        public string id { get; set; }
        public string clientId { get; set; }
        public Client client { get; set; }
        public object freeTrialStartDate { get; set; }
        public DateTime startDate { get; set; }
        public int duration { get; set; }
        public string baseSubscriptionId { get; set; }
        public BaseSubscriptionDetails baseSubscription { get; set; }
        public bool annualBilling { get; set; }
        public string status { get; set; }
        public DateTime endDate { get; set; }
        public int totalAmount { get; set; }
        public List<AddOnDetails> addOns { get; set; }
    }


}
