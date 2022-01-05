namespace shop.commerce.api.common
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// this class is used for all date functionalities
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static class Date
    {
        /// <summary>
        /// the date format used for couchDb id
        /// </summary>
        public const string TimestampFormat = "yyyyMMddHHmmssffff";

        /// <summary>
        /// get a Timestamp value of the current time
        /// </summary>
        public static string Timestamp => DateTime.Now.ToString(TimestampFormat);

        /// <summary>
        /// get Tomorrow date
        /// </summary>
        public static DateTime Tomorrow => DateTime.Today.AddDays(1);

        /// <summary>
        /// get Yesterday date
        /// </summary>
        public static DateTime Yesterday => DateTime.Today.AddDays(-1);

        /// <summary>
        /// get the start of this year
        /// </summary>
        public static DateTime ThisYearStart => new DateTime(DateTime.Now.Year, 01, 01);

        /// <summary>
        /// get the end of this year
        /// </summary>
        public static DateTime ThisYearEnd => new DateTime(DateTime.Now.Year, 12, 31);

        /// <summary>
        /// get dateTime value of the given timestamps
        /// </summary>
        /// <param name="timestamps">the timestamps value</param>
        /// <returns>the <see cref="DateTime"/> representation</returns>
        public static DateTime FromTimestamps(string timestamps)
            => timestamps.ToDateTime(TimestampFormat);

        /// <summary>
        /// compare the creation date with the expiration date,
        /// greater 0: date1 Greater than the Date2
        /// equals 0: are equals
        /// less than 0: date1 less than date2
        /// </summary>
        /// <param name="date1">the creation date</param>
        /// <param name="date2">the expiration date</param>
        /// <returns> the value of the compare method</returns>
        public static int Compare(string date1, string date2)
            => DateTime.Compare(DateTime.Parse(date1), DateTime.Parse(date2));

        /// <summary>
        /// compare the creation date with the expiration date,
        /// => greater than 0: date1 Greater than the Date2.
        /// => equals 0: are equals.
        /// => less than 0: date1 less than date2.
        /// </summary>
        /// <param name="date1">the creation date</param>
        /// <param name="date2">the expiration date</param>
        /// <returns>the value of the compare method</returns>
        public static int Compare(DateTime date1, string date2)
            => DateTime.Compare(date1, DateTime.Parse(date2));

        /// <summary>
        /// get the list of dates between the given start and end date, this will return the 1et of each month in every year between the given two dates
        /// </summary>
        /// <param name="startDate">the start date</param>
        /// <param name="endDate">the end date</param>
        /// <returns>the list of the dates</returns>
        public static IEnumerable<DateTime> GetDates(DateTime startDate, DateTime endDate)
        {
            // get the start month of the start date
            var start = new DateTime(startDate.Year, startDate.Month, 1);

            // create a date while we didn't reach the end date
            while (start <= endDate)
            {
                yield return start; // return the start date
                start = start.AddMonths(1); // then add a month
            }
        }

        /// <summary>
        /// check if the given date is expired
        /// </summary>
        /// <param name="dateToCheck">the date to be checked</param>
        /// <returns>true if expired, false if not</returns>
        public static bool IsExpired(DateTime dateToCheck)
            => DateTime.Today > dateToCheck;

        /// <summary>
        /// check if the date is between the given start date and end date
        /// </summary>
        /// <param name="date">the date to check</param>
        /// <param name="startDate">the start date</param>
        /// <param name="endDate">the end date</param>
        /// <returns>true if between, false if not</returns>
        public static bool IsBetween(DateTime date, DateTime startDate, DateTime endDate)
            => startDate <= date && date < endDate;

        /// <summary>
        /// check if the given date is between the start an the end of the current month
        /// </summary>
        /// <param name="date">the date value</param>
        /// <returns>true if between, false if not</returns>
        public static bool IsInCurrentMonth(DateTime date)
        {
            var startMonth = DateTime.Now.Start().Date;
            var endMonth = DateTime.Now.End().Date;
            return IsBetween(date, startMonth, endMonth);
        }

        /// <summary>
        /// check if the given date is between the start an the end of the last month
        /// </summary>
        /// <param name="date">the date value</param>
        /// <returns>true if between, false if not</returns>
        public static bool IsInLastMonth(DateTime date)
        {
            var startMonth = DateTime.Now.LastMonth().Start().Date;
            var endMonth = DateTime.Now.LastMonth().End().Date;
            return IsBetween(date, startMonth, endMonth);
        }
    }
}
