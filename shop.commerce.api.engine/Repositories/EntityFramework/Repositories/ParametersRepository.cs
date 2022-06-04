using Microsoft.Extensions.Logging;
using shop.commerce.api.infrastructure.Repositories.Entities;

namespace shop.commerce.api.infrastructure.Repositories.EntityFramework
{
    public partial class ParametersRepository : Repository<Parameters, string>
    {

    }
    public partial class ParametersRepository : IParametersRepository
    {
        
        public ParametersRepository(
            ShopContext context,
            ILoggerFactory loggerFactory)
            : base(context, loggerFactory) { }
    }
}
