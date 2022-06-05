using shop.commerce.api.domain.Entities;

namespace shop.commerce.api.domain.Entities
{
    public class User : Account
    {

        public override void BuildSearchTerms()
            => SearchTerms = $"";
    }
}
