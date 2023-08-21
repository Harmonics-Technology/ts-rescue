namespace TimesheetBE.Controllers
{
    public class CommandCenterResponseModel
    {
        public object href { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
        public CommandCenterAddCardResponse data { get; set; }
        public int statusCode { get; set; }
        public object errors { get; set; }
        public object self { get; set; }
    }

    public class CommandCenterAddCardResponse
    {
        public string clientSecret { get; set; }
        public string subscriptionId { get; set; }
    }
}
