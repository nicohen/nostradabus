namespace Nostradabus.BusinessComponents.Scheduler.Tasks
{
    public abstract class TaskBase
    {
        public abstract void Run();
        public virtual bool ShouldRun()
        {
            return true;
        }
        public virtual void Init()
        {
            
        }
    }
}
