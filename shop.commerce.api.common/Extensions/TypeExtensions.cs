namespace shop.commerce.api.common
{
    using System;
    using System.Linq;

    /// <summary>
    /// this class list all extension methods for <see cref="Type"/>
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static partial class TypeExtensions
    {
        /// <summary>
        /// check if the given type implement the given interface
        /// </summary>
        /// <typeparam name="T">the type of the interface to check</typeparam>
        /// <param name="type">the type to look in</param>
        /// <returns>true if it has the interface, false if not</returns>
        public static bool HasIntreface<T>(this Type type)
            where T : Attribute
        {
            return type
                .GetInterfaces().Any(x => x.IsGenericType &&
                  x.GetGenericTypeDefinition() == typeof(T));
        }

        /// <summary>
        /// check if the given enum type has the given value
        /// </summary>
        /// <typeparam name="EnumType">the type of the enum</typeparam>
        /// <param name="input">the <see cref="Enum"/> class</param>
        /// <param name="enumValue">the enum value to check</param>
        /// <returns>true if exist false if not</returns>
        public static bool HasValue<EnumType>(this Enum input, int enumValue)
            => Enum.GetValues(typeof(EnumType))
                .Cast<EnumType>()
                .ToList()
                .Select(v => Convert.ToInt32(v))
                .Contains(enumValue);

        /// <summary>
        /// check if the given enum type has the given value
        /// </summary>
        /// <typeparam name="EnumType">the type of the enum</typeparam>
        /// <param name="input">the <see cref="Enum"/> class</param>
        /// <param name="enumValue">the enum value to check</param>
        /// <returns>true if exist false if not</returns>
        public static bool HasValue<EnumType>(this Enum input, EnumType enumValue)
            => HasValue<EnumType>(input, Convert.ToInt32(enumValue));

        /// <summary>
        /// create an instant of the given option using the option builder
        /// </summary>
        /// <typeparam name="TOptions">the options</typeparam>
        /// <param name="optionsBuilder">the option builder instant</param>
        /// <returns>the instant of the options</returns>
        public static TOptions Create<TOptions>(this Action<TOptions> optionsBuilder) where TOptions : class
        {
            var options = ReflectionHelper.CreateInstantOf<TOptions>(Array.Empty<Type>(), Array.Empty<object>());
            optionsBuilder(options);
            return options;
        }

     
    }
}
