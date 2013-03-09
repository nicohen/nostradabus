using System.Collections.Generic;
using System.Linq;
using Nostradabus.BusinessEntities.Common;
using Nostradabus.Persistence.Interfaces;
using Nostradabus.Persistence.Nhibernate.Common;
using NHibernate.Linq;

namespace Nostradabus.Persistence.Nhibernate
{
    /// <summary>
    /// Implementation  for Persistence of Code Description Entities.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NameDescriptionPersistence<T> : PersistenceManager<T>, INameDescriptionPersistence<T> where T : INameDescripton
    {

        #region INameDescriptionPersistence<T> Members

        public T GetByName(string name)
        {
            IQueryable<T> query = from t in CurrentSession.Query<T>() where t.Name == name select t;

            T entities = query.FirstOrDefault();

            return entities;
        }

        public IList<T> GetAllowedStatus(IList<T> allowedStatus)
        {
            IQueryable<T> query = from t in CurrentSession.Query<T>() select t;

            if (allowedStatus != null)
            {
                query = from u in query
                        where allowedStatus.Contains(u)
                        select u;
                
            }

            return query.ToList();
        }

        #endregion
    }
}
