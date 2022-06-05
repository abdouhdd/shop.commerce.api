
using shop.commerce.api.domain.Entities;

namespace shop.commerce.api.domain.Entities
{
    public class Admin : Account
    {
        public override void BuildSearchTerms()
            => SearchTerms = $"";
    }
}
