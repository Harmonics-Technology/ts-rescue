using System;

namespace TimesheetBE.Models.ViewModels
{
    public class ShiftView
    {
        public Guid UserId { get; set; }
        public UserView User { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int? Hours { get; set; }
        public string? Title { get; set; }
        public string? Color { get; set; }
        public string? RepeatQuery { get; set; }
        public string? Note { get; set; }
    }
}
