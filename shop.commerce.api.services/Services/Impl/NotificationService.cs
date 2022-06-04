
using shop.commerce.api.infrastructure.Repositories;
using shop.commerce.api.infrastructure.Repositories.Entities;
using shop.commerce.api.services.Models;
using System.Collections.Generic;
using System.Linq;

namespace shop.commerce.api.services.Services.Impl
{
    public partial class NotificationService : INotificationService
    {

        public IEnumerable<Notification> GetAllNotifications(DataUser admin)
        {
            Admin adm = _adminRepository.FindAdminByUsername(admin.Username);
            return _notificationRepository.GetAll(req => req.AddPredicate(n => n.AdminId == adm.Id));
        }

        public IEnumerable<Notification> GetNotifications(DataUser admin)
        {
            Admin adm = _adminRepository.FindAdminByUsername(admin.Username);
            return _notificationRepository.GetAll(req => req.AddPredicate(n => n.AdminId == adm.Id && !n.IsViewed));
        }

        public void MarkAllAsViewed(DataUser admin)
        {
            Admin adm = _adminRepository.FindAdminByUsername(admin.Username);
            Notification[] notifications = _notificationRepository.GetAll(req => req.AddPredicate(n => n.AdminId == adm.Id && !n.IsViewed));
            foreach (Notification notification in notifications)
            {
                notification.IsViewed = true;
            }
            _notificationRepository.UpdateRange(notifications);
        }

        public void MarkAsViewed(DataUser admin, string[] ids)
        {
            Admin adm = _adminRepository.FindAdminByUsername(admin.Username);
            Notification[] notifications = _notificationRepository.GetAll(req => req.AddPredicate(n => n.AdminId == adm.Id && ids.Contains(n.Id)));
            foreach (Notification notification in notifications)
            {
                notification.IsViewed = true;
            }
            _notificationRepository.UpdateRange(notifications);
        }
    }

    public partial class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IAdminRepository _adminRepository;

        public NotificationService(INotificationRepository notificationRepository, IAdminRepository adminRepository)
        {
            _notificationRepository = notificationRepository;
            _adminRepository = adminRepository;
        }
    }
}
