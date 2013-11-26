using System;
using System.Web;
using Nostradabus.Common.Logger;
using NHibernate;
using NHibernate.Cfg;

//http://www.martinwilley.com/net/code/nhibernate/sessionmanager.html

namespace Nostradabus.Persistence.Nhibernate.Common
{
    public sealed class SessionManager 
    {
        private const string SESSIONKEY = "NHIBERNATE.SESSION";

        [ThreadStatic]
        private ISession ActiveSession;

        private readonly ISessionFactory sessionFactory;
		private readonly Configuration configuration;
        
        #region Constructor

        #region Singleton

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static SessionManager Instance
        {
            get {
                try
                {
                    return Singleton.SessionManager;
                }
                catch (Exception e)
                {
                    LoggingManager.Logging.Error(e);

					// verification for cases when is running from 
					// tests or process without HttpContext
					if (HttpContext.Current != null)
					{
						HttpContext.Current.Response.Clear();
						string exceptionMessage = e.Message;
						if (null != e.InnerException) exceptionMessage += ' ' + e.InnerException.Message;
						HttpContext.Current.Response.Write("Exception: " + exceptionMessage);
						HttpContext.Current.Response.End();
					}
                    
                    return null;
                }
            }
        }

        private static class Singleton
        {
            internal static readonly SessionManager SessionManager = new SessionManager();
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionManager"/> class.
        /// </summary>
        private SessionManager()
        {
            configuration = new Configuration();
            configuration.Configure();
            sessionFactory = configuration.BuildSessionFactory();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="NHibernate.Cfg.Configuration"/>.
        /// </summary>
        private Configuration Configuration { get { return configuration; } }

        /// <summary>
        /// Gets the <see cref="ISessionFactory"/>
        /// </summary>
        private ISessionFactory SessionFactory { get { return sessionFactory; } }

        /// <summary>
        /// Gets a value indicating whether this instance is web based.
        /// </summary>
        /// <value><c>true</c> if this instance is web based; otherwise, <c>false</c>.</value>
        private static bool IsWeb { get { return (HttpContext.Current != null); } }

        /// <summary>
        /// Closes the session factory. 
        /// </summary>
        public void Close()
        {
            SessionFactory.Close();
        }

        #endregion

        /// <summary>
        /// Gets the current <see cref="ISession"/>. 
        /// Although this is a singleton, this is specific to the thread/ asp session. 
        /// If you want to handle mulitple sessions, use <see cref="OpenSession"/> directly. 
        /// If a session it not open, a new open session is created and returned.
        /// </summary>
        /// <value>The <see cref="ISession"/></value>
        public ISession GetSession()
        {
            //use threadStatic or asp session.
            ISession session = IsWeb ? HttpContext.Current.Items[SESSIONKEY] as ISession : ActiveSession;
            //if using CurrentSessionContext, SessionFactory.GetCurrentSession() can be used

            //if it's an open session, that's all
            if (session != null && session.IsOpen)
                return session;

            //if not open, open a new session
            return OpenSession();
        }

        #region NHibernate Sessions

        /// <summary>
        /// Explicitly open a session. If you have an open session, close it first.
        /// </summary>
        /// <returns>The <see cref="ISession"/></returns>
        public ISession OpenSession()
        {
            ISession session = SessionFactory.OpenSession();
            
            if (IsWeb)
                HttpContext.Current.Items[SESSIONKEY] = session;
            else
                ActiveSession = session;

			session.BeginTransaction();

            return session;
        }

        #endregion
        
    }
}