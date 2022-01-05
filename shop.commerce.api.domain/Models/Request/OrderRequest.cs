using shop.commerce.api.domain.Enum;

namespace shop.commerce.api.domain.Models.Request
{
    public class OrderRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Address { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalAmount { get; set; }
        public EnumPaymentMethod PaymentMethod { get; set; }
        public string OrdersNote { get; set; }
        public OrderItemRequest[] Items { get; set; }
        public EnumOrderStatus? OrderStatus { get; set; }
        public string AddressIp { get; set; }
        public string Browser { get; set; }
    }
}
