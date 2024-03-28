using System;

namespace TimesheetBE.Models.ViewModels
{
    public class ShiftTypeView
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Duration { get; set; }
        public string Color { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
    }
}
