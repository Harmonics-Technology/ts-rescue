namespace TimesheetBE.Models.AppModels
{
    public class PayRollType
    {
        public int Id { get; set; }
        public string Name { get; set;}
    }

    enum PayrollTypes
    {
        ONSHORE = 1,
        OFFSHORE
    }
}