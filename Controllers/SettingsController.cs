using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;

namespace TimesheetBE.Controllers
{
    [ApiController]
    [Authorize(Roles = "Super Admin, Admin, Payroll Manager")]
    [Route("api/[controller]")]

    public class SettingsController : StandardControllerResponse
    {   
        private readonly IExpenseTypeService _expenseTypeService;

        public SettingsController(IExpenseTypeService expenseTypeService)
        {
            _expenseTypeService = expenseTypeService;
        }

        [HttpPost("expense-type/create/{name}", Name = nameof(CreateExpenseType))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<ExpenseTypeView>>> CreateExpenseType([FromQuery] Guid superAdminId, string name)
        {
            return Result(await _expenseTypeService.CreateExpenseType(superAdminId, name));
        }


        [HttpGet("expense-types", Name = nameof(ListExpenseTypes))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<ActionResult<StandardResponse<IEnumerable<ExpenseTypeView>>>> ListExpenseTypes([FromQuery] Guid superAdminId)
        {
            return Result(await _expenseTypeService.ListExpenseTypes(superAdminId));
        }

        [HttpPost("expense-type/status/{expenseTypeId}", Name = nameof(ToggleStatus))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StandardResponse<ExpenseTypeView>>>  ToggleStatus(Guid expenseTypeId)
        {
            return Result(await _expenseTypeService.ToggleStatus(expenseTypeId));
        }
    }
}