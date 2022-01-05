using shop.commerce.api.common;

namespace shop.commerce.api.infrastructure.Repositories.Entities
{
    public class Slide : Entity<int>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Link { get; set; }
        public int Index { get; set; }
        public bool Active { get; set; }

        public override void BuildSearchTerms()
            => SearchTerms = $"";
    }
}
