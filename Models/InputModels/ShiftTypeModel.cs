using System;

namespace TimesheetBE.Models.InputModels
{
    public class ShiftTypeModel
    {
        public Guid? Id { get; set; }
        public Guid? superAdminId { get; set; }
        public string Name { get; set; }
        public double Duration { get; set; }
        public string Color { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
    }
}
