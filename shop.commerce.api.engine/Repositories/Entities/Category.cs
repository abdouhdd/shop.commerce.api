using shop.commerce.api.common;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations.Schema;

namespace shop.commerce.api.infrastructure.Repositories.Entities
{
    public class Category : Entity<int>
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public string Name { get; set; }
        public string Slug { get; set; }
        public bool Active { get; set; }
        public bool HasChildren { get; set; }
        public int Level { get; set; }
        public int? ParentId { get; set; }
        public string Icon { get; set; }
        public int CountProducts { get; set; }
        public int CountAllProducts { get; set; }
        public int Position { get; set; }
        public virtual IEnumerable<Category> Children { get; set; }
        public virtual Category Parent { get; set; }
        public virtual IEnumerable<Product> Products { get; set; }

        public override void BuildSearchTerms()
            => SearchTerms = $"";
    }
}
