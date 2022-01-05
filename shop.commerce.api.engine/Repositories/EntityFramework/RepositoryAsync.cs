
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
    public partial class RepositoryAsync<TEntity, TKey>
    {
        #region Data manipulation operations

        /// <inheritdoc/>>
        public virtual async Task<Result<TEntity>> AddAsync(TEntity entity)
        {
            try
            {
                entity.InsertDate = DateTime.UtcNow;
                entity.LastUpdate = DateTime.UtcNow;
                entity.BuildSearchTerms();

                var entry = await _entity.AddAsync(entity);
                var operationResult = await _context.SaveChangesAsync();

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
        public virtual async Task<Result> AddRangeAsync(IEnumerable<TEntity> entities)
        {
            try
            {
                foreach (var entity in entities)
                {
                    entity.InsertDate = DateTime.UtcNow;
                    entity.LastUpdate = DateTime.UtcNow;
                    entity.BuildSearchTerms();
                }

                await _entity.AddRangeAsync(entities);
                var result = await _context.SaveChangesAsync();

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
        public virtual async Task<Result> UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            try
            {
                foreach (var entity in entities)
                {
                    entity.LastUpdate = DateTime.UtcNow;
                    entity.BuildSearchTerms();
                }

                _entity.UpdateRange(entities);
                var result = await _context.SaveChangesAsync();

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
        public virtual async Task<Result<TEntity>> UpdateAsync(TEntity entity)
        {
            try
            {
                entity.LastUpdate = DateTime.UtcNow;
                entity.BuildSearchTerms();

                var entry = _entity.Update(entity);
                var operationResult = await _context.SaveChangesAsync();

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
        public virtual async Task<Result<TEntity>> UpdateAsync(TKey entityId, Action<TEntity> entityUpdater)
        {
            // validate input
            if (entityUpdater is null)
                throw new ArgumentNullException(nameof(entityUpdater));

            // retrieve the entity to be updated
            var entity = await GetByIdAsync(entityId);
            if (entity is null)
                return Result.Failed<TEntity>($"{Entity} not exits", ResultCode.NotFound);

            // update the entity
            entityUpdater(entity);

            // persist the changes
            return await UpdateAsync(entity);
        }

        /// <inheritdoc/>>
        public virtual async Task<Result> DeleteAsync(TKey id)
        {
            var entity = await GetByIdAsync(id);
            if (entity is null)
                return Result.Failed(
                    $"there is no entity with the id : {id}",
                    ResultCode.NotFound);

            return await DeleteAsync(entity);
        }

        /// <inheritdoc/>>
        public virtual Task<Result> DeleteAsync(TEntity entity) => DeleteRangeAsync(entity);

        /// <inheritdoc/>>
        public virtual async Task<Result> DeleteRangeAsync(params TEntity[] entities)
        {
            try
            {
                _entity.RemoveRange(entities);
                var operationResult = await _context.SaveChangesAsync();

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

        #endregion

        #region Data retrieval Operations

        /// <inheritdoc/>>
        public virtual Task<TEntity> GetByIdAsync(TKey entityId)
            => GetSingle(null).SingleOrDefaultAsync(e => e.Id.Equals(entityId));

        /// <inheritdoc/>>
        public Task<TEntity[]> GetByIdAsync(params TKey[] EntitiesIds)
            => GetList(null).Where(e => EntitiesIds.Contains(e.Id)).ToArrayAsync();

        /// <inheritdoc/>>
        public virtual Task<TEntity> GetFirstAsync(Action<DataRequestBuilder<TEntity>> options)
            => GetSingle(GetDataRequest(options))
                .FirstOrDefaultAsync();

        /// <inheritdoc/>>
        public virtual Task<TResult> GetFirstAsync<TResult>(Action<DataRequestBuilder<TEntity>> options, Expression<Func<TEntity, TResult>> selector)
            => GetSingle(GetDataRequest(options))
                .Select(selector)
                .FirstOrDefaultAsync();

        /// <inheritdoc/>>
        public virtual Task<TEntity> GetSingleAsync(Action<DataRequestBuilder<TEntity>> options)
            => GetSingle(GetDataRequest(options))
                .SingleOrDefaultAsync();

        /// <inheritdoc/>>
        public virtual Task<TResult> GetSingleAsync<TResult>(Action<DataRequestBuilder<TEntity>> options, Expression<Func<TEntity, TResult>> selector)
            => GetSingle(GetDataRequest(options))
                .Select(selector)
                .SingleOrDefaultAsync();

        /// <inheritdoc/>>
        public virtual Task<TEntity[]> GetAllAsync(Action<DataRequestBuilder<TEntity>> options)
            => GetList(GetDataRequest(options)).ToArrayAsync();

        /// <inheritdoc/>>
        public virtual Task<TResult[]> GetAllAsync<TResult>(Action<DataRequestBuilder<TEntity>> options, Expression<Func<TEntity, TResult>> selector)
            => GetList(GetDataRequest(options))
                .Select(selector)
                .ToArrayAsync();

        /// <inheritdoc/>>
        public virtual async Task<(TEntity[] data, int rowsCount)> GetAllAsync<IFilter>(IFilter filterOption)
            where IFilter : IFilterOptions
        {
            filterOption.SearchQuery = filterOption.SearchQuery ?? "";
            var query = GetList(null);

            if (filterOption.SearchQuery.IsValid())
                query = query.Where(e => e.SearchTerms.Contains(filterOption.SearchQuery.ToLower()));

            query = SetPagedResultFilterOptions(query, filterOption);
            query = SetOrderByCommand(query, filterOption);

            var rowsCount = await query.Select(e => e.Id).Distinct().CountAsync();

            if (!filterOption.IgnorePagination)
            {
                query = query
                    .Skip((filterOption.PageIndex - 1) * filterOption.PageSize)
                    .Take(filterOption.PageSize);
            }

            return (await query.ToArrayAsync(), rowsCount);
        }

        /// <inheritdoc/>>
        public virtual Task<TKey[]> GetKeysAsync()
            => _entity.Select(e => e.Id).ToArrayAsync();

        /// <inheritdoc/>>
        public virtual Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate)
            => _entity.AsNoTracking().CountAsync(predicate);

        #endregion

        #region Check existence

        /// <inheritdoc/>>
        public virtual Task<bool> IsExistAsync(Expression<Func<TEntity, bool>> predicate)
            => _entity.AnyAsync(predicate);

        /// <inheritdoc/>>
        public virtual Task<bool> IsExistAsync(TKey entityId)
            => _entity.AnyAsync(e => e.Id.Equals(entityId));

        /// <inheritdoc/>>
        public async Task<bool> IsAllkeysExistAsync(params TKey[] keysToValidate)
        {
            var keys = await GetKeysAsync();
            return keysToValidate.All(keys.Contains);
        }

        #endregion
    }

    /// <summary>
    /// the partial part of <see cref="RepositoryAsync{TEntity, TKey}"/>
    /// </summary>
    /// <typeparam name="TEntity">the type of the entity </typeparam>
    /// <typeparam name="TKey">the type of the primary key</typeparam>
    public partial class RepositoryAsync<TEntity, TKey> : IRepositoryAsync<TEntity, TKey>
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
        public RepositoryAsync(ShopContext context, ILoggerFactory loggerFactory)
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
