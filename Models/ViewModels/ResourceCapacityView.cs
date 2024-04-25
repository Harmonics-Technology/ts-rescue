namespace TimesheetBE.Models.ViewModels
{
    public class ResourceCapacityView
    {
        public string Name { get; set; }
        public string JobTitle { get; set; }
        public int TotalNumberOfTask { get; set; }
        public int TotalNumberOfProject { get; set; }
        public int NoOfTaskCompleted { get; set; }
    }
}
