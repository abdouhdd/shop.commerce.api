using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shop.commerce.api.infrastructure.Repositories;

namespace shop.commerce.api.infrastructure.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            //builder.HasKey(prop => prop.Id);

            builder.Property(prop => prop.Email)
               .HasColumnType("VARCHAR")
               .IsRequired()
               .HasMaxLength(255);

            builder.HasIndex(prop => prop.Email)
                .IsUnique();

            builder.Property(prop => prop.Username)
                .HasColumnType("VARCHAR")
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(prop => prop.Username)
                .IsUnique();
        }
    }
}
