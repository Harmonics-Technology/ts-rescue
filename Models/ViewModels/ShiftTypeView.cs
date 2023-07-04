using System;

namespace TimesheetBE.Models.ViewModels
{
    public class ShiftTypeView
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }
        public string Color { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
    }
}
