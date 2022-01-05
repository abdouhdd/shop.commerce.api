namespace shop.commerce.api.domain.Models.Request
{
    public class OrderItemRequest
    {
        public string Slug { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
    }
}
