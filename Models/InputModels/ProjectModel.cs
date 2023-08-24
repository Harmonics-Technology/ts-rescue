using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.InputModels
{
    public class ProjectModel
    {
        public Guid? Id { get; set; }
        public Guid SuperAdminId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public decimal Budget { get; set; }
        public List<Guid> Assignees { get; set; }
        public string Note { get; set; }
        public string? DocumentURL { get; set; }
    }
}
