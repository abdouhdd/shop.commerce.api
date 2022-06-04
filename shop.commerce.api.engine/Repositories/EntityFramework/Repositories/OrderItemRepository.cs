﻿using Microsoft.Extensions.Logging;
using shop.commerce.api.infrastructure.Repositories.Entities;

namespace shop.commerce.api.infrastructure.Repositories.EntityFramework
{
    public partial class OrderItemRepository : IOrderItemRepository
    {
        
    }

    public partial class OrderItemRepository : Repository<OrderItem, int>
    {
        public OrderItemRepository(ShopContext context,
            ILoggerFactory loggerFactory)
            : base(context, loggerFactory)
        {
        }
    }
}
