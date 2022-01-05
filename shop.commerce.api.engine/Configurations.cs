
namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.EntityFrameworkCore;
    using shop.commerce.api.infrastructure.Repositories;
    using shop.commerce.api.infrastructure.Repositories.EntityFramework;
    using System;

    /// <summary>
    /// the configuration class for the Persistence layer
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// add the Persistence Layer to the application
        /// </summary>
        /// <param name="builder">the service Collection</param>
        public static ConfigurationsBuilder AddInfrastructureLayer(
            this ConfigurationsBuilder builder, Action<InfrastructureOptions> options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            var option = new InfrastructureOptions();
            options(option);

            if (string.IsNullOrEmpty(option.DatabaseConnectionString) ||
                string.IsNullOrWhiteSpace(option.DatabaseConnectionString))
                throw new ArgumentNullException(nameof(option.DatabaseConnectionString),
                    "you must supply a valid connection string");


            if (option.IsMemoryDatabase)
            {
                builder.Services.AddDbContext<ShopContext>(contextOptions =>
                {
                    contextOptions.UseInMemoryDatabase(option.MemoryDatabase);
                });
            }
            else if (option.IsSqlServer)
            {
                builder.Services.AddDbContext<ShopContext>(contextOptions =>
                {
                    //contextOptions.EnableSensitiveDataLogging();
                    contextOptions.UseSqlServer(option.DatabaseConnectionString);
                    contextOptions.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                });
            }
            else if (option.IsPGSQL)
            {
                builder.Services.AddDbContext<ShopContext>(contextOptions =>
                {
                    //contextOptions.EnableSensitiveDataLogging();
                    contextOptions.UseNpgsql(option.DatabaseConnectionString, opt => opt.SetPostgresVersion(new Version(9, 6)));
                    contextOptions.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                });
            }
           

            //builder.Services.AddSingleton<IPathBuilder, PathBuilder>();
            //builder.Services.AddSingleton<IEmailService, EmailService>();

            RegisterRepositories(builder);

            RegisterManagers(builder);

            RegisterBuilders(builder);

            RegisterValidators(builder);

            RegisterResolvers(builder);

            // register AutoMapper
            //builder.Services.AddAutoMapper(typeof(ServiceModelsMappingProfile).Assembly);

            return builder;
        }

        private static void RegisterResolvers(ConfigurationsBuilder builder)
        {
            //builder.Services.AddScoped<IApplicationTypeResolver, ApplicationTypeResolver>();
            //builder.Services.AddScoped<IResolver<IFileQueueBuilder>, FileQueueBuilderResolver>();
            //builder.Services.AddScoped<IResolver<IFileRequestValidator>, FileRequestValidatorResolver>();
        }

        private static void RegisterValidators(ConfigurationsBuilder builder)
        {
            //builder.Services.AddScoped<IFileRequestValidator, SignFileRequestValidator>();
            //builder.Services.AddScoped<IFileRequestValidator, SiiFileRequestValidator>();
            //builder.Services.AddScoped<IFileRequestValidator, SilicieFileRequestValidator>();
        }

        private static void RegisterBuilders(ConfigurationsBuilder builder)
        {
            //builder.Services.AddScoped<IFileQueueBuilder, SignFileQueueBuilder>();
            //builder.Services.AddScoped<IFileQueueBuilder, SiiFileQueueBuilder>();
            //builder.Services.AddScoped<IFileQueueBuilder, SilicieFileQueueBuilder>();
        }

        private static void RegisterManagers(ConfigurationsBuilder builder)
        {
            //builder.Services.AddScoped<IStationManager, StationManager>();
            //builder.Services.AddScoped<IFileQueueManager, FileQueueManager>();
            //builder.Services.AddScoped<IGesisaApplicationManager, GesisaApplicationManager>();
        }

        private static void RegisterRepositories(ConfigurationsBuilder builder)
        {
            // repositories
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAdminRepository, AdminRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            builder.Services.AddScoped<IOrderTrackingRepository, OrderTrackingRepository>();
            builder.Services.AddScoped<ISlideRepository, SlideRepository>();

        }
    }

    /// <summary>
    /// the options for the Persistence layer
    /// </summary>
    public class InfrastructureOptions
    {
        /// <summary>
        /// the connection string to the database
        /// </summary>
        public string DatabaseConnectionString { get; set; }
        public bool IsMemoryDatabase { get; set; }
        public bool IsPGSQL { get; set; }
        public bool IsSqlServer { get; set; }
        public string MemoryDatabase { get; set; }
    }
}
