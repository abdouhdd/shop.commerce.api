using shop.commerce.api.common;
using shop.commerce.api.domain.Enum;
using System;

namespace shop.commerce.api.domain.Entities
{
    public class OrderTracking : Entity<int>
    {
        public int OrderId { get; set; }
        public EnumOrderStatus Status { get; set; }
        public DateTime Date { get; set; }
        public virtual Order Order { get; set; }

        public override void BuildSearchTerms()
            => SearchTerms = $"";
    }
}
