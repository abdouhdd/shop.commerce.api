using shop.commerce.api.domain.Enum;
using System.Security.Claims;

namespace shop.commerce.api.domain.Models.Account
{
    public class ProfileModel
    {
        public string Token { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public EnumRole Role { get; set; }
    }
}
