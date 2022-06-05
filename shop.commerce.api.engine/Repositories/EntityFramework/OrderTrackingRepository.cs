using Microsoft.Extensions.Logging;
using shop.commerce.api.domain.Entities;

namespace shop.commerce.api.infrastructure.Repositories.EntityFramework
{
    public partial class OrderTrackingRepository : IOrderTrackingRepository
    {
    }

    public partial class OrderTrackingRepository : Repository<OrderTracking, int>
    {
        public OrderTrackingRepository(ShopContext context,
            ILoggerFactory loggerFactory)
            : base(context, loggerFactory)
        {
        }
    }
}
