using Microsoft.Extensions.Logging;
using shop.commerce.api.infrastructure.Repositories.Entities;

namespace shop.commerce.api.infrastructure.Repositories.EntityFramework
{
    public partial class ProductImageRepository : IProductImageRepository
    {

    }

    public partial class ProductImageRepository : Repository<ProductImage, string>
    {
        
        public ProductImageRepository(ShopContext context,
            ILoggerFactory loggerFactory)
            : base(context, loggerFactory)
        {
        }
    }

}
