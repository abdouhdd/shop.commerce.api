using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shop.commerce.api.infrastructure.Repositories.Entities;

namespace shop.commerce.api.infrastructure.Configuration
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            //builder.HasKey(x => x.Id);
            //builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.HasIndex(x => x.OrderItemNumber).IsUnique();
            builder.Property(x => x.OrderItemNumber)
                .IsRequired()
                .HasColumnType("VARCHAR")
                .HasMaxLength(20);

            builder.HasOne(ot => ot.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(ot => ot.OrderId);

            builder.HasOne(ot => ot.Product)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(ot => ot.ProductId);
        }
    }
}
