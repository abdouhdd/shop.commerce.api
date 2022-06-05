using shop.commerce.api.domain.Enum;
using shop.commerce.api.domain.Views;
using shop.commerce.api.domain.Entities;
using System.Collections.Generic;

namespace shop.commerce.api.infrastructure.Repositories
{
    public interface IOrderRepository : IRepository<Order, int>
    {
        Order GetOrderBy(string orderNumber);
        IEnumerable<OrderView> GetOrdersView(EnumOrderStatus? status, string search);
        OrderView GetOrderViewBy(string orderNumber);
        int UpdateOrderStatus(Order order, EnumOrderStatus status);
        IEnumerable<OrderTracking> GetOrderTrackings(int orderId);
        OrderItemView GetOrderDetailsView(string orderItemNumber);
        OrderView GetOrderView(string orderNumber);
    }
}
