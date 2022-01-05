using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shop.commerce.api.infrastructure.Repositories.Entities;

namespace shop.commerce.api.infrastructure.Configuration
{
    public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.HasKey(prop => new { prop.ProductId, prop.Filename });
            builder.HasAlternateKey(prop => prop.Id)
                .HasName("UXC_ProductImage_Id");

            //builder.HasOne(img => img.Product)
            //  .WithMany(p => p.ProductImages)
            //  .HasForeignKey(img => img.ProductId);
        }
    }
}
