namespace TimesheetBE.Services.ConnectedServices.Stripe.Resource
{
    public class CardView
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string Brand { get; set; }
        public string LastFourDigit { get; set; }
        public bool IsDefaultCard { get; set; }
    }
}
