using shop.commerce.api.domain.Enum;
using System;
using System.Collections.Generic;

namespace shop.commerce.api.domain.Views
{
    public class OrderView
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Username { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Address { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsPaid { get; set; }
        public EnumOrderStatus Status { get; set; }
        public string StatusName { get => Status.ToString(); }
        public EnumPaymentMethod PaymentMethod { get; set; }
        public string OrdersNote { get; set; }
        public List<OrderItemView> Items { get; set; }
        public DateTime? ProcessAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string AddressIp { get; set; }
        public string Browser { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
