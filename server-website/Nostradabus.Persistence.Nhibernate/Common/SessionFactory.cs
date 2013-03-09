using NHibernate;
using NHibernate.Cfg;

namespace Nostradabus.Persistence.Nhibernate.Common
{
    /// <summary>
    /// NHibernate Session Factory Class.
    /// </summary>
    internal class SessionFactory
    {
        private static SessionFactory current;
        private readonly ISessionFactory sessionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionFactory"/> class.
        /// </summary>
        protected SessionFactory()
        {
            Configuration configuration = new Configuration();
            sessionFactory = configuration.BuildSessionFactory();
        }

        /// <summary>
        /// Gets the current Session Factory.
        /// </summary>
        /// <value>The current.</value>
        internal static SessionFactory Current
        {
            get
            {
                if (current == null)
                    current = new SessionFactory();

                return current;
            }
        }

        /// <summary>
        /// Open and gets the session.
        /// </summary>
        /// <returns></returns>
        internal ISession GetSession()
        {
            return  sessionFactory.OpenSession();
        }
    }
}