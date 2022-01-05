namespace shop.commerce.api.domain.Views
{
    public class CategoryView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public bool Active { get; set; }
        public int Level { get; set; }
        public string Icon { get; set; }
        public int CountProducts { get; set; }
        public bool HasChildren { get; set; }
        public CategoryView[] Children { get; set; }
        public int? ParentId { get; set; }
        public CategoryView Parent { get; set; }
        public int CountAllProducts { get; set; }
        public int Position { get; set; }
    }
}
