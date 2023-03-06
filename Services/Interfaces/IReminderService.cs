namespace TimesheetBE.Services.Interfaces
{
    public interface IReminderService
    {
         void SendFillTimesheetReminder();
         void SendApproveTimesheetReminder();
         void SendFillTimesheetReminderToTeamMember();
         void SendCutOffTimesheetReminderToTeamMember();
    }
}