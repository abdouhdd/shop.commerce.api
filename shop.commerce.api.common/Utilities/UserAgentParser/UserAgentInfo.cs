namespace shop.commerce.api.common.Utilities
{
    /// <summary>
    /// Represents the user agent client information resulting from parsing
    /// a user agent string
    /// </summary>
    public partial class UserAgentInfo : IUserAgentInfo
    {
        /// <summary>
        /// The user agent string, the input for the UAParser
        /// </summary>
        public string UserAgentValue { get; }

        /// <summary>
        /// The OS parsed from the user agent string
        /// </summary>
        public OS OS { get; }

        /// <summary>
        /// The Device parsed from the user agent string
        /// </summary>
        public Device Device { get; }

        /// <summary>
        /// The User Agent parsed from the user agent string
        /// </summary>
        public UserAgent UA { get; }
    }

    /// <summary>
    /// partial part for <see cref="UserAgentInfo"/>
    /// </summary>
    public partial class UserAgentInfo
    {
        /// <summary>
        /// Constructs an instance of the ClientInfo with results of the user agent string parsing
        /// </summary>
        public UserAgentInfo(string userAgentValue, OS os, Device device, UserAgent userAgent)
        {
            UserAgentValue = userAgentValue;
            OS = os;
            Device = device;
            UA = userAgent;
        }

        /// <summary>
        /// a boolean to check if the user is connecting from the website
        /// </summary>
        /// <returns>true if is coming from the website, false if not</returns>
        public bool IsWeb()
        {
            return true;
        }

        /// <summary>
        /// a boolean to check if the user is connecting from the mobile app
        /// </summary>
        /// <returns>true if is coming from the mobile app, false if not</returns>
        public bool IsMobile()
        {
            return true;
        }

        /// <summary>
        /// A readable description of the user agent client information
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{OS} {Device} {UA}";
    }
}
