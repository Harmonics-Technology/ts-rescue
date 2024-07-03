using System;

namespace TimesheetBE.Models.ViewModels
{
    public class ContractView
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Document { get; set; }
        public string Status { get; set; }
        public int Tenor { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; set;}
        public string JobTitle { get; set; }
    }
}