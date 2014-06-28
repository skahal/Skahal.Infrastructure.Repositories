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
    public class EFRepositoryLongIdBase <TDomainEntity> : EFRepositoryBase<TDomainEntity, long>
        where TDomainEntity : EntityWithIdBase<long>, IAggregateRoot
    {

        #region Constructors
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
        public override TDomainEntity FindBy(object key)
        {
            var id = (long)key;

            return DbSet.AsNoTracking().FirstOrDefault((e) => e.Id.Equals(id));
        }
        #endregion
    }
}
