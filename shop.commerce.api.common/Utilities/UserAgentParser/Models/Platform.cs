namespace shop.commerce.api.common.Utilities
{
    using System;
    using System.Linq;

    /// <summary>
    /// this class defines a platform
    /// </summary>
    public static class Platform
    {
        /// <summary>
        /// list of all available platforms
        /// </summary>
        public static string[] ALL = new[] { Mobile, WebApp, desktop, AccoutantWebApp };

        /// <summary>
        /// the mobile platform
        /// </summary>
        public const string Mobile = "mobile";

        /// <summary>
        /// shop.commerce.api.common.Utilities web platform
        /// </summary>
        public const string WebApp = "web-app";

        /// <summary>
        /// the desktop platform
        /// </summary>
        public const string desktop = "desktop";

        /// <summary>
        /// accountant web platform
        /// </summary>
        public const string AccoutantWebApp = "web-app-accountant";

        /// <summary>
        /// check if the given platform is valid
        /// </summary>
        /// <param name="platform">the platform to be checked</param>
        /// <returns>true if valid false if not</returns>
        public static bool IsValid(string platform) => ALL.Contains(platform, StringComparer.OrdinalIgnoreCase);
    }
}
