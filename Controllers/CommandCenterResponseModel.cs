namespace TimesheetBE.Controllers
{
    public class CommandCenterResponseModel
    {
        public object href { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
        public object data { get; set; }
        public int statusCode { get; set; }
        public object errors { get; set; }
        public object self { get; set; }
    }
}
