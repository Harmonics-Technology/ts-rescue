using System;
using System.Threading.Tasks;
using TimesheetBE.Models.AppModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Utilities;

namespace TimesheetBE.Services.Interfaces
{
    public interface IContractService
    {
        Task<StandardResponse<ContractView>> CreateContract(ContractModel model);
        Task<StandardResponse<ContractView>> GetContract(Guid id);
        Task<StandardResponse<ContractView>> UpdateContract(ContractModel model);
        Task<StandardResponse<ContractView>> TerminateContract(Guid id);
        Task<StandardResponse<PagedCollection<ContractView>>> ListContracts(PagingOptions options, Guid superAdminId, string search = null, DateFilter dateFilter = null);
        Task<StandardResponse<ContractView>> GetTeamMemberContract(Guid employeeInformationId, DateFilter dateFilter = null);
        Contract GetCurrentContract(Guid employeeInformationId);

    }
}