using System;

namespace TimesheetBE.Models.InputModels
{
    public class ShiftModel
    {
        public Guid UserId { get; set; }
        public Guid ShiftTypeId { get; set; }
        public DateTime Start { get; set; }
        //public DateTime End { get; set; }
        //public int Hours { get; set; }
        //public string? Title { get; set; }
        //public string? Color { get; set; }
        public string? RepeatQuery { get; set; }
        public string? Note { get; set; }
        public DateTime? RepeatStopDate { get; set; }
    }
}
