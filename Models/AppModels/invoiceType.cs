namespace TimesheetBE.Models.AppModels
{
    public class InvoiceType
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    enum InvoiceTypes {
        EXPENSE = 1,
        PAYROLL = 2,
        PAYMENT_PARTNER = 3,
        CLIENT = 4,
    }
}