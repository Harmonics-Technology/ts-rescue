using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Services;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;

namespace TimesheetBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilityController : StandardControllerResponse
    {
        private readonly IUtilityService _utilityService;
        public UtilityController(IUtilityService utilityService)
        {
            _utilityService = utilityService;
        }

        [HttpPost("contact-us", Name = nameof(SendContactMessage))]
        [AllowAnonymous]
        public async Task<ActionResult<StandardResponse<bool>>> SendContactMessage(ContactMessageModel model)
        {
            return Result(await _utilityService.SendContactMessage(model));
        }

        [HttpGet("countries", Name = nameof(ListCountries))]
        [AllowAnonymous]
        public async Task<ActionResult<StandardResponse<List<CountryView>>>> ListCountries()
        {
            return Result(await _utilityService.ListCountries());
        }
    }
}
