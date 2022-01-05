namespace shop.commerce.api.common.Utilities
{
    /// <summary>
    /// Options available for the parser
    /// </summary>
    public sealed class UserAgentParserOptions
    {
#if REGEX_COMPILATION
        /// <summary>
        /// If true, will use compiled regular expressions for slower startup time
        /// but higher throughput. The default is false.
        /// </summary>
        public bool UseCompiledRegex { get; set; }
#endif

#if REGEX_MATCHTIMEOUT
        /// <summary>
        /// Allows for specifying the maximum time spent on regular expressions,
        /// serving as a fail safe for potential infinite backtracking. The default is
        /// set to Regex.InfiniteMatchTimeout
        /// </summary>
        public TimeSpan MatchTimeOut { get; set; } = Regex.InfiniteMatchTimeout;
#endif
    }
}
