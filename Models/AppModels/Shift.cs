using System;
using TimesheetBE.Models.IdentityModels;

namespace TimesheetBE.Models.AppModels
{
    public class Shift : BaseModel
    {
        public Guid UserId { get; set; }
        public User User { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int Hours { get; set; }
        public string? Title { get; set; }
        public string? Color { get; set; }
        public string? RepeatQuery { get; set; }
        public string? Note { get; set; }
        public bool IsPublished { get; set; }
        public Guid? ShiftToSwapId { get; set; }
        public Shift ShiftToSwap { get; set; }
        public Guid? ShiftSwappedId { get; set; }
        public Shift ShiftSwapped { get; set; }
        public bool IsSwapped { get; set; }
        public int? SwapStatusId { get; set; }
        public Status SwapStatus { get; set; }
    }
}
