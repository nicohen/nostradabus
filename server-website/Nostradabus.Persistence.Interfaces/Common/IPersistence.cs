using System.Collections.Generic;

namespace Nostradabus.Persistence.Interfaces.Common
{
    /// <summary>
    /// IPersistence Interfaces.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPersistence<T>
    {
        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="shouldLock">if set to <c>true</c> [should lock].</param>
        /// <returns></returns>
        T GetById(object id, bool shouldLock);

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        IList<T> GetAll();

        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        T Save(T entity);

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        T Update(T entity);

        /// <summary>
        /// Saves the or update.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        T SaveOrUpdate(T entity);
		
        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Delete(T entity);

        /// <summary>
        /// Commits the changes and close the session.
        /// </summary>
        void CommitChanges();

        /// <summary>
        /// Rollbacks the changes and close the session.
        /// </summary>
        void RollbackChanges();

        void Evict(object obj);

        void Lock(object obj);

        T LoadById(object id, bool shouldLock);
    }
}