using System;
using System.Collections.Generic;
using System.Linq;
using Nostradabus.BusinessEntities.Interfaces;
using Nostradabus.Persistence.Interfaces.Common;
using NHibernate;
using NHibernate.Linq;

namespace Nostradabus.Persistence.Nhibernate.Common
{
    /// <summary>
    /// Base Implementation for Data Access Interfaces.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PersistenceManager<T> : BasePersistenceManager, IPersistence<T> where T : IBusinessEntity
    {
        /// <summary>
        /// Type of T
        /// </summary>
        private readonly Type _persitentType = typeof (T);

        #region IPersistence<T> Members

        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="shouldLock">if set to <c>true</c> [should lock].</param>
        /// <returns></returns>
        public virtual T GetById(object id, bool shouldLock)
        {
            T entity;

            if (shouldLock)
            {
                entity = (T)CurrentSession.Get(_persitentType, id, LockMode.Upgrade);
            }
            else
            {
                entity = (T)CurrentSession.Get(_persitentType, id);
            }
            
            return entity;
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        public IList<T> GetAll()
        {
            IQueryable<T> query = from t in CurrentSession.Query<T>() select t;

            IList<T> entities = query.ToList();

            return entities;
           
        }
		
        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public T Save(T entity)
        {
            CurrentSession.Save(entity);
            return entity;
        }

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public T Update(T entity)
        {
            CurrentSession.Update(entity);
            return entity;
        }

        /// <summary>
        /// Saves the or update.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public T SaveOrUpdate(T entity)
        {
            CurrentSession.SaveOrUpdate(entity);

            return entity;
        }
		
        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Delete(T entity)
        {
            CurrentSession.Delete(entity);
        }

        /// <summary>
        /// Commits the changes and close the session.
        /// </summary>
        public void CommitChanges()
        {
			CurrentSession.Flush();

			if (CurrentSession.Transaction != null && CurrentSession.Transaction.IsActive)
			{
				CurrentSession.Transaction.Commit();

				CurrentSession.BeginTransaction();
			}
        }

        /// <summary>
        /// Rollbacks the changes and close the session.
        /// </summary>
        public void RollbackChanges()
        {
			if (CurrentSession.Transaction != null && CurrentSession.Transaction.IsActive)
			{
				CurrentSession.Clear();

				CurrentSession.Transaction.Rollback();

				CurrentSession.BeginTransaction();
			}
        }

        public void Evict(object obj) 
        {
            CurrentSession.Flush();

            CurrentSession.Evict(obj);
        }

        public void Lock(object obj)
        {
            CurrentSession.Update(obj);
        }

        public virtual T LoadById(object id, bool shouldLock)
        {
            T entity;

            if (shouldLock)
            {
                entity = (T)CurrentSession.Load(typeof(T), id, LockMode.Upgrade);
            }
            else
            {
                entity = (T)CurrentSession.Load(typeof(T), id);
            }

            return entity;
        }

        #endregion
    }
}