using Microsoft.AspNetCore.Mvc;
using shop.commerce.api.infrastructure.Repositories.Entities;
using shop.commerce.api.services.Services;
using System.Collections.Generic;

namespace shop.commerce.api.presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerCore
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("GetAllNotifications")]
        public IEnumerable<Notification> GetAllNotifications(int adminId)
            => _notificationService.GetAllNotifications(DataUser);

        [HttpPost("GetNotifications")]
        public IEnumerable<Notification> GetNotifications(int adminId)
            => _notificationService.GetNotifications(DataUser);

        [HttpPost("MarkAsViewed")]
        public void MarkAsViewed(int adminId, string[] ids)
            => _notificationService.MarkAsViewed(DataUser, ids);

        [HttpPost("MarkAllAsViewed")]
        public void MarkAllAsViewed(int adminId)
            => _notificationService.MarkAllAsViewed(DataUser);
    }
}
