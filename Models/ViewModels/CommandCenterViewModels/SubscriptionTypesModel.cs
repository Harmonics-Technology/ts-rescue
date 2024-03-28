namespace TimesheetBE.Models.ViewModels.CommandCenterViewModels
{
    public class SubscriptionTypesModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string recommendedFor { get; set; }
        public string features { get; set; }
        public double monthlyAmount { get; set; }
        public double? monthlyDiscount { get; set; }
        public double yearlyAmount { get; set; }
        public double yearlyDiscount { get; set; }
        public double totalMonthlyAmount { get; set; }
        public double totalYearlyAmount { get; set; }
        public bool hasFreeTrial { get; set; }
        public int? freeTrialDuration { get; set; }
        public string discountType { get; set; }
    }
}
