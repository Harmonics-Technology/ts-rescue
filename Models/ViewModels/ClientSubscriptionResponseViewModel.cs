using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class ClientSubscriptionResponseViewModel
    {
        public Data data { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

    public class Subscription
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string recommendedFor { get; set; }
        public string features { get; set; }
        public double monthlyAmount { get; set; }
        public double? monthlyDiscount { get; set; }
        public double yearlyAmount { get; set; }
        public double? yearlyDiscount { get; set; }
        public double totalMonthlyAmount { get; set; }
        public double totalYearlyAmount { get; set; }
        public bool hasFreeTrial { get; set; }
        public object freeTrialDuration { get; set; }
        public object discountType { get; set; }
    }

    public class Data
    {
        public string id { get; set; }
        public string clientId { get; set; }
        //public object client { get; set; }
        public object freeTrialStartDate { get; set; }
        public DateTime startDate { get; set; }
        public int duration { get; set; }
        public string subscriptionId { get; set; }
        public Subscription subscription { get; set; }
        public bool annualBilling { get; set; }
        public string status { get; set; }
        public DateTime endDate { get; set; }
        public double totalAmount { get; set; }
    }
}
