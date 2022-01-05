namespace shop.commerce.api.common.Models
{
    /// <summary>
    /// a class that defines the filter option to get the data as paged list
    /// </summary>
    public class FilterOptions : IFilterOptions
    {
        /// <summary>
        /// the search query to search with it
        /// </summary>
        public string SearchQuery { get; set; } = string.Empty;

        /// <summary>
        /// the index of the page to retrieve
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// size of the page, how many records to include
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// a flag to indicate wither to ignore the pagination and get the full list or not
        /// </summary>
        public bool IgnorePagination { get; set; } = false;

        /// <summary>
        /// what property to order by with it
        /// </summary>
        public string OrderBy { get; set; } = nameof(Entity<object>.Id);

        /// <summary>
        /// the sort direction : Descending or Ascending
        /// </summary>
        public SortDirection SortDirection { get; set; } = SortDirection.Ascending;

        /// <summary>
        /// build the string reApplication of the object
        /// </summary>
        /// <returns>the string reApplication</returns>
        public override string ToString()
            => $"SearchQuery: '{SearchQuery}', Page: '{PageIndex}', Size: '{PageSize}', SortDirection: '{SortDirection}', OrderBy: '{OrderBy}'";
    }
}
