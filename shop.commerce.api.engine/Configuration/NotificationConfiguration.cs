using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shop.commerce.api.domain.Common;
using shop.commerce.api.infrastructure.Repositories.Entities;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        //builder.HasKey(prop => prop.Id);
        
        builder.Property(x => x.Id)
            .HasColumnType("VARCHAR")
            .HasMaxLength(20);

        builder.Property(e => e.Data)
            .HasConversion(
                e => e.ToJson(false, false),
                e => e.FromJson<DataNotification>()
            )
            .HasColumnType("TEXT")
            .HasMaxLength(1024);

    }
}
