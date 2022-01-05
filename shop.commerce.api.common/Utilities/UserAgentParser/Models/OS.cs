namespace shop.commerce.api.common.Utilities
{
    /// <summary>
    /// Represents the operating system the user agent runs on
    /// </summary>
    public sealed class OS
    {
        /// <summary>
        /// Constructs an OS instance
        /// </summary>
        public OS(string family, string major, string minor, string patch, string patchMinor)
        {
            Family = family;
            Major = major;
            Minor = minor;
            Patch = patch;
            PatchMinor = patchMinor;
        }

        /// <summary>
        /// The family of the OS
        /// </summary>
        public string Family { get; }

        /// <summary>
        /// The major version of the OS, if available
        /// </summary>
        public string Major { get; }

        /// <summary>
        /// The minor version of the OS, if available
        /// </summary>
        public string Minor { get; }

        /// <summary>
        /// The patch version of the OS, if available
        /// </summary>
        public string Patch { get; }

        /// <summary>
        /// The minor patch version of the OS, if available
        /// </summary>
        public string PatchMinor { get; }

        /// <summary>
        /// the full version of the agent
        /// </summary>
        public string Version => StringExtensions.FormatVersion(Major, Minor, Patch, PatchMinor);

        /// <summary>
        /// A readable description of the OS
        /// </summary>
        /// <returns></returns>
        public override string ToString()
            => $"{Family} {Version}".TrimEnd();
    }
}
