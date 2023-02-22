﻿using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class DashboardClientView
    {
        public int ApprovedTimeSheet { get; set; }
        public int AwaitingTimeSheet { get; set; }
        public int RejectedTimeSheet { get; set; }
        public List<RecentTimeSheetView> RecentTimeSheet { get; set; }
        public List<RecentInvoiceView> RecentInvoice { get; set; }
    }
}
