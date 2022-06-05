using shop.commerce.api.common;
using shop.commerce.api.domain.Enum;
using System;
using System.Collections.Generic;

namespace shop.commerce.api.domain.Entities
{
    public class Order : Entity<int>
    {
        public string OrderNumber { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Address { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsPaid { get; set; }
        public EnumOrderStatus Status { get; set; }
        public EnumPaymentMethod PaymentMethod { get; set; }
        public string OrdersNote { get; set; }
        //public DateTimeOffset? ProcessAt { get; set; }
        //public DateTimeOffset? DeliveredAt { get; set; }
        public string AddressIp { get; set; }
        public string Browser { get; set; }
        public virtual IEnumerable<OrderItem> Items { get; set; }
        public virtual IEnumerable<OrderTracking> OrderTrackings { get; set; }

        public override void BuildSearchTerms()
            => SearchTerms = $"{FullName} {OrderNumber} {OrdersNote} {Country} {Phone}".ToLower();
    }
}
