using shop.commerce.api.services.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// the Configurations class for registering the layer services
    /// </summary>
    public static class Configurations
    {
        /// <summary>
        /// add the services from the services layer
        /// </summary>
        /// <param name="builder">the extensions builder instant</param>
        /// <returns><see cref="ConfigurationsBuilder"/> instant</returns>
        public static ConfigurationsBuilder AddPresentationLayer(this ConfigurationsBuilder builder)
        {
            //builder.Services.AddSingleton<IQueueManager, HangfireQueueService>();
            //builder.Services.AddSingleton<IApplicationSecretsAccessor, ApplicationSecretAccessor>();
            //builder.Services.AddSingleton<IApplicationSettingsAccessor, ApplicationSettingsAccessor>();
            
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IHomeService, HomeService>();

            return builder;
        }
    }
}
