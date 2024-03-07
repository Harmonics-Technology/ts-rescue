using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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
    [Authorize]
    public class DepartmentController : StandardControllerResponse
    {
        private readonly IDepartmentService _departmentService;
        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpPost("add-department", Name = nameof(CreateDepartment))]
        public async Task<ActionResult<StandardResponse<bool>>> CreateDepartment([FromQuery] Guid superAdminId, [FromQuery] string name)
        {
            return Result(await _departmentService.CreateDepartment(superAdminId, name));
        }

        [HttpGet("departments", Name = nameof(ListDepartments))]
        public async Task<ActionResult<StandardResponse<IEnumerable<DepartmentView>>>> ListDepartments([FromQuery] Guid superAdminId)
        {
            return Result(await _departmentService.ListDepartments(superAdminId));
        }

        [HttpPost("delete/{departmentId}", Name = nameof(DeleteDepartment))]
        public async Task<ActionResult<StandardResponse<bool>>> DeleteDepartment(Guid departmentId)
        {
            return Result(await _departmentService.DeleteDepartment(departmentId));
        }
    }
}
