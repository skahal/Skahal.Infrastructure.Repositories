using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
    public abstract class EFRepositoryBase<TDomainEntity, TId> : RepositoryBase<TDomainEntity>
        where TDomainEntity : EntityWithIdBase<TId>, IAggregateRoot
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Skahal.Infrastructure.Repositories.EntityFramework.EFRepositoryBase`2"/> class.
        /// </summary>
        /// <param name="context">The db context.</param>
        protected EFRepositoryBase(DbContext context)
        {
            Context = context;
            Context.Configuration.AutoDetectChangesEnabled = false;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        protected DbContext Context { get; private set; }

        /// <summary>
        /// Gets the db set.
        /// </summary>
        /// <value>The db set.</value>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
        protected virtual DbSet<TDomainEntity> DbSet
        {
            get
            {
                return Context.Set<TDomainEntity>();
            }
        }

        /// <summary>
        /// Gets the db query.
        /// </summary>
        /// <value>The db query.</value>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
        protected virtual DbQuery<TDomainEntity> DbQuery
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
        public override long CountAll(Expression<Func<TDomainEntity, bool>> filter)
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
        public override IEnumerable<TDomainEntity> FindAll(int offset, int limit, Expression<Func<TDomainEntity, bool>> filter)
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
        public override IEnumerable<TDomainEntity> FindAllAscending<TOrderByKey>(int offset, int limit, Expression<Func<TDomainEntity, bool>> filter, Expression<Func<TDomainEntity, TOrderByKey>> orderBy)
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
        public override IEnumerable<TDomainEntity> FindAllDescending<TOrderByKey>(int offset, int limit, Expression<Func<TDomainEntity, bool>> filter, Expression<Func<TDomainEntity, TOrderByKey>> orderBy)
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
        public override abstract TDomainEntity FindBy(object key);

        /// <summary>
        /// Persists the deleted item.
        /// </summary>
        /// <param name="item">Item.</param>
        protected override void PersistDeletedItem(TDomainEntity item)
        {
            var entry = Context.Entry<TDomainEntity>(item);
            entry.State = EntityState.Deleted;
        }

        /// <summary>
        /// Persists the new item.
        /// </summary>
        /// <param name="item">Item.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void PersistNewItem(TDomainEntity item)
        {            
            DbSet.Add(item);
        }

        #region PersistUpdatedItem
        /// <summary>
        /// Persists the updated item.
        /// </summary>
        /// <param name="item">Item.</param>
        protected override void PersistUpdatedItem(TDomainEntity item)
        {
            var entry = Context.Entry<TDomainEntity>(item);

            entry.State = EntityState.Modified;            
        }            
        #endregion

        #region Helpers
        /// <summary>
        /// Logs the query.
        /// </summary>
        /// <param name="query">Query.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "query")]
        private static void LogQuery(IQueryable<TDomainEntity> query)
        {
#if DEBUG
            LogService.Debug(query.ToString());
#endif
        }
        #endregion

        #endregion
    }
}