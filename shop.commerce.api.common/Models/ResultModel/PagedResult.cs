namespace shop.commerce.api.common
{
    using System.Collections.Generic;

    /// <summary>
    /// the paged result implementation class
    /// </summary>
    /// <typeparam name="TResult">the type of the data to be returned</typeparam>
    [System.Diagnostics.DebuggerStepThrough]
    public partial class PagedResult<TResult> : ListResult<TResult>
    {
        /// <summary>
        /// the index of the current selected page
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// the page size, the desired number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// the total count of all items
        /// </summary>
        public int TotalRows { get; set; }

        /// <summary>
        /// the total count of all pages
        /// </summary>
        public int PagesCount => GetPageCount();
    }

    /// <summary>
    /// partial part for <see cref="PagedResult{TResult}"/>
    /// </summary>
    /// <typeparam name="TResult">the type of the data to be returned</typeparam>
    public partial class PagedResult<TResult>
    {
        /// <summary>
        /// calculate skip value
        /// </summary>
        public int GetSkipValue() => CalculateSkip(PageIndex, PageSize);

        /// <summary>
        /// create an instant of <see cref="PagedResult{TResult}"/> form the given result
        /// </summary>
        /// <param name="result">the result to clone</param>
        /// <returns>an instant on <see cref="PagedResult{TResult}"/></returns>
        public static new PagedResult<TResult> Clone(Result result)
            => new PagedResult<TResult>()
            {
                Data = default,
                Code = result.Code,
                Errors = result.Errors,
                Status = result.Status,
                Message = result.Message,
                PageSize = 10,
                TotalRows = 0,
                PageIndex = 1,
            };

        /// <summary>
        /// this method is used to calculate the skip and page size
        /// </summary>
        /// <returns>skip and page size</returns>
        public static (int skip, int pageCount) CalculateSkipAndPageSize(int ItemsCount, int page, int pageSize)
        {
            var pageCount = (int)System.Math.Ceiling((decimal)ItemsCount / pageSize);
            var skip = (page - 1) * pageSize;

            return (skip, pageCount);
        }

        /// <summary>
        /// this method is used to calculate the skip value
        /// </summary>
        /// <returns>skip value</returns>
        public static int CalculateSkip(int page, int pageSize)
            => (page - 1) * pageSize;

        /// <summary>
        /// get the page count
        /// </summary>
        /// <returns>the page count</returns>
        public int GetPageCount()
        {
            if (TotalRows <= 0)
                return 0;

            if (PageSize <= 0)
                return 0;

            return (int)System.Math.Ceiling((decimal)TotalRows / PageSize);
        }

        #region Operators overrides

        public static implicit operator List<TResult>(PagedResult<TResult> result)
            => result is null || !result.HasValue() ? (default) : result.Data as List<TResult>;

        #endregion
    }
}
