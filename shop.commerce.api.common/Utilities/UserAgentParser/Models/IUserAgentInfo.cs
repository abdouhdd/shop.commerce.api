namespace shop.commerce.api.common.Utilities
{
    /// <summary>
    /// Representing the parse results. Structure of this class aligns with the
    /// ua-parser-output WebIDL structure defined in this document: https://github.com/ua-parser/uap-core/blob/master/docs/specification.md
    /// </summary>
    public interface IUserAgentInfo
    {
        /// <summary>
        /// The user agent string, the input for the UAParser
        /// </summary>
        string UserAgentValue { get; }

        /// <summary>
        /// The OS parsed from the user agent string
        /// </summary>
        OS OS { get; }

        /// <summary>
        /// The Device parsed from the user agent string
        /// </summary>
        Device Device { get; }

        /// <summary>
        /// The User Agent parsed from the user agent string
        /// </summary>
        UserAgent UA { get; }
    }
}
