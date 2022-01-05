namespace shop.commerce.api.common
{
    using System;
    using System.Reflection;

    /// <summary>
    /// a class the holds Reflection related helpers
    /// </summary>
    //[System.Diagnostics.DebuggerStepThrough]
    public static class ReflectionHelper
    {
        /// <summary>
        /// use this method to create an instant of any object that has a private or protected constructor
        /// </summary>
        /// <typeparam name="TInstant">the type of the instant you want to create</typeparam>
        /// <param name="paramsType">the constructor parameters type, must be in order</param>
        /// <param name="paramsValues">the value you want to pass, must be in order</param>
        public static TInstant CreateInstantOf<TInstant>(Type[] paramsType, object[] paramsValues)
        {
            Type dataRequestType = typeof(TInstant);

            ConstructorInfo constructor = dataRequestType.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null, paramsType, null);

            return (TInstant)constructor.Invoke(paramsValues);
        }

        /// <summary>
        /// get the property value on the given entity
        /// </summary>
        /// <typeparam name="TOut">the output type</typeparam>
        /// <param name="entity">the entity instant</param>
        /// <param name="propretyName">the name of the property</param>
        /// <param name="defaultValue">the default value to be returned</param>
        /// <param name="throwIfNotExist">throw exception if the property not exist</param>
        /// <returns>the desired value</returns>
        public static TOut GetProprety<TOut>(this object entity, string propretyName, TOut defaultValue = default, bool throwIfNotExist = false)
        {
            // try to get the value
            var propertyInfo = entity.GetProprety(propretyName, throwIfNotExist);

            // check if the property exist
            if (propertyInfo is null)
                return defaultValue;

            // get the value and try to convert it
            var value = propertyInfo.GetValue(entity);
            return (TOut)Convert.ChangeType(value, typeof(TOut));
        }

        /// <summary>
        /// get the property value on the given entity
        /// </summary>
        /// <typeparam name="TOut">the output type</typeparam>
        /// <param name="entity">the entity instant</param>
        /// <param name="propretyName">the name of the property</param>
        /// <param name="defaultValue">the default value to be returned</param>
        /// <param name="throwIfNotExist">throw exception if the property not exist</param>
        /// <returns>the desired value</returns>
        public static PropertyInfo GetProprety(this object entity, string propretyName, bool throwIfNotExist = false)
        {
            // try to get the value
            var propertyInfo = entity.GetType().GetProperty(propretyName);

            // check if the property exist
            if (propertyInfo is null && throwIfNotExist)
                throw new ArgumentException("the property not exist");

            // get the value and try to convert it
            return propertyInfo;
        }

        /// <summary>
        /// check if the type has the propriety with the given name
        /// </summary>
        /// <param name="type">the type to be checked</param>
        /// <param name="propretyName">the property name</param>
        /// <returns>true if exist, false if not</returns>
        public static bool HasProprety(this Type type, string propretyName)
            => !(type.GetProperty(propretyName) is null);
    }
}
