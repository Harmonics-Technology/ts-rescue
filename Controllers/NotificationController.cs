using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TimesheetBE.Models.UtilityModels;
using TimesheetBE.Models.ViewModels;
using TimesheetBE.Services.Interfaces;
using TimesheetBE.Utilities;

namespace TimesheetBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : StandardControllerResponse
    {
        private readonly INotificationService _notificationService;
        private readonly PagingOptions _defaultPagingOptions;

        public NotificationController(IOptions<PagingOptions> defaultPagingOptions, INotificationService notificationService)
        {
            _defaultPagingOptions = defaultPagingOptions.Value;
            _notificationService = notificationService;
        }

        [HttpGet("list", Name = nameof(ListMyNotifications))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<StandardResponse<PagedCollection<NotificationView>>>> ListMyNotifications([FromQuery] PagingOptions pagingOptions)
        {
            pagingOptions.Replace(_defaultPagingOptions);
            return Result(await _notificationService.ListMyNotifications(pagingOptions));
        }

        [HttpGet("{id}", Name = nameof(GetNotification))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<StandardResponse<NotificationView>>> GetNotification(Guid id)
        {
            return Result(await _notificationService.GetNotification(id));
        }

        [HttpPut("{id}/mark-as-read", Name = nameof(MarkAsRead))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<StandardResponse<NotificationView>>> MarkAsRead(Guid id)
        {
            return Result(await _notificationService.MarkAsRead(id));
        }
    }
}