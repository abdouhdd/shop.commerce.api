

namespace shop.commerce.api.infrastructure.Utilities
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;
    using shop.commerce.api.common;
    using shop.commerce.api.infrastructure.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Utilities class
    /// </summary>
    internal static class Utilities
    {
        #region Entity FrameWork extensions

        /// <summary>
        /// Get the result as an IEnumerable
        /// </summary>
        /// <typeparam name="TEntity">the type of the entity</typeparam>
        /// <param name="query">the IQueryable to be executed</param>
        /// <returns>IEnumerable</returns>
        public static async Task<IEnumerable<TEntity>> ToIEnumerableAsync<TEntity>(this IQueryable<TEntity> query)
            => await query.ToListAsync();


        /// <summary>
        /// get the IQueryable of entities filtered by the DataRequest
        /// </summary>
        /// <returns>IQueryable of entities</returns>
        /// <exception cref="ArgumentNullException">if the data request is null</exception>
        internal static IQueryable<TEntity> GetWithDataRequest<TEntity>
            (this IQueryable<TEntity> source, DataRequest<TEntity> request)
                where TEntity : class, IEntity
        {
            if (request is null)
                return source;

            if (request.IgnoreQueryFilter)
                source = source.IgnoreQueryFilters();

            if (request.IncludeDefinitions != null && request.IncludeDefinitions.Count > 0)
            {
                foreach (var includeExpression in request.IncludeDefinitions)
                    source = source.Include(includeExpression);
            }

            if (!string.IsNullOrEmpty(request.SearchQuery))
                source = source.Where(r => r.SearchTerms.Contains(request.SearchQuery.ToLower()));

            if (request.Predicate != null)
                source = source.Where(request.Predicate);

            if (request.OrderByKeySelector != null)
                source = source.OrderBy(request.OrderByKeySelector);

            if (request.OrderByDescKeySelector != null)
                source = source.OrderByDescending(request.OrderByDescKeySelector);

            if (request.Skip.HasValue)
                source = source.Skip((int)request.Skip);

            if (request.Limit.HasValue)
                source = source.Skip((int)request.Limit);

            return source;
        }

        /// <summary>
        /// build the OrderBy Query dynamically
        /// </summary>
        /// <typeparam name="T">the type of entity we building the orderBy for it</typeparam>
        /// <param name="query">the query it self</param>
        /// <param name="sortColumn">the column you are soring with it</param>
        /// <param name="SortDirection">is the sorting direction</param>
        /// <returns>the query</returns>
        public static IQueryable<T> DynamicOrderBy<T>(this IQueryable<T> query, string sortColumn, SortDirection SortDirection)
        {
            // Dynamically creates a call like this: query.OrderBy(p =&gt; p.SortColumn)
            var parameter = Expression.Parameter(typeof(T), "p");

            // get the object type
            var objType = typeof(T);

            var property = objType.GetProperty(sortColumn,
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.Instance |
                BindingFlags.IgnoreCase);

            if (property is null)
                property = objType.GetProperty(nameof(Entity<object>.Id));

            // this is the part p.SortColumn
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);

            // this is the part p =&gt; p.SortColumn
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);

            // the sorting command
            var orderByCommand = SortDirection == SortDirection.Ascending ? "OrderBy" : "OrderByDescending";

            // finally, call the "OrderBy" / "OrderByDescending" method with the order by lambda expression
            var orderQuery = query.Provider
                .CreateQuery<T>(
                    Expression.Call(typeof(Queryable),
                    orderByCommand,
                    new Type[] { objType, property.PropertyType },
                    query.Expression,
                    Expression.Quote(orderByExpression))) as IOrderedQueryable<T>;

            return orderQuery;
        }

        /// <summary>
        /// this method is used to calculate the skip and page size
        /// </summary>
        /// <returns>skip and page size</returns>
        public static (int skip, int pageCount) CalculateSkipAndPageSize(int ItemsCount, int page, int pageSize)
        {
            var pageCount = (int)Math.Ceiling((double)ItemsCount / pageSize);
            var skip = (page - 1) * pageSize;

            return (skip, pageCount);
        }

        #endregion

        #region internal methods

        /// <summary>
        /// Apply the configuration from the current assembly
        /// </summary>
        /// <param name="modelBuilder">the model builder</param>
        internal static ModelBuilder ApplyAllConfigurations(this ModelBuilder modelBuilder)
        {
            var typesToRegister = typeof(ProductConfiguration)
                .Assembly
                .GetTypes()
                .Where(t => t.GetInterfaces()
                    .Any(gi => gi.IsGenericType && gi.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)))
                .ToList();

            foreach (var type in typesToRegister)
            {
                dynamic configurationInstance = Activator.CreateInstance(type);
                modelBuilder.ApplyConfiguration(configurationInstance);
            }

            return modelBuilder;
        }

        /// <summary>
        /// apply a default size to all properties of type string
        /// </summary>
        /// <param name="modelBuilder">the model builder</param>
        internal static ModelBuilder ApplyStringDefaultSize(this ModelBuilder modelBuilder)
        {
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                        .SelectMany(t => t.GetProperties())
                        .Where(p => p.ClrType == typeof(string)))
            {
                //property.AsProperty().Builder
                //    .HasMaxLength(255, ConfigurationSource.Convention);
            }

            return modelBuilder;
        }

        /// <summary>
        /// Apply the Commune Configuration to all entities that derives from <see cref="Entity{Tkey}"/>
        /// </summary>
        /// <param name="modelBuilder">the module builder instant</param>
        /// <returns>the module builder instant</returns>
        internal static ModelBuilder ApplyBaseEntityConfiguration(this ModelBuilder modelBuilder)
        {
            var method = typeof(Utilities).GetTypeInfo().DeclaredMethods
                .Single(m => m.Name == nameof(Configure));

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (entityType.ClrType.IsBaseEntity(out var T))
                {
                    method.MakeGenericMethod(entityType.ClrType, T)
                        .Invoke(null, new[] { modelBuilder });
                }
            }

            return modelBuilder;
        }

        #endregion

        #region private methods

        /// <summary>
        /// here apply all commune configuration to all entities that derives from <see cref="Entity{Tkey}"/>
        /// </summary>
        /// <typeparam name="TEntity">the type of the entity to be configured</typeparam>
        /// <typeparam name="T">the type of the key</typeparam>
        /// <param name="modelBuilder">the module builder instant</param>
        private static void Configure<TEntity, T>(ModelBuilder modelBuilder)
            where TEntity : Entity<T>
        {
            modelBuilder.Entity<TEntity>(builder =>
            {
                    //key
                    builder.HasKey(e => e.Id);

                    //properties configurations
                    builder.Property(e => e.Id)
                    .HasColumnName("Id");

                builder.Property(e => e.InsertDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                builder.Property(e => e.LastUpdate)
                    .IsRequired(false)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                builder.Property(e => e.SearchTerms)
                    .HasMaxLength(500);

                    // index
                    builder.HasIndex(e => e.SearchTerms);
            });
        }

        /// <summary>
        /// check if the given type is the base type
        /// </summary>
        /// <param name="type">the type to be checked</param>
        /// <param name="T">the output key type</param>
        /// <returns>true if it the base type, false if not</returns>
        private static bool IsBaseEntity(this Type type, out Type T)
        {
            for (var baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Entity<>))
                {
                    T = baseType.GetGenericArguments()[0];
                    return true;
                }
            }

            T = null;
            return false;
        }

        #endregion
    }
}
