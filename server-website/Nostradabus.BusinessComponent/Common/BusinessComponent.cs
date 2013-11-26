using System;
using System.Collections.Generic;
using Nostradabus.BusinessComponents.Exceptions;
using Nostradabus.BusinessEntities.Interfaces;
using Nostradabus.Common.Logger;
using Nostradabus.Persistence.Interfaces.Common;
using Microsoft.Practices.ServiceLocation;

namespace Nostradabus.BusinessComponents.Common
{
    /// <summary>
    /// Component base 
    /// </summary>
    /// <typeparam name="T">Entity (IBusinessEntity) to use</typeparam>
    public abstract class BusinessComponent<T> where T : IBusinessEntity
    {
        public virtual string ResourceName
        {
            get { return typeof (T).FullName; }
        }

        #region CRUD Methods

        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual T Save(T entity)
        {
			return Save(entity, false);
        }

		/// <summary>
		/// Saves the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="keepAliveTransaction">if set to <c>true</c> [keep alive transaction].</param>
		/// <returns></returns>
		public virtual T Save(T entity,bool keepAliveTransaction)
		{
			try
			{
				ValidationSummary validationSummary = Validate(entity);

				if (!validationSummary.HasErrors)
				{
					entity = Persistence().Save(entity);
					//Check if necessary still working with some transaction.
					if(!keepAliveTransaction)
						Persistence().CommitChanges();
					return entity;
				}

				throw new ValidationException(validationSummary);
			}
			catch (Exception e)
			{
				Persistence().RollbackChanges();
				LoggingManager.Logging.Error(e);
				throw;
			}
		}

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual T Update(T entity)
        {
        	return Update(entity, false);
        }

		/// <summary>
		/// Updates the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="keepAliveTransaction">if set to <c>true</c> keep working on some transaction [keep alive transaction].</param>
		/// <returns></returns>
		public virtual T Update(T entity,bool keepAliveTransaction)
		{
			try
			{
				ValidationSummary validationSummary = Validate(entity);

				if (!validationSummary.HasErrors)
				{
					entity = Persistence().Update(entity);
					//Check if necessary still working with some transaction.
					if(!keepAliveTransaction)
						Persistence().CommitChanges();
					return entity;
				}

				throw new ValidationException(validationSummary);
			}
			catch (Exception e)
			{
				Persistence().RollbackChanges();
				LoggingManager.Logging.Error(e);
				throw;
			}
		}

        /// <summary>
        /// Saves the or update the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual T SaveOrUpdate(T entity)
        {
        	return SaveOrUpdate(entity, false);
        }

		/// <summary>
		/// Saves the or update the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="keepAliveTransaction">if set to <c>true</c> keep working on some transaction [keep alive transaction].</param>
		/// <returns></returns>
		public virtual T SaveOrUpdate(T entity,bool keepAliveTransaction)
		{
			try
			{
				ValidationSummary validationSummary = Validate(entity);

				if (!validationSummary.HasErrors)
				{
					entity = Persistence().SaveOrUpdate(entity);
					//Check if necessary still working with some transaction.
					if(!keepAliveTransaction)
						Persistence().CommitChanges();
					return entity;
				}

				throw new ValidationException(validationSummary);
			}
			catch (Exception e)
			{
				Persistence().RollbackChanges();
				LoggingManager.Logging.Error(e);
				throw;
			}
		}

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual bool Delete(T entity)
        {
        	return Delete(entity, false);
        }

		/// <summary>
		/// Deletes the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="keepAliveTransaction">if set to <c>true</c> keep working on some transaction [keep alive transaction].</param>
		/// <returns></returns>
		public virtual bool Delete(T entity,bool keepAliveTransaction)
		{
			try
			{
				Persistence().Delete(entity);
				if(!keepAliveTransaction)
					Persistence().CommitChanges();

				return true;
			}
			catch (Exception e)
			{
				Persistence().RollbackChanges();
				LoggingManager.Logging.Error(e);
				throw;
			}
		}

        /// <summary>
        /// Gets the by id without lock
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
		public virtual T GetById(object id)
        {
            try
            {
                T entity = Persistence().GetById(id, false);
                return entity;
            }
            catch (Exception e)
            {
                LoggingManager.Logging.Error(e);
                throw;
            }
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        public virtual IList<T> GetAll()
        {
            try
            {
                IList<T> entities = Persistence().GetAll();
                return entities;
            }
            catch (Exception e)
            {
                LoggingManager.Logging.Error(e);
                throw;
            }
        }

        #endregion

        #region Protect Methods

        /// <summary>
        /// Throws the validation exception with error code.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="entity">The entity.</param>
        protected virtual void ThrowValidationException(string errorCode, string resourceName, IBusinessEntity entity)
        {
            var summary = new ValidationSummary();
            summary.Errors.Add(new ValidationError(errorCode, resourceName, null));
            throw new ValidationException(summary);
        }

        /// <summary>
        /// Throws the validation exception.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="entity">The entity.</param>
        protected virtual void ThrowValidationException(string errorCode, IBusinessEntity entity)
        {
            ThrowValidationException(errorCode, ResourceName, null);
        }

        #endregion

        internal TD Persistence<TD>() where TD : IPersistence<T>
        {
            return ServiceLocator.Current.GetInstance<TD>();
        }

		internal TD Persistence<TD,TI>() where TD : IPersistence<TI>
		{
			return ServiceLocator.Current.GetInstance<TD>();
		}

        internal IPersistence<T> Persistence()
        {
            return Persistence<IPersistence<T>>();
        }

		public abstract ValidationSummary Validate(T entity);

        /// <summary>
        /// Gets the by id without lock
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public virtual T LoadById(object id)
        {
            try
            {
                T entity = Persistence().LoadById(id, false);
                return entity;
            }
            catch (Exception e)
            {
                LoggingManager.Logging.Error(e);
                throw;
            }
        }
    }
}