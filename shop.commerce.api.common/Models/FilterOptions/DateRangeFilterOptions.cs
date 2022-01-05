namespace shop.commerce.api.common.Models
{
    using System;

    /// <summary>
    /// filter options that defines the date ranger lookup
    /// </summary>
    public partial class DateRangeFilterOptions : FilterOptions
    {
        /// <summary>
        /// the start date
        /// </summary>
        public DateTime? DateStart { get; set; }

        /// <summary>
        /// the end date
        /// </summary>
        public DateTime? DateEnd { get; set; }
    }
}
