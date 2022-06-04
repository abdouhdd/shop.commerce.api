using shop.commerce.api.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shop.commerce.api.infrastructure.Repositories.Entities
{
    public partial class StockProduct : Entity<int>
    {
        public int ProductId { get; set; }
        public decimal Stock { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public partial class StockProduct 
    {
        public override void BuildSearchTerms()
            => SearchTerms = "";
    }
}
