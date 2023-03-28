using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Services.Abstractions;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;

namespace TimesheetBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class ExportController : StandardControllerResponse
    {
        private readonly IUserService _userService;
        private readonly IInvoiceService _invoiceService;
        public ExportController(IUserService userService, IInvoiceService invoiceService)
        {
            _userService = userService;
            _invoiceService = invoiceService;
        }

        [HttpGet("users", Name = nameof(ExportUserRecord))]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public ActionResult ExportUserRecord([FromQuery] UserRecordDownloadModel model, [FromQuery] DateFilter dateFilter )
        {
           var result = _userService.ExportUserRecord(model, dateFilter);
            if (result.Status)
            {
                return File(
                        result.Data,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"{model.Record.ToString()} for {dateFilter.StartDate:D} to {dateFilter.EndDate:D}.xlsx"
                        );
            }
            return BadRequest(result);
            
        }

        [HttpGet("invoice", Name = nameof(ExportInvoiceRecord))]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public ActionResult ExportInvoiceRecord([FromQuery] InvoiceRecordDownloadModel model, [FromQuery] DateFilter dateFilter)
        {
            var result = _invoiceService.ExportInvoiceRecord(model, dateFilter);
            if (result.Status)
            {
                return File(
                        result.Data,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"{model.Record.ToString()} for {dateFilter.StartDate:D} to {dateFilter.EndDate:D}.xlsx"
                        );
            }
            return BadRequest(result);

        }
    }
}
