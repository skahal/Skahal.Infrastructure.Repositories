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
        #region Events
        /// <summary>
        /// Occurrs after a rollback is performed
        /// </summary>
        public event EventHandler Rollbacked;
        #endregion
        #region Fields
        private Func<DbContext> m_createNewContext;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Skahal.Infrastructure.Repositories.EntityFramework.EFUnitOfWork"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public EFUnitOfWork(DbContext context)
            : this(() => context)
        {
            ExceptionHelper.ThrowIfNull("context", context);
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Skahal.Infrastructure.Repositories.EntityFramework.EFUnitOfWork"/> class.
        /// </summary>
        /// <param name="createNewContext">The func to the context.</param>
        public EFUnitOfWork(Func<DbContext> createNewContext)
        {
            ExceptionHelper.ThrowIfNull("getContext", createNewContext);
            m_createNewContext = createNewContext;
            Context = createNewContext();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        protected virtual DbContext Context { get; private set; }
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

        /// <summary>
        /// Undo changes made after the latest commit.
        /// </summary>
        public override void Rollback()
        {
            base.Rollback();
            OnRollbacked(EventArgs.Empty);
            Context = m_createNewContext();
        }

        /// <summary>
        /// Raises the Rollbacked event.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        protected virtual void OnRollbacked(EventArgs args)
        {
            if (Rollbacked != null)
            {
                Rollbacked(this, args);
            }
        }
        #endregion
    }
}