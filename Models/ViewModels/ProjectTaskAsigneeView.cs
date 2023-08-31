using System;
using TimesheetBE.Models.IdentityModels;

namespace TimesheetBE.Models.ViewModels
{
    public class ProjectTaskAsigneeView
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public UserView User { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? TaskId { get; set; }
        public double HoursLogged { get; set; }
    }
}
