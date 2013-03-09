using Nostradabus.Persistence.Nhibernate.Common;
using NHibernate;

namespace Nostradabus.Persistence.Nhibernate.Common
{
    /// <summary>
    /// Base Class for Persistence of Nhibernate.
    /// </summary>
    public abstract class BasePersistenceManager
    {
        public ISession CurrentSession { get { return SessionManager.Instance.GetSession(); } }
    }
}
