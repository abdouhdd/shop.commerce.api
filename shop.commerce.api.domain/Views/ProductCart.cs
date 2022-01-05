using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shop.commerce.api.domain.Views
{
    public class ProductCart
    {
        public string Slug { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int NewPrice { get; set; }
        public int OldPrice { get; set; }
        public string[] Images { get; set; }
    }
}
