using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shop.commerce.api.domain.Extensions
{
    public static class Calcul
    {
        public static bool IsOffer(decimal price, decimal specialPrice)
        {
            return specialPrice < price;
        }

        public static decimal Offer(decimal price, decimal specialPrice)
        {
            //return price - specialPrice;
            return (price - specialPrice) * 100 / price;
        }

        public static decimal CalculSpecialPrice(decimal offer, decimal price)
        {
            decimal specialPrice = price - (offer * price / 100);
            return specialPrice;
        }
    }
}

