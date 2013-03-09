using Nostradabus.Persistence.Interfaces.Common;
using Nostradabus.Persistence.Nhibernate.Common;
using NHibernate;

namespace Nostradabus.Persistence.Nhibernate.Common
{
    public class SessionManagerProxy : IPersistanceSessionManager
    {
        public void OpenSession()
        {
            SessionManager.Instance.OpenSession();
        }

        public void CloseSession()
        {
            ISession session = SessionManager.Instance.GetSession();

            if (session.IsOpen)
            {
            	session.Close();
            }
        }
        
        public void ClearSession()
        {
            SessionManager.Instance.GetSession().Clear();
        }
	}
}