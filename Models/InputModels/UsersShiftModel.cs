using System;

namespace TimesheetBE.Models.InputModels
{
    public class UsersShiftModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? UserId { get; set; }
        public bool? IsPublished { get; set; }
    }
}
