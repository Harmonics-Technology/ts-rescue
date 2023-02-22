using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;

namespace TimesheetBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Super Admin, Admin, Payroll Manager")]
    public class ContractController : StandardControllerResponse
    {
        private readonly IContractService _contractService;
        private readonly PagingOptions _defaultPagingOptions;

        public ContractController(IContractService contractService, IOptions<PagingOptions> defaultPagingOptions)
        {
            _contractService = contractService;
            _defaultPagingOptions = defaultPagingOptions.Value;
        }

        [HttpPost("create", Name = nameof(CreateContract))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<ContractView>>> CreateContract([FromBody] ContractModel model)
        {
            return Result(await _contractService.CreateContract(model));
        }

        [HttpGet("get/{id}", Name = nameof(GetContract))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<ContractView>>> GetContract([FromRoute] Guid id)
        {
            return Result(await _contractService.GetContract(id));
        }

        [HttpPut("update", Name = nameof(UpdateContract))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<ContractView>>> UpdateContract([FromBody] ContractModel model)
        {
            return Result(await _contractService.UpdateContract(model));
        }

        [HttpDelete("terminate/{id}", Name = nameof(TerminateContract))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<ContractView>>> TerminateContract([FromRoute] Guid id)
        {
            return Result(await _contractService.TerminateContract(id));
        }

        [HttpGet("list", Name = nameof(ListContracts))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ContractView>>>> ListContracts(
            [FromQuery] PagingOptions pagingOptions, [FromQuery] string search = null, [FromQuery] DateFilter dateFilter = null)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _contractService.ListContracts(pagingOptions, search, dateFilter));
        }

        [HttpGet("team-member/contracts", Name = nameof(ListTeamMemberContracts))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<PagedCollection<ContractView>>>> ListTeamMemberContracts([FromQuery] Guid employeeInformationId, [FromQuery] DateFilter dateFilter = null)
        {
            return Result(await _contractService.GetTeamMemberContract(employeeInformationId, dateFilter));
        }
    }
}