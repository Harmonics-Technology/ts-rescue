using System;

namespace TimesheetBE.Models.AppModels
{
    public class Department : BaseModel
    {
        public Guid SuperAdminId { get; set; }
        public string Name { get; set; }
    }
}
