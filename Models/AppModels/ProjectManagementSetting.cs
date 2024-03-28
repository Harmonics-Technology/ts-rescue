using System;

namespace TimesheetBE.Models.AppModels
{
    public class ProjectManagementSetting : BaseModel
    {
        public Guid SuperAdminId { get; set; }
        public bool AdminProjectCreation { get; set; }
        public bool PMProjectCreation { get; set; }
        public bool AllProjectCreation { get; set; }
        public bool AdminTaskCreation { get; set; }
        public bool AssignedPMTaskCreation { get; set; }
        public bool ProjectMembersTaskCreation { get; set;}
        public bool AdminTaskViewing { get; set; }
        public bool AssignedPMTaskViewing { get; set; }
        public bool ProjectMembersTaskViewing { get;set; }
        public bool PMTaskEditing { get; set; }
        public bool TaskMembersTaskEditing { get; set; }
        public bool ProjectMembersTaskEditing { get; set; }
        public bool ProjectMembersTimesheetVisibility { get; set; }
        public bool TaskMembersTimesheetVisibility { get; set; }
    }
}
