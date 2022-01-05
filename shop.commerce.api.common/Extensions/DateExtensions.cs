namespace shop.commerce.api.common
{
    using System;

    /// <summary>
    /// the date extensions
    /// </summary>
    public static class DateExtensions
    {
        /// <summary>
        /// get the starting date of the current date instant
        /// </summary>
        /// <param name="date">the current date instant</param>
        /// <returns>the start date value</returns>
        public static DateTime Start(this DateTime date) => date.AddDays(1 - date.Day);

        /// <summary>
        /// get the ending date of the current date instant
        /// </summary>
        /// <param name="date">the current date instant</param>
        /// <returns>the ending date of the date</returns>
        public static DateTime End(this DateTime date) => date.Start().AddMonths(1);

        /// <summary>
        /// get the last Month of the given date instant
        /// </summary>
        /// <param name="date">the date instant</param>
        /// <returns>the last month</returns>
        public static DateTime LastMonth(this DateTime date) => date.AddMonths(-1);

        /// <summary>
        /// check if the given date is between the two given date
        /// </summary>
        /// <param name="date">the date to compare</param>
        /// <param name="dateStart">the start date</param>
        /// <param name="dateEnd">the end date</param>
        /// <returns>true if between false if not</returns>
        public static bool IsBewteen(this DateTime date, DateTime dateStart, DateTime dateEnd)
            => dateStart <= date && date >= dateEnd;

        /// <summary>
        /// check if the given date is between the two given date
        /// </summary>
        /// <param name="date">the date to compare</param>
        /// <param name="dateStart">the start date</param>
        /// <param name="dateEnd">the end date</param>
        /// <returns>true if between false if not</returns>
        public static bool IsBewteen(this DateTimeOffset date, DateTimeOffset dateStart, DateTimeOffset dateEnd)
            => dateStart <= date && date >= dateEnd;

        /// <summary>
        /// the month difference between the two dates
        /// </summary>
        /// <param name="dateStart">the start date</param>
        /// <param name="dateEnd">the end date</param>
        /// <param name="getAbs">get the value as a valid positive integer</param>
        /// <returns>number of months</returns>
        public static int GetMonthsDiff(this DateTime dateStart, DateTime dateEnd, bool getAbs = true)
        {
            var value = (dateStart.Month - dateEnd.Month) + 12 * (dateStart.Year - dateEnd.Year);
            return getAbs ? Math.Abs(value) : value;
        }

        /// <summary>
        /// the month difference between the two dates
        /// </summary>
        /// <param name="dateStart">the start date</param>
        /// <param name="dateEnd">the end date</param>
        /// <param name="getAbs">get the value as a valid positive integer</param>
        /// <returns>number of months</returns>
        public static int GetMonthsDiff(this DateTimeOffset dateStart, DateTimeOffset dateEnd, bool getAbs = true)
        {
            var value = (dateStart.Month - dateEnd.Month) + 12 * (dateStart.Year - dateEnd.Year);
            return getAbs ? Math.Abs(value) : value;
        }

        /// <summary>
        /// generate a UNIX timestamps
        /// </summary>
        /// <param name="date">the date instant</param>
        /// <returns>the UNIX timestamps</returns>
        public static long UnixTimestamp(this DateTime date)
            => (long)date.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

        /// <summary>
        /// generate a UNIX timestamps
        /// </summary>
        /// <param name="date">the date instant</param>
        /// <returns>the UNIX timestamps</returns>
        public static long UnixTimestamp(this DateTimeOffset date)
            => (long)date.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                .TotalSeconds;
    }
}
