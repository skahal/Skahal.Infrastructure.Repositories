using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Skahal.Infrastructure.Framework.Domain;

namespace Skahal.Infrastructure.Repositories.EntityFramework
{
    /// <summary>
    /// An EntityFramework repository using long id.
    /// </summary>
    public class EFRepositoryLongIdBase <TEntity> : EFRepositoryBase<TEntity, long>
        where TEntity : EntityWithIdBase<long>, IAggregateRoot
    {

        #region Constructors
		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="Skahal.Infrastructure.Repositories.EntityFramework.EFRepositoryLongIdBase{TEntity}"/> class.
		/// </summary>
		/// <param name="context">Context.</param>
        protected EFRepositoryLongIdBase(DbContext context) : base(context)
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public override TEntity FindBy(object key)
        {
            var id = (long)key;

            return DbSet.AsNoTracking().FirstOrDefault((e) => e.Id.Equals(id));
        }
        #endregion
    }
}
