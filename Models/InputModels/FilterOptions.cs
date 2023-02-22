using System;
using TimesheetBE.Models.AppModels;

namespace TimesheetBE.Models.InputModels
{
    public class FilterOptions
    {
        public Statuses? Status { get; set; }
        public OrderType? Order { get; set; }
    }

    public enum OrderType { Ascending, Descending }
}