using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.InputModels
{
    public class ClientInvoiceModel
    {
        public List<Guid> InvoiceIds { get; set; }
        public Guid CLientId { get; set; }
    }
}
