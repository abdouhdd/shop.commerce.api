namespace shop.commerce.api.infrastructure.Repositories
{
    using shop.commerce.api.common;
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// represent the DataAcsess implementation
    /// </summary>
    /// <typeparam name="TEntity">the type of the entity, must implement <see cref="ISearchable"/>, <see cref="IRecordable"/> and <see cref="IPrimaryKey{TKey}"/></typeparam>
    /// <typeparam name="TKey">the type of the entity Id</typeparam>
    public interface IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {
        /// <summary>
        /// the name of the entity, this is just the Name of the class
        /// </summary>
        string Entity { get; }

        /// <summary>
        /// add the Entity to the underlying database
        /// </summary>
        /// <param name="entity">the Entity to be add</param>
        /// <param name="setDefaultData">true to set the default data (createdOn, lastModifiedOn, and buildSearchTerms)</param>
        /// <returns>the new added entity</returns>
        Result<TEntity> Add(TEntity entity);

        /// <summary>
        /// add the list of entities to the underlying database
        /// </summary>
        /// <param name="entities">the Entity to be add</param>
        /// <returns>the result of the operation</returns>
        Result AddRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// update list of entities at ones
        /// </summary>
        /// <param name="entities">list of entities to be updated</param>
        /// <returns>the updated result</returns>
        Result UpdateRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// update the given model
        /// </summary>
        /// <param name="model">model to be updated</param>
        /// <returns>the updated model</returns>
        Result<TEntity> Update(TEntity model);

        /// <summary>
        /// use this method to update the entity without retrieving it
        /// </summary>
        /// <param name="entityId">the id of the entity to be updated</param>
        /// <param name="entityUpdater">the entity updater function</param>
        /// <returns>the operation result</returns>
        Result<TEntity> Update(TKey entityId, Action<TEntity> entityUpdater);

        /// <summary>
        /// delete the entity with the given id
        /// </summary>
        /// <param name="id">the id of the entity to be deleted</param>
        /// <returns>an operation result</returns>
        Result Delete(TKey id);

        /// <summary>
        /// delete the given Entity
        /// </summary>
        /// <param name="entity">the Entity to be deleted</param>
        /// <returns>a boolean true if deleted</returns>
        Result Delete(TEntity entity);

        /// <summary>
        /// delete the given Entities
        /// </summary>
        /// <param name="Entities">the Entities to be deleted</param>
        /// <returns>a boolean true if deleted</returns>
        Result DeleteRange(params TEntity[] Entities);

        /// <summary>
        /// get the first result using the given data request options
        /// </summary>
        /// <param name="request">Data Request builder options</param>
        /// <returns>the result</returns>
        TEntity GetFirst(Action<DataRequestBuilder<TEntity>> options);

        /// <summary>
        /// get the first result using the given data request options
        /// </summary>
        /// <typeparam name="TResult">the type of the output</typeparam>
        /// <param name="options">the options builder</param>
        /// <param name="selector">the selector to select the desired out put</param>
        /// <returns>the result</returns>
        TResult GetFirst<TResult>(Action<DataRequestBuilder<TEntity>> options, Expression<Func<TEntity, TResult>> selector);

        /// <summary>
        /// get a single result using the given data request
        /// </summary>
        /// <param name="request">Data Request instant</param>
        /// <returns>a single result</returns>
        TEntity GetSingle(Action<DataRequestBuilder<TEntity>> options);

        /// <summary>
        /// get a single result using the given data request options
        /// </summary>
        /// <typeparam name="TResult">the type of the output</typeparam>
        /// <param name="options">the options builder</param>
        /// <param name="selector">the selector to select the desired out put</param>
        /// <returns>a single result</returns>
        TResult GetSingle<TResult>(Action<DataRequestBuilder<TEntity>> options, Expression<Func<TEntity, TResult>> selector);

        /// <summary>
        /// get the a list result using the given data request options
        /// </summary>
        /// <param name="options">the data request builder options</param>
        /// <returns>a list result of a given entity</returns>
        TEntity[] GetAll(Action<DataRequestBuilder<TEntity>> options = null);

        /// <summary>
        /// get the a list result using the given data request options
        /// </summary>
        /// <param name="options">the data request builder options</param>
        /// <param name="selector">the selector to select the desired out put</param>
        /// <returns>a list result of a given entity</returns>
        TResult[] GetAll<TResult>(Action<DataRequestBuilder<TEntity>> options, Expression<Func<TEntity, TResult>> selector);

        /// <summary>
        /// get list of data using the given filter and return the total count of rows
        /// </summary>
        /// <typeparam name="IFilter">the type of the filter to use</typeparam>
        /// <param name="filterOption">the filter instant</param>
        /// <returns>the data result and, total count of rows</returns>
        (TEntity[] data, int rowsCount) GetAll<IFilter>(IFilter filterOption)
            where IFilter : IFilterOptions;

        /// <summary>
        /// get the model with the given id
        /// </summary>
        /// <param name="modelId">the id of the model to get</param>
        /// <returns>the model</returns>
        TEntity GetById(TKey modelId);

        /// <summary>
        /// get all the entities with the given ids
        /// </summary>
        /// <param name="EntitiesIds">the ids of the entities to retrieve</param>
        /// <returns>the list of entities</returns>
        TEntity[] GetById(params TKey[] EntitiesIds);

        /// <summary>
        /// get the count of items in dataSource using the given predicate
        /// </summary>
        /// <param name="predicate">the predicate to retrieve the count by it</param>
        int GetCount(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// check if there is any entity that matches the given predicate
        /// </summary>
        /// <param name="predicate">the predicate to be evaluated</param>
        /// <returns>true if exist, false if not</returns>
        bool IsExist(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// check if the entity with the given id is exist
        /// </summary>
        /// <param name="id">the id of the entity</param>
        /// <returns>true if exist, false if not</returns>
        bool IsExist(TKey id);

        /// <summary>
        /// validate if all <see cref="TEntity"/> keys exist or not
        /// </summary>
        /// <param name="keysToValidate">the list of keys to validate</param>
        /// <returns>true if all exist, false if not</returns>
        bool IsAllkeysExist(params TKey[] keysToValidate);

        /// <summary>
        /// save data changes
        /// </summary>
        /// <returns>true if changes is affected to database</returns>
        int Save();
    }
}
