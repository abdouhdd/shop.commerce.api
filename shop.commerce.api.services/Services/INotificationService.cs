using shop.commerce.api.infrastructure.Repositories.Entities;
using shop.commerce.api.services.Models;
using System.Collections.Generic;

namespace shop.commerce.api.services.Services
{
    public interface INotificationService
    {
        IEnumerable<Notification> GetAllNotifications(DataUser admin);
        IEnumerable<Notification> GetNotifications(DataUser admin);
        void MarkAsViewed(DataUser admin, string[] ids);
        void MarkAllAsViewed(DataUser admin);
    }
}
