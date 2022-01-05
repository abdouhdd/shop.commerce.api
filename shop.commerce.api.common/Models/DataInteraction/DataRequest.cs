namespace shop.commerce.api.common
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// the data request class for querying data, to create an instant of this class
    /// use the <see cref="DataRequestBuilder{TEntity}"/> builder to build an instant
    /// </summary>
    /// <typeparam name="TEntity">the entity type</typeparam>
    [System.Diagnostics.DebuggerStepThrough]
    public class DataRequest<TEntity>
        where TEntity : class, IEntity
    {
        /// <summary>
        /// create an instant of <see cref="DataRequest"/>
        /// </summary>
        protected DataRequest()
        {
            IncludeDefinitions = new List<Expression<Func<TEntity, object>>>();
        }

        /// <summary>
        /// the query for the search term
        /// </summary>
        public string SearchQuery { get; set; }

        /// <summary>
        /// the Include definition
        /// </summary>
        public ICollection<Expression<Func<TEntity, object>>> IncludeDefinitions { get; set; }

        /// <summary>
        /// the predicate of the where query
        /// </summary>
        public Expression<Func<TEntity, bool>> Predicate { get; set; }

        /// <summary>
        /// the order by Key Selector
        /// </summary>
        public Expression<Func<TEntity, object>> OrderByKeySelector { get; set; }

        /// <summary>
        /// the OrderByDesc KeySelector
        /// </summary>
        public Expression<Func<TEntity, object>> OrderByDescKeySelector { get; set; }

        /// <summary>
        /// number of items to skip
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// number of items to limit at
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// true to use the default includes configured in the target entity dataAccess, defaulted to true
        /// </summary>
        public bool WithDefaultIncludes { get; set; } = true;

        /// <summary>
        /// a flag to ignore the query filter
        /// </summary>
        public bool IgnoreQueryFilter { get; set; }

        /// <summary>
        /// the string value of the object
        /// </summary>
        /// <returns>the string reApplication of the object</returns>
        public override string ToString()
        {
            return $"Query: {SearchQuery}" + Environment.NewLine +
                   $"Predicate: {Predicate?.ToString()}" +
                   $"OrderByKeySelector: {OrderByKeySelector?.ToString()}" +
                   $"OrderByDescKeySelector: {OrderByDescKeySelector?.ToString()}";
        }
    }
}
