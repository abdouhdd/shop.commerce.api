using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace shop.commerce.api.domain.Common
{
    /// <summary>
    /// a global JSON Helper
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static class JsonHelper
    {
        /// <summary>
        /// Deserialize the given object to the specified generic type
        /// </summary>
        /// <typeparam name="TEntity">the type of the entity</typeparam>
        /// <param name="JsonObj">the json object to be deserialized</param>
        public static TEntity FromJson<TEntity>(this string JsonObj)
        {
            if (string.IsNullOrEmpty(JsonObj) || string.IsNullOrWhiteSpace(JsonObj))
                return default(TEntity);

            return JsonConvert.DeserializeObject<TEntity>(JsonObj);
        }

        /// <summary>
        /// serialize the given object to a JSON string
        /// </summary>
        /// <param name="obj">the object to be serialized</param>
        /// <returns>the string value of the object</returns>
        public static string ToJson<TEntity>(this TEntity obj, bool ignoreNull = false, bool Indented = false)
            where TEntity : class
        {
            if (obj is null)
                return string.Empty;

            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = ignoreNull ? NullValueHandling.Ignore : NullValueHandling.Include,
                Formatting = Indented ? Formatting.Indented : Formatting.None
            });
        }
    }
}
