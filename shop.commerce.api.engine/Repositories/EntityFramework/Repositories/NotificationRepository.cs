using Microsoft.Extensions.Logging;
using shop.commerce.api.infrastructure.Repositories.Entities;

namespace shop.commerce.api.infrastructure.Repositories.EntityFramework
{
    public partial class NotificationRepository : INotificationRepository
    {
        
    }
    public partial class NotificationRepository : Repository<Notification, string>
    {
        public NotificationRepository(
            ShopContext context,
            ILoggerFactory loggerFactory)
            : base(context, loggerFactory) { }
    }
}
