using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class UserCardListView
    {
        public List<Datum> data { get; set; }
    }

    public class Datum
    {
        public string id { get; set; }
        public string customerId { get; set; }
        public string customerName { get; set; }
        public string customerEmail { get; set; }
        public string brand { get; set; }
        public string lastFourDigit { get; set; }
        public bool isDefaultCard { get; set; }
    }
}
