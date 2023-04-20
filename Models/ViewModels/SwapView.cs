using System;

namespace TimesheetBE.Models.ViewModels
{
    public class SwapView
    {
        public Guid SwapperId { get; set; }
        public UserView Swapper { get; set; }
        public Guid SwapeeId { get; set; }
        public UserView Swapee { get; set; }
        public Guid ShiftId { get; set; }
        public ShiftView Shift { get; set; }
        public Guid ShiftToSwapId { get; set; }
        public ShiftView ShiftToSwap { get; set; }
        public int StatusId { get; set; }
        public string Status { get; set; }
        public bool IsApproved { get; set; }
    }
}
