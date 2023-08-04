using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class SubscriptionClientDetail
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

    public class SubscriptionData
    {
        public int offset { get; set; }
        public int limit { get; set; }
        public int size { get; set; }
        public SubscriptionFirst first { get; set; }
        public SubscriptionSelf self { get; set; }
        public List<SubscriptionValue> value { get; set; }
    }

    public class SubscriptionFirst
    {
        public string href { get; set; }
        public List<string> rel { get; set; }
    }

    public class SubscriptionHistoryViewModel
    {
        public object href { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
        public SubscriptionData data { get; set; }
        public int statusCode { get; set; }
        public object errors { get; set; }
        public object self { get; set; }
    }

    public class SubscriptionSelf
    {
        public string href { get; set; }
        public List<string> rel { get; set; }
    }

    public class SubscriptionDetail
    {
        public string id { get; set; }
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
        public bool hasFreeTrial { get; set; }
        public DateTime? freeTrialDuration { get; set; }
        public string? discountType { get; set; }
    }

    public class SubscriptionValue
    {
        public string id { get; set; }
        public string clientId { get; set; }
        public SubscriptionClientDetail client { get; set; }
        public DateTime? freeTrialStartDate { get; set; }
        public DateTime startDate { get; set; }
        public int duration { get; set; }
        public string subscriptionId { get; set; }
        public SubscriptionDetail subscription { get; set; }
        public bool annualBilling { get; set; }
        public string status { get; set; }
        public DateTime endDate { get; set; }
        public decimal totalAmount { get; set; }
        public bool isCanceled { get; set; }
        public string cancelationReason { get; set; }
    }


}
