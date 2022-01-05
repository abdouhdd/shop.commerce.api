using System.ComponentModel;

namespace shop.commerce.api.domain.Enum
{
    public enum EnumOrderStatus
    {
        [Description("En cours")]
        Pending = 1,
        [Description("En attente")]
        Processing = 2,
        [Description("Attente paiement")]
        WaitPayment = 3,
        [Description("Remboursé")]
        Rembourse = 4,
        [Description("Terminé")]
        Completed = 5,
        [Description("Annulé")]
        Canceled = 6,
        [Description("Échouée")]
        Failed = 7
    }
    public enum EnumStatusAccount
    {
        [Description("Active")]
        Active = 1,
        [Description("Desactive")]
        Desactive = 2,
        [Description("Bloque")]
        Bloque = 3,
    }

}
