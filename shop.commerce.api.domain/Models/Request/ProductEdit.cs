
using shop.commerce.api.domain.Views;

namespace shop.commerce.api.domain.Request
{
    public class ProductEdit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
        public bool Active { get; set; }
        public int CategoryId { get; set; }
        public int Rating { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public decimal Quantity { get; set; }
        public bool IsOffer { get; set; }
        public decimal Offer { get; set; }
        public string MetaTitle { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public ImageView[] Images { get; set; }
        public int CountView { get; set; }
        public string Details { get; set; }
        public string Specification { get; set; }
        public string MainCharacteristics { get; set; }
        public string TechnicalDescription { get; set; }
        public string General { get; set; }
        public string Garantie { get; set; }
        public string VenduWith { get; set; }
    }
}
