namespace shop.commerce.api.common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// an implementation of <see cref="DataRequestBuilder"/>
    /// </summary>
    /// <typeparam name="TEntity">the entity we building the data request for</typeparam>
    [System.Diagnostics.DebuggerStepThrough]
    public class DataRequestBuilder<TEntity>
        where TEntity : class, IEntity
    {
        protected string SearchQuery;
        protected Expression<Func<TEntity, bool>> Predicate;
        protected Expression<Func<TEntity, object>> OrderByKeySelector;
        protected Expression<Func<TEntity, object>> OrderByDescKeySelector;
        protected ICollection<Expression<Func<TEntity, object>>> includeDefinitions;
        protected bool _ignoreDefaultIncludes = false;
        protected bool _ignoreQueryFilter = false;
        protected int? Limit = null;
        protected int? Skip = null;

        /// <summary>
        /// the default constructor
        /// </summary>
        public DataRequestBuilder()
        {
            includeDefinitions = new HashSet<Expression<Func<TEntity, object>>>();
        }

        /// <summary>
        /// set the data request to ignore default includes
        /// </summary>
        /// <returns>the builder</returns>
        public DataRequestBuilder<TEntity> IgnoreDefaultIncludes()
        {
            _ignoreDefaultIncludes = true;
            return this;
        }

        /// <summary>
        /// set the data request to ignore the query filter
        /// </summary>
        /// <returns>the builder</returns>
        public DataRequestBuilder<TEntity> IgnoreQueryFilter()
        {
            _ignoreQueryFilter = true;
            return this;
        }

        /// <summary>
        /// add the search query
        /// </summary>
        /// <param name="query">the search query</param>
        /// <returns>the builder</returns>
        public DataRequestBuilder<TEntity> AddQuery(string query)
        {
            SearchQuery = query;
            return this;
        }

        /// <summary>
        /// specify the entities to be included
        /// </summary>
        /// <param name="includes">the include expression</param>
        /// <returns>the builder</returns>
        public DataRequestBuilder<TEntity> AddInclude(Expression<Func<TEntity, object>> include)
        {
            includeDefinitions.Add(include);
            return this;
        }

        /// <summary>
        /// add the predicate query
        /// </summary>
        /// <param name="predicate">the predicate value</param>
        /// <returns>the builder</returns>
        public DataRequestBuilder<TEntity> AddPredicate(Expression<Func<TEntity, bool>> predicate)
        {
            Predicate = predicate;
            return this;
        }

        /// <summary>
        /// add the orderBy query
        /// </summary>
        /// <param name="orderByKeySelector">orderBy selector</param>
        /// <returns>the builder</returns>
        public DataRequestBuilder<TEntity> AddOrderBy(SortDirection orderBy, Expression<Func<TEntity, object>> orderByKeySelector)
        {
            switch (orderBy)
            {
                case SortDirection.Descending:
                    OrderByDescKeySelector = orderByKeySelector;
                    break;
                case SortDirection.Ascending:
                    OrderByKeySelector = orderByKeySelector;
                    break;
            }

            return this;
        }

        /// <summary>
        /// add the orderBy query
        /// </summary>
        /// <param name="orderByKeySelector">orderBy selector</param>
        /// <returns>the builder</returns>
        public DataRequestBuilder<TEntity> AddSkip(int skip)
        {
            Skip = skip;
            return this;
        }

        /// <summary>
        /// add the orderBy query
        /// </summary>
        /// <param name="orderByKeySelector">orderBy selector</param>
        /// <returns>the builder</returns>
        public DataRequestBuilder<TEntity> AddLimit(int limit)
        {
            Limit = limit;
            return this;
        }

        /// <summary>
        /// build and return the DataRequest instant
        /// </summary>
        /// <returns>DataRequest instant</returns>
        public virtual DataRequest<TEntity> Build()
        {
            var dataRequestInstant = ReflectionHelper
                .CreateInstantOf<DataRequest<TEntity>>(Array.Empty<Type>(), null);

            dataRequestInstant.Predicate = Predicate;
            dataRequestInstant.OrderByKeySelector = OrderByKeySelector;
            dataRequestInstant.IncludeDefinitions = includeDefinitions.ToArray();
            dataRequestInstant.OrderByDescKeySelector = OrderByDescKeySelector;
            dataRequestInstant.SearchQuery = SearchQuery;
            dataRequestInstant.Limit = Limit;
            dataRequestInstant.Skip = Skip;

            dataRequestInstant.WithDefaultIncludes = !_ignoreDefaultIncludes;
            dataRequestInstant.IgnoreQueryFilter = _ignoreQueryFilter;

            EmptyFields();
            return dataRequestInstant;
        }

        /// <summary>
        /// clear the values of the fields
        /// </summary>
        protected virtual void EmptyFields()
        {
            Skip = null;
            Limit = null;
            SearchQuery = null;
            Predicate = null;
            OrderByKeySelector = null;
            includeDefinitions.Clear();
            OrderByDescKeySelector = null;
            _ignoreDefaultIncludes = false;
            _ignoreQueryFilter = false;
        }
    }
}
