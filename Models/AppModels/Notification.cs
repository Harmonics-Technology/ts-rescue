using System;
using TimesheetBE.Models.IdentityModels;

namespace TimesheetBE.Models.AppModels
{
    public class Notification : BaseModel
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set;}
        public bool IsRead { get; set; }
    }
}