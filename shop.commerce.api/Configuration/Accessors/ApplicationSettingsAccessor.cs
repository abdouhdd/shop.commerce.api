
namespace shop.commerce.api.presentation.Configuration.Accessors
{
    using shop.commerce.api.Application.Configuration;
    using shop.commerce.api.Application.Models;
    using shop.commerce.api.domain.Enum;
    using Microsoft.Extensions.Options;
    using System;

    /// <summary>
    /// the application configuration accessor
    /// </summary>
    public partial class ApplicationSettingsAccessor
    {
        /// <summary>
        /// get the environment type
        /// </summary>
        /// <returns>the <see cref="EnvironmentType"/></returns>
        public EnvironmentType GetEnvironmentType()
        {
            switch (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
            {
                case "Production":
                    return EnvironmentType.Production;
                case "Staging":
                case "Test":
                    return EnvironmentType.Test;
                default:
                    return EnvironmentType.Development;
            }
        }

        /// <summary>
        /// get the output directory where to save the files
        /// </summary>
        /// <returns>the directory path</returns>
        public string GetDirectoryImages()
            => _applicationSettings.DirectoryImages;

        /// <summary>
        /// get the output directory where to save the slides
        /// </summary>
        /// <returns></returns>
        public string GetDirectorySlides()
            => _applicationSettings.DirectorySlidesImages;

        /// <summary>
        /// Gets the time in minutes a user is locked out for when a lockout occurs. Defaults to 5 minutes.
        /// </summary>
        /// <returns>Lockout Time in minutes</returns>
        public double GetDefaultLockoutTimeSpan()
            => _applicationSettings.Authentication.DefaultLockoutTimeSpan;

        /// <summary>
        /// get the max allowed Login attempts
        /// </summary>
        /// <returns>the max allowed Login attempts</returns>
        public int GetMaxFailedAccessAttempts()
            => _applicationSettings.Authentication.MaxFailedAccessAttempts;

    }

    /// <summary>
    /// partial part for <see cref="ApplicationSettingsAccessor"/>
    /// </summary>
    public partial class ApplicationSettingsAccessor : IApplicationSettingsAccessor
    {
        private readonly IDisposable _secretsChangedDisposable;
        private ApplicationSettings _applicationSettings;

        /// <summary>
        /// create an instant of <see cref="ApplicationSettingsAccessor"/>
        /// </summary>
        /// <param name="options"></param>
        public ApplicationSettingsAccessor(IOptionsMonitor<ApplicationSettings> options)
        {
            _applicationSettings = options.CurrentValue;
            _secretsChangedDisposable = options.OnChange(e => _applicationSettings = e);
        }

        /// <summary>
        /// the object destructor
        /// </summary>
        ~ApplicationSettingsAccessor()
        {
            _secretsChangedDisposable?.Dispose();
        }
    }
}
