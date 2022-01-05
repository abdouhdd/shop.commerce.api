namespace shop.commerce.api.common
{
    using System;
    using System.Linq;

    /// <summary>
    /// the enum extension class
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// get the attribute value of the given type for the given enum
        /// </summary>
        /// <typeparam name="TAttribute">the type of the attribute</typeparam>
        /// <param name="value">the value of enum</param>
        /// <returns>the attribute if any, or null</returns>
        public static TAttribute GetAttribute<TAttribute>(this Enum value)
            where TAttribute : Attribute
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);

            if (name is null)
                throw new ArgumentException($"permission with value [{value}] not exist");

            return type.GetField(name)
                .GetCustomAttributes(false)
                .OfType<TAttribute>()
                .SingleOrDefault();
        }
    }
}
