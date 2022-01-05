namespace Microsoft.Extensions.DependencyInjection
{
    using shop.commerce.api.common.Utilities;

    /// <summary>
    /// Extension methods to configure GesisaApps Services.
    /// </summary>
    public static class GesisaAppsCommonExtensions
    {
        /// <summary>
        /// Adds GesisaApps Common Utilities
        /// <para>
        /// this will give you access to full list of Gesisa Apps services, for a simple communication.
        /// </summary>
        /// <param name="services">the services collection</param>
        public static void AddGesisaAppsCommonUtilities(this IServiceCollection services)
        {
            // register the PuenteSign Service
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
        }
    }
}
