using System;
using Nostradabus.Common;

namespace Nostradabus.BusinessComponents.Scheduler.Tasks
{
    public class UserCleanUpTask : TaskBase
    {
        private DateTime _lastDayRunned;
        private TimeSpan _runningTime;
        private bool _isRunning;
        
        public override void Init()
        {
            _runningTime = GetRunningTime();
        }

        private static TimeSpan GetRunningTime()
        {
            string time = Configuration.ConfigurationManager.Instance.GetSchedulerSetting("UserCleanUpTime");
            return DateTimeHelper.ParseShortTime(time);
        }
        public override void Run()
        {
            _isRunning = true;
            

            _lastDayRunned = DateTimeHelper.Now();
            _isRunning = false;
        }
        public override bool ShouldRun()
        {
            if (_isRunning) return false;
            if (_lastDayRunned.Date == DateTimeHelper.Now().Date) return false;
            return _runningTime <= DateTimeHelper.Now().TimeOfDay;
        }
    }
}
