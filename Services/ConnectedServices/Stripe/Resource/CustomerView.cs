namespace TimesheetBE.Services.ConnectedServices.Stripe.Resource
{
    public class CustomerView
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string DefaultSource { get; set; }
    }
}
