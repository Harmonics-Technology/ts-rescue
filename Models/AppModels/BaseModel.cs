using System;
namespace TimesheetBE.Models.AppModels
{
    public class BaseModel
    {
        public BaseModel()
        {
        }

        public Guid Id { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime DateModified { get; set; } = DateTime.Now;
    }
}

