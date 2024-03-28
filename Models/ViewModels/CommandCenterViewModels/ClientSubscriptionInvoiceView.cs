using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels.CommandCenterViewModels
{
    public class ClientSubscriptionInvoiceViewData
    {
        public int offset { get; set; }
        public int limit { get; set; }
        public int size { get; set; }
        public ClientSubscriptionInvoiceViewFirst first { get; set; }
        public ClientSubscriptionInvoiceViewSelf self { get; set; }
        public List<ClientSubscriptionInvoiceViewValue> value { get; set; }
    }

    public class ClientSubscriptionInvoiceView
    {
        public object href { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
        public ClientSubscriptionInvoiceViewData data { get; set; }
        public int statusCode { get; set; }
        public object errors { get; set; }
        public object self { get; set; }
    }

    public class ClientSubscriptionInvoiceViewFirst
    {
        public string href { get; set; }
        public List<string> rel { get; set; }
    }

    public class ClientSubscriptionInvoiceViewSelf
    {
        public string href { get; set; }
        public List<string> rel { get; set; }
    }

    public class ClientSubscriptionInvoiceViewValue
    {
        public string id { get; set; }
        public string clientId { get; set; }
        public string billingAccount { get; set; }
        public string clientSubscriptionId { get; set; }
        public string invoiceReference { get; set; }
        public string licenceType { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string status { get; set; }
        public string paymentStatus { get; set; }
        public double amountInCent { get; set; }
        public string invoicePDFURL { get; set; }
        public string hostedInvoiceURL { get; set; }
    }
}
