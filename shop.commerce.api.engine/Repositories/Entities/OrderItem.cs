
using shop.commerce.api.common;
using System.ComponentModel.DataAnnotations;

namespace shop.commerce.api.infrastructure.Repositories.Entities
{
    public class OrderItem : Entity<int>
    {
        //[Key]
        public string OrderItemNumber { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public virtual Product Product { get; set; }
        public virtual Order Order { get; set; }

        public override void BuildSearchTerms()
            => SearchTerms = $"";
    }
}
