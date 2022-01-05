using shop.commerce.api.domain.Enum;

namespace shop.commerce.api.domain.Filters
{
    public class OrderFilterAdmin
    {
        //public int PageNumber { get; set; } = 1;
        //public int Length { get; set; } = 10;
        public int? Status { get; set; }
        public string Search { get; set; }
    }

}
