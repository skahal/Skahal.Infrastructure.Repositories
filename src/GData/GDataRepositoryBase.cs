using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GDataDB;
using HelperSharp;
using Skahal.Infrastructure.Framework.Domain;
using Skahal.Infrastructure.Framework.Repositories;

namespace Skahal.Infrastructure.Repositories.GData
{
    /// <summary>
    /// GData repository base class.
    /// <remarks>>
    /// This repository use a Google Docs Spreadsheet has database, each entity will be a sheet inside that spreadsheet.
    /// Of course, this repository is for specific use, please, don't try to use it has an app database, it is not for that ;)
    /// 
    /// The common use for this repository is to store a specific app configuration or to import/export data to a spreeadsheet.
    /// Does your user ask you to export that report data to a Google Docs Spreadsheet? This repository is for that kind o use.
    /// 
    /// The CountAll and FindAll methods are not optimized, this mean that all linq expression are performed locally.
    /// </remarks>
    /// </summary>
    public class GDataRepositoryBase<TEntity> : RepositoryBase<TEntity> 
        where TEntity : EntityWithIdBase<string>, IAggregateRoot, new()
    {
        #region Fields
        private string m_userName;
        private string m_password;
        private string m_databaseName;
        private string m_tableName;
        private ITable<TEntity> m_table;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Skahal.Infrastructure.Repositories.GData.GDataRepositoryBase`1"/> class.
        /// </summary>
        /// <param name="databaseName">The database name. This is the name of spreadsheet that will be created on Google Docs.</param>
        /// <param name="userName">The Google Docs account user name.</param>
        /// <param name="password">The Google Docs account password. If you use two-steps-verification, will you need to generated a app specific password.</param>
        public GDataRepositoryBase(string databaseName, string userName, string password)
            : this(databaseName, userName, password, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Skahal.Infrastructure.Repositories.GData.GDataRepositoryBase`1"/> class.
        /// </summary>
        /// <param name="databaseName">The database name. This is the name of spreadsheet that will be created on Google Docs.</param>
        /// <param name="userName">The Google Docs account user name.</param>
        /// <param name="password">The Google Docs account password. If you use two-steps-verification, will you need to generated a app specific password.</param>
        /// <param name="tableName">The default name of table (sheet) is the entity name, but you can choose your own with this argument.</param>>
        public GDataRepositoryBase(string databaseName, string userName, string password, string tableName)
        {
            m_databaseName = databaseName;
            m_userName = userName;
            m_password = password;

            if (String.IsNullOrEmpty(tableName))
            {
                m_tableName = typeof(TEntity).Name;
            }
            else
            {
                m_tableName = tableName;
            }
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
            return FindAll(0, int.MaxValue, filter).Count();
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
            var rows = GetTable().FindAll();
            var elements = rows.Select(t => t.Element);

            elements = filter == null ? elements : elements.Where(filter.Compile());

            return elements.Skip (offset).Take (limit);
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
            return FindAll(offset, limit, filter).OrderByDescending(orderBy.Compile());
        }

        /// <summary>
        /// Finds the entity by the key.
        /// </summary>
        /// <returns>The found entity.</returns>
        /// <param name="key">Key.</param>
        public override TEntity FindBy(object key)
        {
            TEntity element = null;
            var row = GetRowById(key.ToString());

            if (row != null)
            {
                element = row.Element;
            }

            return element;
        }

        /// <summary>
        /// Persists the deleted item.
        /// </summary>
        /// <param name="item">Item.</param>
        protected override void PersistDeletedItem(TEntity item)
        {
            var row = GetRowById(item.Id);

            if (row != null)
            {
                row.Delete();
            }
        }

        /// <summary>
        /// Persists the new item.
        /// </summary>
        /// <param name="item">Item.</param>
        protected override void PersistNewItem(TEntity item)
        {
            if (String.IsNullOrEmpty(item.Id))
            {
                item.Id = Guid.NewGuid().ToString();
            }

            GetTable().Add(item);
        }

        /// <summary>
        /// Persists the updated item.
        /// </summary>
        /// <param name="item">Item.</param>
        protected override void PersistUpdatedItem(TEntity item)
        {
            var row = GetRowById(item.Id);

            if (row != null)
            {
                row.Element = item;
                row.Update();
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Gets the table.
        /// </summary>
        /// <returns>The table.</returns>
        private ITable<TEntity> GetTable()
        {
            if (m_table == null)
            {
                var client = new DatabaseClient(m_userName, m_password);
                var db = client.GetDatabase(m_databaseName) ?? client.CreateDatabase(m_databaseName);

                m_table = db.GetTable<TEntity>(m_tableName) ?? db.CreateTable<TEntity>(m_tableName);
            }

            return m_table;
        }

        /// <summary>
        /// Gets the row by identifier.
        /// </summary>
        /// <returns>The row by identifier.</returns>
        /// <param name="key">Key.</param>
        private IRow<TEntity> GetRowById(string key)
        {
            return GetTable().FindStructured("id={0}".With(key)).FirstOrDefault();
        }
        #endregion
    }
}
