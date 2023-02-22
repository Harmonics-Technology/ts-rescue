using System;

namespace TimesheetBE.Models.InputModels
{
    public class ExpenseTypeView
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }

    }
}