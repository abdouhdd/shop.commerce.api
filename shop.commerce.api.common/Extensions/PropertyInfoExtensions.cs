namespace shop.commerce.api.common
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// this class list all extension methods for <see cref="PropertyInfo"/>
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static class PropertyInfoExtensions
    {
        /// <summary>
        /// check if the given property has the given Attribute
        /// </summary>
        /// <typeparam name="T">the type of the attribute to check</typeparam>
        /// <param name="property">the property to look in</param>
        /// <returns>true if it has the attribute, false if not</returns>
        public static bool HasAttribute<T>(this PropertyInfo property)
            where T : Attribute
        {
            var complexTypeAttribute = property
               .GetCustomAttributes(typeof(T), true)
               .FirstOrDefault();

            if (complexTypeAttribute is null)
                return false;

            return true;
        }
    }
}
