using shop.commerce.api.common;
//using System.ComponentModel.DataAnnotations.Schema;

namespace shop.commerce.api.infrastructure.Repositories.Entities
{
    //[NotMapped]
    public class Parameters : Entity<string>
    {
        public string Value { get; set; }

        public override void BuildSearchTerms()
            => SearchTerms = "";
    }
}
