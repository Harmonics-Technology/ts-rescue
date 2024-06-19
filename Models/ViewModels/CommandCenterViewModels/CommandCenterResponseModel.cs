using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels.CommandCenterViewModels
{
    public class CommandCenterResponseModel<T>
    {
        public object href { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
        public List<T> data { get; set; }
        public int statusCode { get; set; }
        public object errors { get; set; }
        public object self { get; set; }
    }
}
