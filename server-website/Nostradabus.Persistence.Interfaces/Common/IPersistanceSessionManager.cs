namespace Nostradabus.Persistence.Interfaces.Common
{
    /// <summary>
    /// Interfaces that define PersistanceSession.
    /// </summary>
    public interface IPersistanceSessionManager
    {
        void OpenSession();
        void CloseSession();
        void ClearSession();
    }
}