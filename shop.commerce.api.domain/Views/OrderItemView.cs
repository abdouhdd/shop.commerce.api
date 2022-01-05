namespace shop.commerce.api.domain.Views
{
    public class OrderItemView
    {
        public string OrderItemNumber { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public string Slug { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int Rating { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public bool IsOffer { get; set; }
        public decimal Offer { get; set; }
    }
}
