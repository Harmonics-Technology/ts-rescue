namespace TimesheetBE.Models.AppModels
{
    public class PayrollGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    enum PayrollGroups
    {
        PROINSIGHT = 1,
        OLADE = 2
    }
}
