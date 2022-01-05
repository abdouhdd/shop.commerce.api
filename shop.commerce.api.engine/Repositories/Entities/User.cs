using shop.commerce.api.infrastructure.Repositories.Entities;

namespace shop.commerce.api.infrastructure.Repositories
{
    public class User : Account
    {

        public override void BuildSearchTerms()
            => SearchTerms = $"";
    }
}
