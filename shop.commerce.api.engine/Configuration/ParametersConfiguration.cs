using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shop.commerce.api.infrastructure.Repositories.Entities;

public class ParametersConfiguration : IEntityTypeConfiguration<Parameters>
{
    public void Configure(EntityTypeBuilder<Parameters> builder)
    {
        //builder.HasKey(prop => prop.Id);
        //builder.Property(prop => prop.Id)
        //    .ValueGeneratedOnAdd();

        builder.Property(x => x.Id)
            .HasColumnType("VARCHAR")
            .HasMaxLength(50);
        
        builder.Property(x => x.Value)
            .HasColumnType("VARCHAR")
            .HasMaxLength(255);

    }
}
