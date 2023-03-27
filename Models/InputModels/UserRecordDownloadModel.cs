using System;

namespace TimesheetBE.Models.InputModels
{
    public class UserRecordDownloadModel
    {
        public RecordsToDownload Record { get; set; }
        public Guid? SupervisorId { get; set; }
        public Guid? ClientId { get; set; }
        public Guid? PaymentPartnerId { get; set; }
    }

    public enum RecordsToDownload
    {
        AdminUsers = 1,
        TeamMembers,
        Supervisors,
        Client,
        PaymentPartner,
        PayrollManagers,
        Admin,
        ClientSupervisors,
        Supervisees,
        ClientTeamMembers,
        PaymentPartnerTeamMembers

    }
}
