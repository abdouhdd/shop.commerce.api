using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shop.commerce.api.domain.Entities;

namespace shop.commerce.api.infrastructure.Configuration
{
    public class SlideConfiguration : IEntityTypeConfiguration<Slide>
    {
        public void Configure(EntityTypeBuilder<Slide> builder)
        {
            builder.HasKey(prop => new { prop.Id });
        }
    }
}
