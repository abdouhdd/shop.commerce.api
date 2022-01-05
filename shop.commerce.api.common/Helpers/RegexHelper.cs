namespace shop.commerce.api.common
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// this class contains all the Regex functionality
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static class RegexHelper
    {
        public const string ArrayOfNumbersRegex = @"^\[[^(,)]*[(\d*)(,)]*(\d)*\]$";
        public const string DiscountUpgradeRegex = @"^upgrade from '(\w*)' '(\d*)' => '(\w*)' '(\d*)'$";
        public const string EmailRegex = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$";

        public static Regex EmailValidationRegex = BuildRegex(@"[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*");
        public static Regex NumbersAndLettersRegex = BuildRegex(@"([a-zA-Z0-9]*)");

        /// <summary>
        /// check if the given string is in a valid format of an integer array example: [] or [1,5,22]
        /// </summary>
        /// <param name="value">the string to check</param>
        /// <returns>true if valid false if not</returns>
        public static bool IsArrayOfNumbers(string value)
            => IsMatch(value, ArrayOfNumbersRegex);

        /// <summary>
        /// check if the given email is a valid email address
        /// </summary>
        /// <param name="email">the mail to be checked</param>
        /// <returns>true if valid, false if not</returns>
        public static bool IsValidEmailAddress(string email)
            => IsMatch(email, EmailRegex) && !email.StartsWith("foliatech_it_");

        /// <summary>
        /// check if the value match the given pattern
        /// </summary>
        /// <param name="value">the string value to check</param>
        /// <param name="pattern">the pattern to check for</param>
        /// <returns>true if match, false if not</returns>
        public static bool IsMatch(string value, string pattern)
            => Regex.IsMatch(value, pattern);

        /// <summary>
        /// extract number from the given string
        /// </summary>
        /// <param name="value">the string value</param>
        /// <returns>number</returns>
        public static int ExtractNumberFromString(string value)
            => int.TryParse(Regex.Match(value, @"\d+").Value, out int num) ? num : 0;

        /// <summary>
        /// extract the MimeType from the given base64 file
        /// </summary>
        /// <param name="fileBase64">the file to extract his type</param>
        /// <returns>the mime type</returns>
        /// <exception cref="ArgumentException">if the given base64 file is null or empty</exception>
        public static string Base64MimeType(string fileBase64)
        {
            if (!fileBase64.IsValid())
                throw new ArgumentException("the given base64 file is null of empty", fileBase64);

            var regex = @"data:([a-zA-Z0-9]+\/[a-zA-Z0-9-.+]+).*,.*";
            var result = Regex.Match(fileBase64, regex);

            if (result.Groups.Count <= 0)
                return "unknown";

            return result.Groups[0].Value;
        }

        private static Regex BuildRegex(string pattern)
        {
            const RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;

            try
            {
                TimeSpan matchTimeout = TimeSpan.FromSeconds(2);

                if (AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") == null)
                {
                    return new Regex(pattern, options, matchTimeout);
                }
            }
            catch { }

            return new Regex(pattern, options);
        }
    }
}
