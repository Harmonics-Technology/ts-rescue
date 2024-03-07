using System;

namespace TimesheetBE.Models.ViewModels
{
    public class DepartmentView
    {
        public int Id { get; set; }
        public Guid SuperAdminId { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
