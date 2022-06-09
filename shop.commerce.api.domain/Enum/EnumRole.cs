using System.ComponentModel;

namespace shop.commerce.api.domain.Enum
{
    public enum EnumRole
    {
        [Description("User")]
        User = 1,
        [Description("Admin")]
        Admin = 2,
        [Description("Admin")]
        SupperAdmin = 3,
    }

}
