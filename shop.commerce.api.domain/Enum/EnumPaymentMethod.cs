using System.ComponentModel;

namespace shop.commerce.api.domain.Enum
{
    public enum EnumPaymentMethod
    {
        [Description("Cash on delivery")]
        cash_on_delivery = 1,
        [Description("Master card")]
        master_card = 2,
        [Description("Paypal")]
        paypal = 3,
    }

}
