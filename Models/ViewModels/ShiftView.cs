using System;

namespace TimesheetBE.Models.ViewModels
{
    public class ShiftView
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public UserView User { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int? Hours { get; set; }
        public string? Title { get; set; }
        public string? Color { get; set; }
        public string? RepeatQuery { get; set; }
        public string? Note { get; set; }
        public bool IsPublished { get; set; }
        //public Guid? SwapId { get; set; }
        //public Guid? ShiftToSwapId { get; set; }
        //public ShiftToSwapView ShiftToSwap { get; set; }
        //public bool IsSwapped { get; set; }
        //public int? SwapStatusId { get; set; }
        //public string? SwapStatus { get; set; }
    }
}
