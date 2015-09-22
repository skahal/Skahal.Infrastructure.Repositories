using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Skahal.Infrastructure.Framework.Domain;
using Skahal.Infrastructure.Framework.Logging;
using Skahal.Infrastructure.Framework.Repositories;

namespace Skahal.Infrastructure.Repositories.EntityFramework
{
    /// <summary>
    /// Entity framework repository base class.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    public abstract class EFRepositoryBase<TEntity, TId> : RepositoryBase<TEntity>
        where TEntity : EntityWithIdBase<TId>, IAggregateRoot
    {
        #region Fields
        private Func<DbContext> m_getContext;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Skahal.Infrastructure.Repositories.EntityFramework.EFRepositoryBase{TEntity, TId}" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        protected EFRepositoryBase(DbContext context)
            : this(() => context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Skahal.Infrastructure.Repositories.EntityFramework.EFRepositoryBase{TEntity, TId}" /> class.
        /// </summary>
        /// <param name="getContext">The func to the context.</param>
        protected EFRepositoryBase(Func<DbContext> getContext)
        {
            m_getContext = getContext;
            Context.Configuration.AutoDetectChangesEnabled = false;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        protected virtual DbContext Context
        {
            get
            {
                return m_getContext();
            }
        }

        /// <summary>
        /// Gets the db set.
        /// </summary>
        /// <value>The db set.</value>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
        protected virtual DbSet<TEntity> DbSet
        {
            get
            {
                return Context.Set<TEntity>();
            }
        }

        /// <summary>
        /// Gets the db query.
        /// </summary>
        /// <value>The db query.</value>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
        protected virtual DbQuery<TEntity> DbQuery
        {
            get
            {
                return DbSet.AsNoTracking();
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Counts all entities that matches the filter.
        /// </summary>
        /// <returns>The number of the entities that matches the filter.</returns>
        /// <param name="filter">Filter.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public override long CountAll(Expression<Func<TEntity, bool>> filter)
        {
            return DbSet.Count(filter);
        }

        /// <summary>
        /// Finds all entities that matches the filter.
        /// </summary>
        /// <returns>The found entities.</returns>
        /// <param name="offset">The offset to start the result.</param>
        /// <param name="limit">The result count limit.</param>
        /// <param name="filter">The entities filter.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public override IEnumerable<TEntity> FindAll(int offset, int limit, Expression<Func<TEntity, bool>> filter)
        {
            var result = DbQuery
                .Where(filter)
                .OrderBy(e => e.Id)
                .Skip(offset)
                .Take(limit);

            LogQuery(result);

            return result;
        }

        /// <summary>
        /// Finds all entities that matches the filter in a ascending order.
        /// </summary>
        /// <returns>The found entities.</returns>
        /// <param name="offset">The offset to start the result.</param>
        /// <param name="limit">The result count limit.</param>
        /// <param name="filter">The entities filter.</param>
        /// <param name="orderBy">The order.</param>
        /// <typeparam name="TOrderByKey">The 1st type parameter.</typeparam>
        public override IEnumerable<TEntity> FindAllAscending<TOrderByKey>(int offset, int limit, Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TOrderByKey>> orderBy)
        {
            var result = DbQuery
                .Where(filter)
                .OrderBy(orderBy)
                .Skip(offset)
                .Take(limit);

            LogQuery(result);

            return result;
        }

        /// <summary>
        /// Finds all entities that matches the filter in a descending order.
        /// </summary>
        /// <returns>The found entities.</returns>
        /// <param name="offset">The offset to start the result.</param>
        /// <param name="limit">The result count limit.</param>
        /// <param name="filter">The entities filter.</param>
        /// <param name="orderBy">The order.</param>
        /// <typeparam name="TOrderByKey">The 1st type parameter.</typeparam>
        public override IEnumerable<TEntity> FindAllDescending<TOrderByKey>(int offset, int limit, Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TOrderByKey>> orderBy)
        {
            var result = DbQuery
                .Where(filter)
                .OrderByDescending(orderBy)
                .Skip(offset)
                .Take(limit);

            LogQuery(result);

            return result;
        }

        /// <summary>
        /// Finds the entity by the key.
        /// </summary>
        /// <returns>The found entity.</returns>
        /// <param name="key">Key.</param>
        public override abstract TEntity FindBy(object key);

        /// <summary>
        /// Persists the deleted item.
        /// </summary>
        /// <param name="item">Item.</param>
        protected override void PersistDeletedItem(TEntity item)
        {
            var entry = Context.Entry<TEntity>(item);
            entry.State = EntityState.Deleted;
        }

        /// <summary>
        /// Persists the new item.
        /// </summary>
        /// <param name="item">Item.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void PersistNewItem(TEntity item)
        {
            DbSet.Add(item);
        }

        #region PersistUpdatedItem
        /// <summary>
        /// Persists the updated item.
        /// </summary>
        /// <param name="item">Item.</param>
        protected override void PersistUpdatedItem(TEntity item)
        {
            var entry = Context.Entry<TEntity>(item);

            entry.State = EntityState.Modified;
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Logs the query.'    
        /// </summary>
        /// <param name="query">Query.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "query")]
        private static void LogQuery(IQueryable<TEntity> query)
        {
#if DEBUG
            LogService.Debug(query.ToString());
#endif
        }
        #endregion

        #endregion
    }
}