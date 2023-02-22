namespace TimesheetBE.Services.Interfaces
{
    public interface IReminderService
    {
         void SendFillTimesheetReminder();
         void SendApproveTimesheetReminder();
    }
}