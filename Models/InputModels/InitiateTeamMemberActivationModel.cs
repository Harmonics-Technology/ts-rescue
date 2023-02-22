using System.Collections.Generic;

namespace TimesheetBE.Models.InputModels
{
    public class InitiateTeamMemberActivationModel
    {
        public string Email { get; set; }
        public List<string> AdminEmails { get; set; }
    }
}
