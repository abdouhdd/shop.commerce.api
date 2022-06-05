using shop.commerce.api.domain.Enum;

namespace shop.commerce.api.domain.Filters
{
    public class ProductFilterUser
    {
        public int[] Categories { get; set; }
        public int PageNumber { get; set; }
        public int Length { get; set; }
        public decimal[] Price { get; set; }
        public string Search { get; set; }
    }

    public class ProductFilterAdmin
    {
        public int PageNumber { get; set; } = 1;
        public int Length { get; set; } = 10;
        public int? CategoryId { get; set; }
        public string Search { get; set; }
        public string Seller { get; set; }
    }

}
