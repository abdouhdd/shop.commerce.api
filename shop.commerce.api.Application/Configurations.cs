namespace Microsoft.Extensions.DependencyInjection
{
    using System;

    /// <summary>
    /// the Configurations class for registering the layer services
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static class Configurations
    {
        /// <summary>
        /// add the Application Layer configuration, and return the <see cref="ConfigurationsBuilder"/>
        /// instant
        /// </summary>
        /// <param name="services">the service collection instant</param>
        /// <returns>a <see cref="ConfigurationsBuilder"/> instant</returns>
        public static ConfigurationsBuilder AddApplicationLayer(this IServiceCollection services, string projectId)
        {
            // register AutoMapper
            //services.AddAutoMapper(typeof(ApplicationModelsMappingProfile).Assembly);

            // register handlers
            //services.AddScoped<IStationHandler, StationHandler>();

            // return the extension builder
            return new ConfigurationsBuilder(services, projectId);
        }
    }

    /// <summary>
    /// this class will help you configure the layers
    /// </summary>
    public class ConfigurationsBuilder
    {
        /// <summary>
        /// create an instant of <see cref="ConfigurationsBuilder"/>
        /// </summary>
        /// <param name="services">the service collection</param>
        /// <param name="globalConfiguration">the global configuration</param>
        /// <param name="secrets">the secretes instant</param>
        public ConfigurationsBuilder(IServiceCollection services, string projectId)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            Services = services;
            ProjectId = projectId;
        }

        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> services are attached to.
        /// </summary>
        /// <value>
        /// The <see cref="IServiceCollection"/> services are attached to.
        /// </value>
        public IServiceCollection Services { get; private set; }

        /// <summary>
        /// the id of the project
        /// </summary>
        public string ProjectId { get; set; }
    }
}
