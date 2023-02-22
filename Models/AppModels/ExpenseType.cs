using System;

namespace TimesheetBE.Models.AppModels
{
    public class ExpenseType : BaseModel
    {
        public string Name { get; set;}
        public int StatusId { get; set; }
        public Status Status { get; set; }
    }
}