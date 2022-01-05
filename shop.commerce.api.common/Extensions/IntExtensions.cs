namespace shop.commerce.api.common
{
    using System;

    /// <summary>
    /// int extensions
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static class NumbersExtensions
    {
        /// <summary>
        /// get how many numbers in the given integer
        /// </summary>
        /// <param name="value">the int value</param>
        /// <returns>the count of numbers</returns>
        public static int GetNumbersCount(this int value)
            => value.ToString().Replace("-", "").Length;

        /// <summary>
        /// increment the int value by the provided value
        /// </summary>
        /// <param name="value">the int value</param>
        /// <param name="incrementBy">the value to increment with it</param>
        /// <returns>the new value</returns>
        public static int Increment(this int value, int incrementBy = 1)
            => value += incrementBy;

        /// <summary>
        /// round the given number to a precision of 2 digits
        /// </summary>
        /// <param name="value">value to round</param>
        /// <returns>the rounded value</returns>
        public static float Round(this float value) => value.Round(2);

        /// <summary>
        /// round the given number to a precision of 2 digits
        /// </summary>
        /// <param name="value">value to round</param>
        /// <param name="rounding">the rounding value to round with it</param>
        /// <returns>the rounded value</returns>
        public static float Round(this float value, int rounding)
            => (float)Math.Round(value, rounding);

        /// <summary>
        /// multiply the given value to 100, and round the result using <see cref="Round(float)"/>
        /// </summary>
        /// <param name="price">the price to get the it cents value</param>
        /// <returns>the price at cent value</returns>
        public static long GetPriceInCents(this float price)
            => (long)(price * 100);

        /// <summary>
        /// this method is used to set the value to one if the given value is equals to zero
        /// </summary>
        /// <returns>the new value</returns>
        public static int SetToOne(this int value)
            => value <= 0 ? 1 : value;

        /// <summary>
        /// change the given int value to a boolean , if null false will be returned
        /// </summary>
        /// <param name="value">the value to change</param>
        /// <returns>the boolean value</returns>
        public static bool ToBoolean(this int? value)
        {
            if (value == null || value < 1)
                return false;

            return true;
        }

        public static float Sum(this (float val1, float val2) values)
            => values.val1 + values.val2;
    }
}
