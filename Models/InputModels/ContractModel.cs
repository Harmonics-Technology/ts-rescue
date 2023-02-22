using System;

namespace TimesheetBE.Models.InputModels
{
    public class ContractModel
    {
        public Guid? Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Document { get; set; }
    }
}