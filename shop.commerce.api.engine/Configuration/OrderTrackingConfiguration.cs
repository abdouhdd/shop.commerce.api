using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shop.commerce.api.domain.Entities;

namespace shop.commerce.api.infrastructure.Configuration
{
    public class OrderTrackingConfiguration : IEntityTypeConfiguration<OrderTracking>
    {
        public void Configure(EntityTypeBuilder<OrderTracking> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.HasOne(ot => ot.Order)
                .WithMany(o => o.OrderTrackings)
                .HasForeignKey(ot => ot.OrderId);
        }
    }
}
