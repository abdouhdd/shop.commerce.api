namespace shop.commerce.api.common
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// class for defining strings extensions
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static class StringExtensions
    {
        /// <summary>
        /// this method is used to append a time stamp to the given string
        /// in the form of: YearMonthDayHourMinuteSecondMillisecond => ex: 201912031632418386751
        /// </summary>
        /// <param name="value">the string to append with the time stamp</param>
        /// <returns>the new value of the string</returns>
        public static string AppendTimeStamp(this string value) => value += Date.Timestamp;

        /// <summary>
        /// this will append a random letters to the end of the given string
        /// </summary>
        /// <param name="value">the string value</param>
        /// <param name="length">the count of characters to append</param>
        /// <returns>the new string value</returns>
        public static string AppendRandomLetters(this string value, int length = 4)
        {
            return value + get_unique_string(length);

            string get_unique_string(int string_length)
            {
                using (var rng = new RNGCryptoServiceProvider())
                {
                    var bit_count = string_length * 6;
                    var byte_count = (bit_count + 7) / 8;
                    var bytes = new byte[byte_count];
                    rng.GetBytes(bytes);
                    return Convert.ToBase64String(bytes);
                }
            }
        }

        /// <summary>
        /// convert the string value to an int value, if the conversion failed -1 will be returned
        /// </summary>
        /// <param name="value">the string value to convert</param>
        /// <returns>the int value of the string</returns>
        public static int ToInt(this string value)
            => ToInt(value, -1);

        /// <summary>
        /// convert the string value to an int value, if the conversion failed default value will be returned
        /// </summary>
        /// <param name="value">the string value to convert</param>
        /// <param name="defaultValue">default value to return</param>
        /// <returns>the int value, or default value</returns>
        public static int ToInt(this string value, int defaultValue)
            => int.TryParse(value, out int result) ? result : defaultValue;

        /// <summary>
        /// convert the string value to an int value
        /// </summary>
        /// <param name="value">the string value to convert</param>
        /// <returns>the Guid value of the string</returns>
        public static Guid ToGuid(this string value) => Guid.Parse(value);

        /// <summary>
        /// parse the given string to a dateTime value
        /// </summary>
        /// <param name="date">the date string to be parsed</param>
        /// <param name="format">the format to be parsed to it</param>
        /// <returns>the date time value</returns>
        public static DateTime ToDateTime(this string date, string format )
            => DateTime.ParseExact(date, format, CultureInfo.InvariantCulture);

        /// <summary>
        /// parse the given string to a dateTime value
        /// </summary>
        /// <param name="date">the date string to be parsed</param>
        /// <param name="format">the format to be parsed to it</param>
        /// <returns>the date time value</returns>
        private static DateTime ToDateTimeOrNow(string date, string format)
        {
            var dateValue = date.ToDateTimeIfValid(format);

            return dateValue is null
                 ? DateTime.Now
                 : dateValue.Value;
        }

        /// <summary>
        /// parse the given string to a dateTime value
        /// </summary>
        /// <param name="date">the date string to be parsed</param>
        /// <param name="format">the format to be parsed to it</param>
        /// <returns>the date time value</returns>
        public static DateTime? ToDateTimeIfValid(this string date, string format)
        {
            try
            {
                return DateTime.ParseExact(date, format, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// convert the given string value to base 64
        /// </summary>
        /// <param name="value">the string value to be encoded</param>
        /// <returns>the base64 encoding</returns>
        public static string ToBase64(this string value)
            => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

        /// <summary>
        /// make the first letter of the string upper case
        /// </summary>
        /// <param name="value">the string value</param>
        /// <returns>the new string</returns>
        public static string ToUpperFirstChar(this string value)
        {
            switch (value)
            {
                case null: throw new ArgumentNullException(nameof(value));
                case "": throw new ArgumentException($"{nameof(value)} cannot be empty", nameof(value));
                default: return value.First().ToString().ToUpper() + value.Substring(1);
            }
        }

        /// <summary>
        /// make the first letter of the string lower case
        /// </summary>
        /// <param name="value">the string value</param>
        /// <returns>the new string</returns>
        public static string ToLowerFirstChar(this string value)
        {
            switch (value)
            {
                case null: throw new ArgumentNullException(nameof(value));
                case "": throw new ArgumentException($"{nameof(value)} cannot be empty", nameof(value));
                default: return value.First().ToString().ToLower() + value.Substring(1);
            }
        }

        /// <summary>
        /// build the string representation of the given array of expressions
        /// </summary>
        /// <typeparam name="TIn">the type of the input</typeparam>
        /// <typeparam name="TOut">the type of the output</typeparam>
        /// <param name="expressions">the expression instant</param>
        /// <returns>the string value</returns>
        public static string Stringify<TIn, TOut>(this IEnumerable<Expression<Func<TIn, TOut>>> expressions)
        {
            if (expressions is null)
                return "";

            var value = "";
            foreach (var expression in expressions)
            {
                value += expression.ToString() + Environment.NewLine;
            }
            return value;
        }

        /// <summary>
        /// ensure that we return a default value in case that the given string is null, empty or whitespace
        /// </summary>
        /// <param name="value">the string value</param>
        /// <param name="defaultValue">the default value</param>
        /// <returns>the string</returns>
        public static string EnsureValue(this string value, string defaultValue = "")
            => value.IsValid() ? value : defaultValue;

        /// <summary>
        /// extract the number from the given string ex: "test12" => 12
        /// </summary>
        /// <param name="value">the string to extract the value from it</param>
        /// <returns>the number as int</returns>
        public static int ExtractNumber(this string value)
            => RegexHelper.ExtractNumberFromString(value);

        /// <summary>
        /// replace the first Occurrence of the search by the given replacement
        /// </summary>
        /// <param name="input">the input to modify</param>
        /// <param name="search">the string to search for</param>
        /// <param name="replacement">the replacement value</param>
        /// <returns>the new string</returns>
        public static string ReplaceFirstOccurrence(this string input, string search, string replacement)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            var index = input.IndexOf(search, StringComparison.Ordinal);
            return index >= 0
                 ? input.Substring(0, index) + replacement + input.Substring(index + search.Length)
                 : input;
        }

        /// <summary>
        /// format the given parts to a version string ["1","10", "1001"] => "1.10.1001"
        /// </summary>
        /// <param name="parts">the parts to join</param>
        /// <returns>the version format</returns>
        public static string FormatVersion(params string[] parts)
           => string.Join(".", parts.Where(v => !string.IsNullOrEmpty(v)).ToArray());

        /// <summary>
        /// Fix Special Characters Escaping, example "test \" test" => "test \\" test"
        /// </summary>
        /// <param name="value">the string value</param>
        /// <returns>the new string version</returns>
        public static string FixSpecialCharactersEscaping(this string value)
            => value.Replace("\"", "\\\"");

        /// <summary>
        /// split the given string value into multiple chunks
        /// </summary>
        /// <param name="value">the value of the string</param>
        /// <param name="chunkSize">the chunk size</param>
        /// <returns>list of chunks</returns>
        public static string[] ChunkSplit(this string value, int chunkSize)
            => Enumerable.Range(0, value.Length / chunkSize)
                .Select(i => value.Substring(i * chunkSize, chunkSize))
                .ToArray();

        /// <summary>
        /// check if the given string is not null or empty or white space
        /// </summary>
        /// <param name="value">the string value to be checked</param>
        /// <returns>true if valid, false if not</returns>
        public static bool IsValid(this string value)
            => !(string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value));

        /// <summary>
        /// check if the given guid value is a valid guid
        /// </summary>
        /// <param name="guidValue">the guid value</param>
        /// <returns>true if valid false if not</returns>
        public static bool IsAValidGuid(this string guidValue)
            => Guid.TryParse(guidValue, out Guid value);

        /// <summary>
        /// convert the string into an enum
        /// </summary>
        /// <typeparam name="TEnum">the enum type to convert to it</typeparam>
        /// <param name="value">the string value</param>
        /// <returns>the enum type</returns>
        public static TEnum? ToEnum<TEnum>(this string value) where TEnum : struct
        {
            if (Enum.TryParse<TEnum>(value, out TEnum enumValue))
                return enumValue;

            return null;
        }
    }
}
