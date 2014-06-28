using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using HelperSharp;
using Skahal.Infrastructure.Framework.Repositories;

namespace Skahal.Infrastructure.Repositories.EntityFramework
{
    /// <summary>
    /// An IUnitOfWork implementation for EntityFramework.
    /// </summary>
    public class EFUnitOfWork : MemoryUnitOfWork
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Skahal.Infrastructure.Repositories.EntityFramework.EFUnitOfWork"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public EFUnitOfWork(DbContext context)
        {
            ExceptionHelper.ThrowIfNull("context", context);

            Context = context;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        protected DbContext Context { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Commit the registered entities.
        /// </summary>        
        public override void Commit()
        {
            base.Commit();

            try
            {
                Context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var msg = new StringBuilder();
                msg.AppendLine("The following errors occurred when performing the validation in the context SaveChanges:");

                foreach (var error in ex.EntityValidationErrors)
                {
                    msg.AppendFormat("{0}: {1}", error.Entry.Entity.GetType().Name, String.Join(", ", error.ValidationErrors.Select(v => v.ErrorMessage)));
                    msg.AppendLine();
                }

                throw new InvalidOperationException(msg.ToString(), ex);
            }
        }
        #endregion
    }
}