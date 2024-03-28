namespace TimesheetBE.Services.ConnectedServices.Stripe.Resource
{
    public class CreateChargeResource
    {
        public string Currency { get; set; }
        public long Amount { get; set; }
        public string CardId { get; set; }
        public string ReceiptEmail { get; set; }
        public string Description { get; set; }
    }
}
