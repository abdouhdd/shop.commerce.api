using System.Collections.Generic;
using System.Text;

namespace shop.commerce.api.domain.Request
{
    public class CategoryRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public int? ParentId { get; set; }
        public string Icon { get; set; }
        public List<CategoryRequest> Children { get; set; }
    }
}
