namespace shop.commerce.api.common
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// the List result implementation class
    /// </summary>
    /// <typeparam name="TResult">the type of the data to be returned</typeparam>
    [System.Diagnostics.DebuggerStepThrough]
    public partial class ListResult<TResult>
    {
        /// <summary>
        /// the count on the items in the list
        /// </summary>
        public int Count => Data?.Count() ?? 0;
    }

    /// <summary>
    /// the partial part for <see cref="ListResult{TResult}"/>
    /// </summary>
    /// <typeparam name="TResult">the type of the result value</typeparam>
    public partial class ListResult<TResult> : Result<TResult[]>
    {
        /// <summary>
        /// check if the given list empty
        /// </summary>
        public bool IsListEmpty() => Count <= 0;

        /// <summary>
        /// create an instant of <see cref="ListResult{TResult}"/> form the given result
        /// </summary>
        /// <param name="result">the result to clone</param>
        /// <returns>an instant on <see cref="ListResult{TResult}"/></returns>
        public static new ListResult<TResult> Clone(Result result)
            => new ListResult<TResult>()
            {
                Data = default,
                Code = result.Code,
                Status = result.Status,
                Message = result.Message,
                Errors = result.Errors,
            };

        /// <summary>
        /// get the string reApplication of the object
        /// </summary>
        /// <returns>the string value</returns>
        public override string ToString()
            => $"Status: {Status}, HasValue: {HasValue()}, Value: {Data.Count()} elements";

        #region Operators overrides

        public static implicit operator List<TResult>(ListResult<TResult> result)
            => result is null || !result.HasValue() ? (default) : result.Data as List<TResult>;

        public static implicit operator ListResult<TResult>(List<TResult> result)
            => ListSuccess(result.ToArray());

        public static implicit operator TResult[](ListResult<TResult> result)
            => result is null || !result.HasValue() ? (default) : result.Data.ToArray();

        public static implicit operator ListResult<TResult>(TResult[] result)
            => ListSuccess(result);

        #endregion
    }
}
