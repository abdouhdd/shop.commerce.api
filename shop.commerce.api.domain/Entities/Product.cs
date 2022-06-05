using shop.commerce.api.common;
using System.Collections.Generic;

namespace shop.commerce.api.domain.Entities
{
    public class Product : Entity<int>
    {
        public string Admin { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
        public string Specification { get; set; }
        public string Slug { get; set; }
        public bool Active { get; set; }
        public int? CategoryId { get; set; }
        public int Rating { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public decimal Quantity { get; set; }
        public bool IsOffer { get; set; }
        public decimal Offer { get; set; }
        public string MetaTitle { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string Image { get; set; }
        public int CountView { get; set; }
        public int CountSale { get; set; }
        public int Position { get; set; }
        public string MainCharacteristics { get; set; }
        public string TechnicalDescription { get; set; }
        public string General { get; set; }
        public string Garantie { get; set; }
        public string VenduWith { get; set; }
        public virtual Category Category { get; set; }
        public virtual IEnumerable<ProductImage> ProductImages { get; set; }

        public override void BuildSearchTerms()
            => SearchTerms = $"{Name} {Description} {Slug} {MetaKeywords} {MetaTitle} {MetaDescription}".ToLower();
    }
}
