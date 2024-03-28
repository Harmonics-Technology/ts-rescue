namespace TimesheetBE.Services.ConnectedServices.Stripe.Resource
{
    public class CreateCardResource {
        public string Name { get; set; }
        public string Number { get; set; }
        public string ExpiryYear { get; set; }
        public string ExpiryMonth { get; set; }
        public string Cvc { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    };
}
