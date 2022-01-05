namespace shop.commerce.api.common
{
    using System;

    [System.Diagnostics.DebuggerStepThrough]
    public static class GeneralExtensions
    {
        /// <summary>
        /// this method is used to Encode the value string, only used to Encode the query string value
        /// </summary>
        /// <returns>the encoded parameter</returns>
        public static string Encode(this string value) => System.Web.HttpUtility.UrlEncode(value);

        /// <summary>
        /// this method is used to Encode the value string, only used to Encode the query string value
        /// </summary>
        /// <returns>the encoded parameter</returns>
        public static string Encode(this Guid value) => System.Web.HttpUtility.UrlEncode(value.ToString());

        /// <summary>
        /// this method is used to Encode the value string, only used to Encode the query string value
        /// </summary>
        /// <returns>the encoded parameter</returns>
        public static string Encode(this int value) => System.Web.HttpUtility.UrlEncode(value.ToString());
    }
}
