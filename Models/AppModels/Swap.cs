using System;
using System.Collections.Generic;
using TimesheetBE.Models.IdentityModels;

namespace TimesheetBE.Models.AppModels
{
    public class Swap : BaseModel
    {
        public Guid SwapperId { get; set; }
        public User Swapper { get; set; }
        public Guid SwapeeId { get; set; }
        public User Swapee { get; set; }
        public Guid ShiftId { get; set; }
        public Shift Shift { get; set; }
        public Guid ShiftToSwapId { get; set; }
        public Shift ShiftToSwap { get; set; }
        public int StatusId { get; set; }
        public Status Status { get; set; }
        public bool IsApproved { get; set; }
        //public ICollection<Shift> Shifts { get; set; }
    }
}
