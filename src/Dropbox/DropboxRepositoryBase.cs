using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BobbyTables;
using HelperSharp;
using Skahal.Infrastructure.Framework.Domain;
using Skahal.Infrastructure.Framework.Repositories;

namespace Skahal.Infrastructure.Repositories.Dropbox
{
    /// <summary>
    /// Dropbox repository base class.
    /// <remarks>>
    /// This repository use the Dropbox datastore API (https://www.dropbox.com/developers/datastore) via BobbyTables (http://mrsharpoblunto.github.io/BobbyTables/).
    /// 
    /// The common use for this repository is to sync structured data like contacts, to-do items, and game state.
    /// 
    /// </remarks>
    /// </summary>
    public class DropboxRepositoryBase<TEntity> : RepositoryBase<TEntity> where TEntity : EntityWithIdBase<string>, IAggregateRoot, new()
    {
        #region Fields
        private string m_apiToken;
        private string m_datastoreId;
        private Datastore m_ds;
        private Table<TEntity> m_table;
        private bool m_initialized;
        #endregion

        #region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Skahal.Infrastructure.Repositories.Dropbox.DropboxRepositoryBase{TEntity}" /> class.
		/// </summary>
		/// <param name="apiToken">The Dropbox API token. Configure your app on Dropbox, then call https://www.dropbox.com/1/oauth2/authorize?client_id=[app key]&amp;response_type=code&amp;redirect_uri=http://localhost</param>
		/// <param name="datastoreId">The datastore identifier.</param>
        public DropboxRepositoryBase(string apiToken, string datastoreId)
        {
            ExceptionHelper.ThrowIfNull("apiToken", apiToken);
            ExceptionHelper.ThrowIfNull("datastoreId", datastoreId);

            m_apiToken = apiToken;
            m_datastoreId = datastoreId;
        }
        #endregion

        #region Methods
		/// <summary>
		/// Counts all entities that matches the filter.
		/// </summary>
		/// <returns>The number of the entities that matches the filter.</returns>
		/// <param name="filter">Filter.</param>
        public override long CountAll(Expression<Func<TEntity, bool>> filter)
        {
            Initialize();

            if (filter == null)
            {
                return m_table.Count();
            }
            else
            {
                return m_table.Count(filter.Compile());
            }
        }

		/// <summary>
		/// Finds all entities that matches the filter.
		/// </summary>
		/// <returns>The found entities.</returns>
		/// <param name="offset">The offset to start the result.</param>
		/// <param name="limit">The result count limit.</param>
		/// <param name="filter">The entities filter.</param>
        public override IEnumerable<TEntity> FindAll(int offset, int limit, Expression<Func<TEntity, bool>> filter)
        {
            Initialize();

            if (filter == null)
            {
                return m_table
                    .Skip(offset)
                    .Take(limit);
            }
            else
            {
                return m_table
                    .Where(filter.Compile())
                    .Skip(offset)
                    .Take(limit);
            }
        }

		/// <summary>
		/// Finds all entities that matches the filter in a ascending order.
		/// </summary>
		/// <returns>The found entities.</returns>
		/// <param name="offset">The offset to start the result.</param>
		/// <param name="limit">The result count limit.</param>
		/// <param name="filter">The entities filter.</param>
		/// <param name="orderBy">The order.</param>
		/// <typeparam name="TKey">The 1st type parameter.</typeparam>
        public override IEnumerable<TEntity> FindAllAscending<TKey>(int offset, int limit, Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> orderBy)
        {
            Initialize();
            return FindAll(offset, limit, filter).OrderBy(orderBy.Compile());
        }

		/// <summary>
		/// Finds all entities that matches the filter in a descending order.
		/// </summary>
		/// <returns>The found entities.</returns>
		/// <param name="offset">The offset to start the result.</param>
		/// <param name="limit">The result count limit.</param>
		/// <param name="filter">The entities filter.</param>
		/// <param name="orderBy">The order.</param>
		/// <typeparam name="TKey">The 1st type parameter.</typeparam>
        public override IEnumerable<TEntity> FindAllDescending<TKey>(int offset, int limit, Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> orderBy)
        {
            Initialize();
            return FindAll(offset, limit, filter).OrderByDescending(orderBy.Compile());
        }

		/// <summary>
		/// Finds the entity by the key.
		/// </summary>
		/// <returns>The found entity.</returns>
		/// <param name="key">Key.</param>
        public override TEntity FindBy(object key)
        {
            Initialize();
            return m_table.Get(key as string);
        }

        /// <summary>
        /// Clears all.
        /// </summary>
        public void ClearAll()
        {
            Initialize();

            foreach (var r in m_table)
            {
                m_table.Delete(r.Id);
            }

            PushToDropbox();
        }

		/// <summary>
		/// Persists the deleted item.
		/// </summary>
		/// <param name="item">Item.</param>
        protected override void PersistDeletedItem(TEntity item)
        {
            Initialize();
            m_table.Delete(item.Id);
            PushToDropbox();
        }

        /// <summary>
        /// Persists the new item.
        /// </summary>
        /// <param name="item">The item.</param>
        protected override void PersistNewItem(TEntity item)
        {
            Initialize();
            m_table.Insert(item);
            PushToDropbox();
        }

        private void PushToDropbox()
        {
            if (!m_ds.Push())
            {
                throw new InvalidOperationException("Error syncing with DropBox.");
            }
        }

        /// <summary>
        /// Persists the updated item.
        /// </summary>
        /// <param name="item">The item.</param>
        protected override void PersistUpdatedItem(TEntity item)
        {
            Initialize();
            m_table.Update(item);
            PushToDropbox();
        }

        private void Initialize()
        {
            if (!m_initialized)
            {
                var manager = new DatastoreManager(m_apiToken);
                m_ds = manager.GetOrCreate(m_datastoreId.ToLowerInvariant());
                m_table = m_ds.GetTable<TEntity>(typeof(TEntity).Name);
                m_ds.Pull();
                m_initialized = true;
            }
        }
        #endregion
    }
}
