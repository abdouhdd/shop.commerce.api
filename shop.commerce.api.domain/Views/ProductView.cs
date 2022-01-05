namespace shop.commerce.api.domain.Views
{
    public class ProductView
    {
        public int Id { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Slug { get; set; }
        public bool Active { get; set; }
        public string Category { get; set; }
        public int Rating { get; set; }
        public decimal Quantity { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public bool IsOffer { get; set; }
        public decimal Offer { get; set; }
        public int CountView { get; set; }
        public int Position { get; set; }
    }
}
