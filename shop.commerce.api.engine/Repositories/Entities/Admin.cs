
namespace shop.commerce.api.infrastructure.Repositories.Entities
{
    public class Admin : Account
    {
        public override void BuildSearchTerms()
            => SearchTerms = $"";
    }
}
