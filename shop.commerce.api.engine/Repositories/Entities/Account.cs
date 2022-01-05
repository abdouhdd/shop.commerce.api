
using shop.commerce.api.common;
using shop.commerce.api.domain.Enum;
using System;

namespace shop.commerce.api.infrastructure.Repositories.Entities
{
    public class Account : Entity<int>
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public DateTime RegistrationDate { get; set; }
        public EnumRole Role { get; set; }
        public int CountFailLogin { get; set; }
        public int CountDesactive { get; set; }
        public DateTime? DateDesactive { get; set; }
        public EnumStatusAccount Status { get; set; }

        public override void BuildSearchTerms()
            => SearchTerms = $"";
    }
}
