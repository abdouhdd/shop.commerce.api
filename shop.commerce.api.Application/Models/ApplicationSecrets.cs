
namespace shop.commerce.api.Application.Models
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// this class will hold a reference
    /// to the secretes used in the application
    /// </summary>
    public class ApplicationSecrets
    {
        /// <summary>
        /// the client Id of the app
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// the client Secret of the app
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// the Authentication Secrets
        /// </summary>
        public AuthenticationSecrets Authentication { get; set; }

        /// <summary>
        /// the ConnectionStrings secrets
        /// </summary>
        public ConnectionStrings ConnectionStrings { get; set; }

        public string MemoryDatabase { get; set; }

        public bool IsMemoryDatabase { get; set; }
        public bool IsSqlServer { get; set; }
        public bool IsPGSQL { get; set; }
    }

    /// <summary>
    /// the Authentication Secrets
    /// </summary>
    public class AuthenticationSecrets
    {
        /// <summary>
        /// the Signing Secret
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// the token issuer
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// the token audience
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// the token expiration
        /// </summary>
        public int AccessExpiration { get; set; }

        /// <summary>
        /// the refresh token expiration
        /// </summary>
        public int RefreshExpiration { get; set; }

        /// <summary>
        /// Generates a random value (nonce) for each generated token.
        /// </summary>
        /// <remarks>The default nonce is a random GUID.</remarks>
        public static Func<Task<string>> NonceGenerator { get; set; }
            = () => Task.FromResult(Guid.NewGuid().ToString());
    }

    /// <summary>
    /// database connection strings
    /// </summary>
    public class ConnectionStrings
    {
        /// <summary>
        /// connection string to the main app database
        /// </summary>
        public string MainDatabase { get; set; }

        /// <summary>
        /// connection string to Hangfire database
        /// </summary>
        public string HangfireDatabase { get; set; }
    }

}
