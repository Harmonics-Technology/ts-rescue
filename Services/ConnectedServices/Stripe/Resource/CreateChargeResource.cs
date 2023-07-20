namespace TimesheetBE.Services.ConnectedServices.Stripe.Resource
{
    public record CreateChargeResource(
    string Currency,
    long Amount,
    string CustomerId,
    string ReceiptEmail,
    string Description);
}
