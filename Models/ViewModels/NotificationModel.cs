using System;

namespace TimesheetBE.Models.ViewModels
{
    public class NotificationModel
    {
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
    }
}
