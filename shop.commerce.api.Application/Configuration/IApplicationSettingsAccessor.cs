
namespace shop.commerce.api.Application.Configuration
{
    using shop.commerce.api.Application.Models;
    using shop.commerce.api.domain.Enum;
    public interface IApplicationSettingsAccessor
    {
        /// <summary>
        /// get the output directory where to save the files
        /// </summary>
        /// <returns>the directory path</returns>
        string GetDirectoryImages();
        /// <summary>
        /// get the output directory where to save the slides images
        /// </summary>
        /// <returns></returns>
        string GetDirectorySlides();
        /// <summary>
        /// get the Environment Type
        /// </summary>
        /// <returns>the <see cref="EnvironmentType"/></returns>
        EnvironmentType GetEnvironmentType();

        /// <summary>
        /// get the max allowed Login attempts
        /// </summary>
        /// <returns>the max allowed Login attempts</returns>
        int GetMaxFailedAccessAttempts();

        /// <summary>
        /// Gets the time in minutes a user is locked out for when a lockout occurs. Defaults to 5 minutes.
        /// </summary>
        /// <returns>Lockout Time in minutes</returns>
        double GetDefaultLockoutTimeSpan();
       
    }
}
