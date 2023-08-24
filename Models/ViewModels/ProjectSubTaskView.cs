using System;

namespace TimesheetBE.Models.ViewModels
{
    public class ProjectSubTaskView
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public string Priority { get; set; }
        public string Note { get; set; }
    }
}
