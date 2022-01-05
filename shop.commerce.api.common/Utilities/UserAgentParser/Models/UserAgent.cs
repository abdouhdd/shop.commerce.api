namespace shop.commerce.api.common.Utilities
{
    /// <summary>
    /// Represents a user agent, commonly a browser
    /// </summary>
    public sealed class UserAgent
    {
        /// <summary>
        /// Construct a UserAgent instance
        /// </summary>
        public UserAgent(string family, string major, string minor, string patch)
        {
            Family = family;
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        /// <summary>
        /// The family of user agent
        /// </summary>
        public string Family { get; }

        /// <summary>
        /// Major version of the user agent, if available
        /// </summary>
        public string Major { get; }

        /// <summary>
        /// Minor version of the user agent, if available
        /// </summary>
        public string Minor { get; }

        /// <summary>
        /// Patch version of the user agent, if available
        /// </summary>
        public string Patch { get; }

        /// <summary>
        /// the full version of the agent
        /// </summary>
        public string Version
        {
            get
            {
                var version = StringExtensions.FormatVersion(Major, Minor, Patch);
                return version.IsValid() ? version : "";
            }
        }

        /// <summary>
        /// The user agent as a readable string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
            => $"{Family} {Version}".TrimEnd();
    }
}
