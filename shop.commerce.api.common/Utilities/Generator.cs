namespace shop.commerce.api.common.Utilities
{
    using System;
    using System.Threading;

    /// <summary>
    /// use this class to generate unique ids
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static partial class Generator
    {
        /// <summary>
        /// use this method to generate a Random id
        /// </summary>
        /// <remarks>
        /// we generate the id base on a Guid value by converting it to base64 and removing special characters
        /// </remarks>
        /// <returns>the generated id</returns>
        public static string GenerateRandomId()
            => Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("=", "").Replace("+", "").Replace("/", "").Replace("\\", "");

        /// <summary>
        /// generate a unique tread safe id
        /// </summary>
        /// <returns>a unique string id</returns>
        public static string GenerateUniqueId() => GenerateId(Interlocked.Increment(ref _lastId));

        /// <summary>
        /// Generates a random password based on the rules passed in the parameters
        /// </summary>
        /// <param name="options">the <see cref="PasswordOptions"/> is null the default one will be tacken</param>
        /// <returns></returns>
        public static string GeneratePassword(PasswordOptions options = null)
        {
            if (options is null)
                options = PasswordOptions.Default;

            if (options.Passwordlength < PasswordOptions.LENGTH_MIN || options.Passwordlength > PasswordOptions.LENGTH_MAX)
            {
                return "Password length must be between 8 and 128.";
            }

            string characterSet = "";

            if (options.IncludeLowercase)
                characterSet += PasswordOptions.LOWERCASE_CHARACTERS;

            if (options.IncludeUppercase)
                characterSet += PasswordOptions.UPPERCASE_CHARACTERS;

            if (options.IncludeNumeric)
                characterSet += PasswordOptions.NUMERIC_CHARACTERS;

            if (options.IncludeSpecial)
                characterSet += PasswordOptions.SPECIAL_CHARACTERS;

            char[] password = new char[options.Passwordlength];
            int characterSetLength = characterSet.Length;

            var random = new Random();
            for (int characterPosition = 0; characterPosition < options.Passwordlength; characterPosition++)
            {
                password[characterPosition] = characterSet[random.Next(characterSetLength - 1)];

                bool moreThanTwoIdenticalInARow =
                    characterPosition > PasswordOptions.MAXIMUM_IDENTICAL_CONSECUTIVE_CHARS
                    && password[characterPosition] == password[characterPosition - 1]
                    && password[characterPosition - 1] == password[characterPosition - 2];

                if (moreThanTwoIdenticalInARow)
                {
                    characterPosition--;
                }
            }

            return string.Join(null, password);
        }

        /// <summary>
        /// generate a random error code for log tracing
        /// </summary>
        /// <returns>the generated error code</returns>
        public static string GenerateLogTraceErrorCode() => GenerateRandomId();
    }

    /// <summary>
    /// a partial part for <see cref="Generator"/>
    /// </summary>
    public static partial class Generator
    {
        /// <summary>
        /// password generation options
        /// </summary>
        public class PasswordOptions
        {
            public const string SPECIAL_CHARACTERS = @"!#$%&*@\";
            public const string NUMERIC_CHARACTERS = "0123456789";
            public const string LOWERCASE_CHARACTERS = "abcdefghijklmnopqrstuvwxyz";
            public const string UPPERCASE_CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            public const int MAXIMUM_IDENTICAL_CONSECUTIVE_CHARS = 2;
            public const int LENGTH_MAX = 128;
            public const int LENGTH_MIN = 8;

            /// <summary>
            /// Include Lowercase
            /// </summary>
            public bool IncludeLowercase { get; set; } = true;

            /// <summary>
            /// Include Uppercase
            /// </summary>
            public bool IncludeUppercase { get; set; } = true;

            /// <summary>
            /// Include Numeric
            /// </summary>
            public bool IncludeNumeric  { get; set; } = true;

            /// <summary>
            /// Include Special
            /// </summary>
            public bool IncludeSpecial  { get; set; } = true;

            /// <summary>
            /// set Password length
            /// </summary>
            public int Passwordlength { get; set; } = 15;

            /// <summary>
            /// get default instant
            /// </summary>
            public static PasswordOptions Default = new PasswordOptions();
        }

        private static readonly string _encode_32_Chars = "0123456789abcdefghijklmnopqrstuv";

        private static long _lastId = DateTime.UtcNow.Ticks;

        private static readonly ThreadLocal<char[]> _buffer = new ThreadLocal<char[]>(() => new char[13]);

        private static string GenerateId(long id)
        {
            var buffer = _buffer.Value;

            buffer[0] = 'c';
            buffer[1] = _encode_32_Chars[(int)(id >> 55) & 31];
            buffer[2] = _encode_32_Chars[(int)(id >> 50) & 31];
            buffer[3] = _encode_32_Chars[(int)(id >> 45) & 31];
            buffer[4] = _encode_32_Chars[(int)(id >> 40) & 31];
            buffer[5] = _encode_32_Chars[(int)(id >> 35) & 31];
            buffer[6] = _encode_32_Chars[(int)(id >> 30) & 31];
            buffer[7] = _encode_32_Chars[(int)(id >> 25) & 31];
            buffer[8] = _encode_32_Chars[(int)(id >> 20) & 31];
            buffer[9] = _encode_32_Chars[(int)(id >> 15) & 31];
            buffer[10] = _encode_32_Chars[(int)(id >> 10) & 31];
            buffer[11] = _encode_32_Chars[(int)(id >> 5) & 31];
            buffer[12] = _encode_32_Chars[(int)id & 31];

            return new string(buffer, 0, buffer.Length).ToLower();
        }
    }
}
