namespace shop.commerce.api.common.Utilities
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// a special switch to build a switch case statement
    /// built for types switching
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public class TypeSwitch
    {
        Dictionary<Type, Action> _matches = new Dictionary<Type, Action>();

        /// <summary>
        /// this method is used to add switch case statements
        /// </summary>
        /// <typeparam name="T">the type to switch for</typeparam>
        /// <param name="action">the action to execute when the type is matched</param>
        /// <returns>the current instant of the <see cref="TypeSwitch"/></returns>
        public TypeSwitch Case<T>(Action action)
        {
            _matches.Add(typeof(T), () => action());
            return this;
        }

        /// <summary>
        /// the switch statement
        /// </summary>
        /// <param name="x">the type to match</param>
        public void Switch(object x)
            => _matches[x.GetType()]();

        /// <summary>
        /// the switch statement
        /// </summary>
        /// <param name="type">the type to switch on it</param>
        public void Switch(Type type)
            => _matches[type]();

        /// <summary>
        /// the switch statement
        /// </summary>
        /// <param name="x">the type to match</param>
        public void Switch<Type>()
            => _matches[typeof(Type)]();
    }
}
