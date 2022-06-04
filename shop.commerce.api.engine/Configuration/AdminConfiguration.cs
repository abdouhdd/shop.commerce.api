using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shop.commerce.api.infrastructure.Repositories.Entities;

namespace shop.commerce.api.infrastructure.Configuration
{
    public class AdminConfiguration : IEntityTypeConfiguration<Admin>
    {
        public void Configure(EntityTypeBuilder<Admin> builder)
        {
            builder.Property(prop => prop.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(prop => prop.Email)
                .IsUnique();
            
            builder.Property(prop => prop.Username)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(prop => prop.Username)
                .IsUnique();
        }
    }
}
