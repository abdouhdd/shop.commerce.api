namespace shop.commerce.api.presentation.Configuration.Accessors
{
    using shop.commerce.api.Application.Configuration;
    using shop.commerce.api.Application.Models;
    using Microsoft.Extensions.Options;
    using System;
    /// <summary>
    /// the application configuration accessor
    /// </summary>
    public partial class ApplicationSecretsAccessor : IApplicationSecretsAccessor
    {
        /// <summary>
        /// get the clinet id
        /// </summary>
        /// <returns>the clinet id</returns>
        public string ClientId()
            => _applicationSecrets.ClientId;

        /// <summary>
        /// get the authentication secrets
        /// </summary>
        /// <returns>the authentication secrets</returns>
        public AuthenticationSecrets GetAuthenticationSecrets()
            => _applicationSecrets.Authentication;

        /// <summary>
        /// get the client secret
        /// </summary>
        /// <returns>the client secret</returns>
        public string GetClientSecret()
            => _applicationSecrets.ClientSecret;

        /// <summary>
        /// Gets the hangfire database.
        /// </summary>
        /// <returns>hangfire database</returns>
        public string GetHangfireDatabase()
            => _applicationSecrets.ConnectionStrings.HangfireDatabase;

        /// <summary>
        /// get the main database
        /// </summary>
        /// <returns>the max allowed Login attempts</returns>
        public string GetMainDatabase()
            => _applicationSecrets.ConnectionStrings.MainDatabase;

        public string GetMemoryDatabase()
            => _applicationSecrets.MemoryDatabase;

        public bool GetIsMemoryDatabase()
            => _applicationSecrets.IsMemoryDatabase;

        public bool GetIsSqlServer()
            => _applicationSecrets.IsSqlServer;

        public bool GetIsPGSQL()
            => _applicationSecrets.IsPGSQL;
    }

    /// <summary>
    /// partial part for <see cref="ApplicationSecretsAccessor"/>
    /// </summary>
    public partial class ApplicationSecretsAccessor : IApplicationSecretsAccessor
    {
        private readonly IDisposable _secretsChangedDisposable;
        private ApplicationSecrets _applicationSecrets;

        /// <summary>
        /// create an instant of <see cref="ApplicationSecretsAccessor"/>
        /// </summary>
        /// <param name="options"></param>
        public ApplicationSecretsAccessor(IOptionsMonitor<ApplicationSecrets> options)
        {
            _applicationSecrets = options.CurrentValue;
            _secretsChangedDisposable = options.OnChange(e => _applicationSecrets = e);
        }

        /// <summary>
        /// the object destructor
        /// </summary>
        ~ApplicationSecretsAccessor()
        {
            _secretsChangedDisposable?.Dispose();
        }
    }

}
