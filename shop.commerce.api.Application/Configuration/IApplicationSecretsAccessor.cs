using shop.commerce.api.Application.Models;

namespace shop.commerce.api.Application.Configuration
{
    public interface IApplicationSecretsAccessor
    {
        string ClientId();
        AuthenticationSecrets GetAuthenticationSecrets();
        string GetClientSecret();
        string GetHangfireDatabase();
        string GetMainDatabase();
        bool GetIsMemoryDatabase();
        string GetMemoryDatabase();
        bool GetIsSqlServer();
        bool GetIsPGSQL();
    }
}
