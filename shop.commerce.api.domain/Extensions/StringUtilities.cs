using System;
using System.Text.RegularExpressions;

namespace shop.commerce.api.domain.Extensions
{
    public static class StringUtilities
    {
        public static string HtmlToPlainText(this string html)
        {
            string output;

            //get rid of HTML tags
            output = Regex.Replace(html, "<[^>]*>", string.Empty);

            //get rid of multiple blank lines
            output = Regex.Replace(output, @"^\s*$\n", string.Empty, RegexOptions.Multiline);

            return output;
        }
    }
}

