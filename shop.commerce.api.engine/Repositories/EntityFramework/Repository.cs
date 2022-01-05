
namespace shop.commerce.api.infrastructure.Repositories.EntityFramework
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using shop.commerce.api.common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Utilities;

    /// <summary>
    /// an abstract implementation of the IDataSource interface
    /// </summary>
    /// <typeparam name="TEntity">the entity type</typeparam>
    /// <typeparam name="TKey">the entity key type</typeparam>
    public partial class Repository<TEntity, TKey>
    {
        #region Data manipulation operations

        /// <inheritdoc/>>
        public virtual Result<TEntity> Add(TEntity entity)
        {
            try
            {
                entity.InsertDate = DateTime.UtcNow;
                entity.LastUpdate = DateTime.UtcNow;
                entity.BuildSearchTerms();

                var entry = _entity.Add(entity);
                var operationResult = _context.SaveChanges();

                if (operationResult <= 0)
                    return Result.Failed<TEntity>(
                        $"failed adding the {Entity}, unknown reason",
                        ResultCode.OperationFailed);

                return entry.Entity;
            }
            catch (Exception ex)
            {
                return Result.Failed<TEntity>(
                    $"Failed adding the {Entity}s",
                    ResultCode.OperationFailedException,
                    ex);
            }
        }

        /// <inheritdoc/>>
        public virtual Result AddRange(IEnumerable<TEntity> entities)
        {
            try
            {
                foreach (var entity in entities)
                {
                    entity.InsertDate = DateTime.UtcNow;
                    entity.LastUpdate = DateTime.UtcNow;
                    entity.BuildSearchTerms();
                }

                _entity.AddRange(entities);
                var result = _context.SaveChanges();

                if (result <= 0)
                    return Result.Failed(
                        $"failed to add the range of {Entity}, unknown reason",
                        ResultCode.OperationFailed);

                return Result.ResultSuccess();
            }
            catch (Exception ex)
            {
                return Result.Failed(
                    $"Failed to add the {Entity}s",
                    ResultCode.OperationFailedException,
                    ex);
            }
        }

        /// <inheritdoc/>>
        public virtual Result UpdateRange(IEnumerable<TEntity> entities)
        {
            try
            {
                foreach (var entity in entities)
                {
                    entity.LastUpdate = DateTime.UtcNow;
                    entity.BuildSearchTerms();
                }

                _entity.UpdateRange(entities);
                var result = _context.SaveChanges();

                if (result <= 0)
                {
                    return Result.Failed($"failed to update the range of {Entity}, unknown reason", ResultCode.OperationFailed);
                }

                return Result.ResultSuccess();
            }
            catch (Exception ex)
            {
                return Result.Failed($"Failed to update the {Entity}s", ResultCode.OperationFailedException, ex);
            }
        }

        /// <inheritdoc/>>
        public virtual Result<TEntity> Update(TEntity entity)
        {
            try
            {
                entity.LastUpdate = DateTime.UtcNow;
                entity.BuildSearchTerms();

                var entry = _entity.Update(entity);
                var operationResult = _context.SaveChanges();

                if (operationResult <= 0)
                    return Result.Failed<TEntity>(
                        $"Failed to update the {Entity}",
                        ResultCode.OperationFailed);

                return entry.Entity;
            }
            catch (Exception ex)
            {
                return Result.Failed<TEntity>(
                    $"Failed to update the {Entity}",
                    ResultCode.OperationFailedException,
                    ex);
            }
        }

        /// <inheritdoc/>>
        public virtual Result<TEntity> Update(TKey entityId, Action<TEntity> entityUpdater)
        {
            // validate input
            if (entityUpdater is null)
                throw new ArgumentNullException(nameof(entityUpdater));

            // retrieve the entity to be updated
            var entity = GetById(entityId);
            if (entity is null)
                return Result.Failed<TEntity>($"{Entity} not exits", ResultCode.NotFound);

            // update the entity
            entityUpdater(entity);

            // persist the changes
            return Update(entity);
        }

        /// <inheritdoc/>>
        public virtual Result Delete(TKey id)
        {
            var entity = GetById(id);
            if (entity is null)
                return Result.Failed(
                    $"there is no entity with the id : {id}",
                    ResultCode.NotFound);

            return Delete(entity);
        }

        /// <inheritdoc/>>
        public virtual Result Delete(TEntity entity) => DeleteRange(entity);

        /// <inheritdoc/>>
        public virtual Result DeleteRange(params TEntity[] entities)
        {
            try
            {
                _entity.RemoveRange(entities);
                var operationResult = _context.SaveChanges();

                if (operationResult <= 0)
                {
                    return Result.Failed(
                        $"failed to delete {Entity}(s)",
                        ResultCode.OperationFailed);
                }

                return Result.ResultSuccess();
            }
            catch (Exception ex)
            {
                return Result.Failed(
                    $"Failed to delete the {Entity}s",
                    ResultCode.OperationFailedException,
                    ex);
            }
        }

        public int Save()
            => _context.SaveChanges();

        #endregion

        #region Data retrieval Operations

        /// <inheritdoc/>>
        public virtual TEntity GetById(TKey entityId)
            => GetSingle().SingleOrDefault(e => e.Id.Equals(entityId));

        /// <inheritdoc/>>
        public TEntity[] GetById(params TKey[] EntitiesIds)
            => GetList(null).Where(e => EntitiesIds.Contains(e.Id)).ToArray();

        /// <inheritdoc/>>
        public virtual TEntity GetFirst(Action<DataRequestBuilder<TEntity>> options)
            => GetSingle(GetDataRequest(options))
                .FirstOrDefault();

        /// <inheritdoc/>>
        public virtual TResult GetFirst<TResult>(Action<DataRequestBuilder<TEntity>> options, Expression<Func<TEntity, TResult>> selector)
            => GetSingle(GetDataRequest(options))
                .Select(selector)
                .FirstOrDefault();

        /// <inheritdoc/>>
        public virtual TEntity GetSingle(Action<DataRequestBuilder<TEntity>> options)
            => GetSingle(GetDataRequest(options))
                .SingleOrDefault();

        /// <inheritdoc/>>
        public virtual TResult GetSingle<TResult>(Action<DataRequestBuilder<TEntity>> options, Expression<Func<TEntity, TResult>> selector)
            => GetSingle(GetDataRequest(options))
                .Select(selector)
                .SingleOrDefault();

        /// <inheritdoc/>>
        public virtual TEntity[] GetAll(Action<DataRequestBuilder<TEntity>> options)
            => GetList(GetDataRequest(options)).ToArray();

        /// <inheritdoc/>>
        public virtual TResult[] GetAll<TResult>(Action<DataRequestBuilder<TEntity>> options, Expression<Func<TEntity, TResult>> selector)
            => GetList(GetDataRequest(options))
                .Select(selector)
                .ToArray();

        /// <inheritdoc/>>
        public virtual (TEntity[] data, int rowsCount) GetAll<IFilter>(IFilter filterOption)
            where IFilter : IFilterOptions
        {
            filterOption.SearchQuery = filterOption.SearchQuery ?? "";
            var query = GetList(null);

            if (filterOption.SearchQuery.IsValid())
                query = query.Where(e => e.SearchTerms.Contains(filterOption.SearchQuery.ToLower()));

            query = SetPagedResultFilterOptions(query, filterOption);
            query = SetOrderByCommand(query, filterOption);

            var rowsCount = query.Select(e => e.Id).Distinct().Count();

            if (!filterOption.IgnorePagination)
            {
                query = query
                    .Skip((filterOption.PageIndex - 1) * filterOption.PageSize)
                    .Take(filterOption.PageSize);
            }

            return (query.ToArray(), rowsCount);
        }

        /// <inheritdoc/>>
        public virtual TKey[] GetKeys()
            => _entity.Select(e => e.Id).ToArray();

        /// <inheritdoc/>>
        public virtual int GetCount(Expression<Func<TEntity, bool>> predicate)
            => _entity.AsNoTracking().Count(predicate);

        #endregion

        #region Check existence

        /// <inheritdoc/>>
        public virtual bool IsExist(Expression<Func<TEntity, bool>> predicate)
            => _entity.Any(predicate);

        /// <inheritdoc/>>
        public virtual bool IsExist(TKey entityId)
            => _entity.Any(e => e.Id.Equals(entityId));

        /// <inheritdoc/>>
        public bool IsAllkeysExist(params TKey[] keysToValidate)
        {
            var keys = GetKeys();
            return keysToValidate.All(keys.Contains);
        }

        #endregion
    }

    /// <summary>
    /// the partial part of <see cref="Repository{TEntity, TKey}"/>
    /// </summary>
    /// <typeparam name="TEntity">the type of the entity </typeparam>
    /// <typeparam name="TKey">the type of the primary key</typeparam>
    public partial class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {
        protected readonly DataRequestBuilder<TEntity> _dataRequestBuilder;
        protected readonly ShopContext _context;
        protected readonly ILogger _logger;

        /// <summary>
        /// the entity Db set
        /// </summary>
        protected readonly DbSet<TEntity> _entity;

        /// <summary>
        /// the entity name, this is just the name of the class
        /// </summary>
        public string Entity => typeof(TEntity).Name;

        /// <summary>
        /// default constructor that tacks the context
        /// </summary>
        /// <param name="context">the DataSource</param>
        public Repository(ShopContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _entity = _context.Set<TEntity>();
            _dataRequestBuilder = new DataRequestBuilder<TEntity>();
            _logger = loggerFactory.CreateLogger($"[{Entity}].DataAccess");
        }

        /// <summary>
        /// get the data request instant from the given action builder
        /// </summary>
        /// <param name="options">the action builder</param>
        /// <returns>the data request instant</returns>
        private DataRequest<TEntity> GetDataRequest(Action<DataRequestBuilder<TEntity>> options)
        {
            if (options is null)
                return null;

            options(_dataRequestBuilder);
            return _dataRequestBuilder.Build();
        }

        /// <summary>
        /// get the IQueryable of entities to query the entities as a single instant from the database using the passed in DataRequest
        /// </summary>
        /// <param name="request">the DataRequest object</param>
        /// <param name="defaultIncludes">whether to include the default Includes or not</param>
        /// <returns>IQueryable of entities</returns>
        protected IQueryable<TEntity> GetSingle(DataRequest<TEntity> request = null)
        {
            var query = _entity.AsNoTracking();
            if (request is null)
            {
                query = SetDefaultIncludsForSingleRetrieve(query);
                return query;
            }

            if (request.WithDefaultIncludes)
                query = SetDefaultIncludsForSingleRetrieve(query);

            return query.GetWithDataRequest(request);
        }

        /// <summary>
        /// get the IQueryable of entities to query the entities from the database using the
        /// passed in DataRequest
        /// </summary>
        /// <param name="request">the DataRequest object</param>
        /// <param name="defaultIncludes">whether to include the default Includes or not</param>
        /// <returns>IQueryable of entities</returns>
        protected IQueryable<TEntity> GetList(DataRequest<TEntity> request = null)
        {
            var query = _entity.AsNoTracking();
            if (request is null)
            {
                query = SetDefaultIncludsForListRetrieve(query);
                return query;
            }

            if (request.WithDefaultIncludes)
                query = SetDefaultIncludsForListRetrieve(query);

            return query.GetWithDataRequest(request);
        }

        /// <summary>
        /// this method is used to set the default includes of the entity when returning a list of entities
        /// </summary>
        /// <param name="query">the query instant</param>
        /// <returns>the IQueryble instant</returns>
        protected virtual IQueryable<TEntity> SetDefaultIncludsForListRetrieve(IQueryable<TEntity> query) => query;

        /// <summary>
        /// set the default includes for single entity retrieval ex: GetById etc ...
        /// </summary>
        /// <param name="query">the query instant</param>
        /// <returns>the IQueryble instant</returns>
        protected virtual IQueryable<TEntity> SetDefaultIncludsForSingleRetrieve(IQueryable<TEntity> query) => query;

        /// <summary>
        /// set the default filter option 
        /// </summary>
        /// <typeparam name="IFilter">the filter type</typeparam>
        /// <param name="query">the query instant</param>
        /// <param name="filterOption">the filter options instant</param>
        protected virtual IQueryable<TEntity> SetPagedResultFilterOptions<IFilter>(IQueryable<TEntity> query, IFilter filterOption)
            where IFilter : IFilterOptions => query;

        /// <summary>
        /// set the OrderBy Command, by defaults it uses the Dynamic builder
        /// </summary>
        /// <typeparam name="IFilter">the filter type</typeparam>
        /// <param name="query">the query instant</param>
        /// <param name="filterOption">the filter options instant</param>
        protected virtual IQueryable<TEntity> SetOrderByCommand<IFilter>(IQueryable<TEntity> query, IFilter filterOption)
            where IFilter : IFilterOptions => query.DynamicOrderBy(filterOption.OrderBy, filterOption.SortDirection);

    }
}
