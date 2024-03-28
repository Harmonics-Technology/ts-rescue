using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class CountryView
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Currency { get; set; }
        public string Flag { get; set; }
    }

    public class CountryAPIResponse
    {
        public bool error { get; set; }
        public string msg { get; set; }
        public List<CountryData> data { get; set; }
    }

    public class CountryData
    {
        public string name { get; set; }
        public string currency { get; set; }
        public string flag { get; set; }
    }

}
