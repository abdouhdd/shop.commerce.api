namespace shop.commerce.api.common
{
    /// <summary>
    /// the filter options requirements
    /// </summary>
    public interface IFilterOptions
    {
        /// <summary>
        /// property to order with it
        /// </summary>
        string OrderBy { get; set; }

        /// <summary>
        /// the page index
        /// </summary>
        int PageIndex { get; set; }

        /// <summary>
        /// the pageSize
        /// </summary>
        int PageSize { get; set; }

        /// <summary>
        /// the search query to search with it
        /// </summary>
        string SearchQuery { get; set; }

        /// <summary>
        /// the sort direction
        /// </summary>
        SortDirection SortDirection { get; set; }

        /// <summary>
        /// a flag to indicate wither to ignore the pagination and get the full list or not
        /// </summary>
        bool IgnorePagination { get; set; }
    }
}