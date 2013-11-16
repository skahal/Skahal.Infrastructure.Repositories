using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Skahal.Infrastructure.Framework.Domain;
using Skahal.Infrastructure.Framework.Repositories;

namespace Skahal.Infrastructure.Repositories
{
	/// <summary>
	/// Mongo DB repository base class.
	/// </summary>
	public abstract class MongoDBRepositoryBase<TEntity>: RepositoryBase<TEntity> 
		where TEntity : IAggregateRoot
	{
		#region Fields
		private MongoCollection<TEntity> m_collection;
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes the <see cref="Skahal.Infrastructure.Repositories.MongoDBRepositoryBase`1"/> class.
		/// </summary>
		static MongoDBRepositoryBase()
		{
			BsonClassMap.RegisterClassMap<EntityBase>(cm => {
				cm.MapIdProperty("Key");
			});
		}

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="jogosdaqui.Infrastructure.Repositories.MongoDB.MongoDBRepositoryBase`1"/> class.
		/// </summary>
		public MongoDBRepositoryBase(string mongoUrl, string collectionName)
		{
			var url = new MongoUrl(mongoUrl);
			var client = new MongoClient(url);
			var server = client.GetServer();
			var database = server.GetDatabase(String.IsNullOrWhiteSpace(url.DatabaseName) ? "test" : url.DatabaseName);
			m_collection = database.GetCollection<TEntity>(collectionName);
		}
		#endregion

		#region implemented abstract members of RepositoryBase

		/// <summary>
		/// Finds the entity by the key.
		/// </summary>
		/// <returns>The found entity.</returns>
		/// <param name="key">Key.</param>
		public override TEntity FindBy (object key)
		{
			return m_collection.AsQueryable<TEntity> ().FirstOrDefault(t => t.Key.Equals(key));
		}

		/// <summary>
		/// Finds all entities that matches the filter.
		/// </summary>
		/// <returns>The found entities.</returns>
		/// <param name="offset">The offset to start the result.</param>
		/// <param name="limit">The result count limit.</param>
		/// <param name="filter">The entities filter.</param>
		public override IEnumerable<TEntity> FindAll (int offset, int limit, Expression<Func<TEntity, bool>> filter)
		{
			return m_collection.FindAll ().Where (filter.Compile()).Skip (offset).Take (limit);
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
		public override IEnumerable<TEntity> FindAllAscending<TKey> (int offset, int limit, Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> orderBy)
		{
			return m_collection.FindAll ().Where (filter.Compile()).OrderBy(orderBy.Compile()).Skip (offset).Take (limit);
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
		public override IEnumerable<TEntity> FindAllDescending<TKey> (int offset, int limit, Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> orderBy)
		{
			return m_collection.FindAll ().Where (filter.Compile()).OrderByDescending(orderBy.Compile()).Skip (offset).Take (limit);
		}

		/// <summary>
		/// Counts all entities that matches the filter.
		/// </summary>
		/// <returns>The number of the entities that matches the filter.</returns>
		/// <param name="filter">Filter.</param>
		public override long CountAll (Expression<Func<TEntity, bool>> filter)
		{
			return m_collection.FindAll ().LongCount (filter.Compile());
		}

		/// <summary>
		/// Persists the new item.
		/// </summary>
		/// <param name="item">Item.</param>
		protected override void PersistNewItem (TEntity item)
		{
			m_collection.Insert (item);
		}

		/// <summary>
		/// Persists the updated item.
		/// </summary>
		/// <param name="item">Item.</param>
		protected override void PersistUpdatedItem (TEntity item)
		{
			m_collection.Save (item);
		}

		/// <summary>
		/// Persists the deleted item.
		/// </summary>
		/// <param name="item">Item.</param>
		protected override void PersistDeletedItem (TEntity item)
		{
			m_collection.Remove(((MongoQueryable<TEntity>) m_collection.AsQueryable<TEntity>().Where(f => f.Key.Equals(item.Key))).GetMongoQuery());			
		}
		#endregion
	}
}