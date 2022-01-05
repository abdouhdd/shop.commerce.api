using shop.commerce.api.common;
using System;

namespace shop.commerce.api.infrastructure.Repositories.Entities
{
    public class ProductImage : Entity<string>
    {
        public int ProductId { get; set; }
        public string Filename { get; set; }
        public bool IsMaster { get; set; }
        public int Position { get; set; }

        public virtual Product Product { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ProductImage productImage && productImage.ProductId == ProductId && productImage.Filename == Filename;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ProductId, Filename);
        }

        public override void BuildSearchTerms()
            => SearchTerms = $"";
    }
}
