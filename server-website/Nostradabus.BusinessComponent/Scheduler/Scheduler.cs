using System.Collections.Generic;
using System.Timers;
using Nostradabus.BusinessComponents.Scheduler.Tasks;
using Nostradabus.Configuration;
using System;

namespace Nostradabus.BusinessComponents.Scheduler
{
    public class Scheduler
    {
        private Timer _timer;

        #region Singleton implementation

        private static Scheduler _instance;
        protected static readonly object Padlock = new object();

        /// <summary>
        /// Flag that is used to synchronize the timer with the current time, it´s used only the first time that the timer runs.
        /// </summary>
        private static bool _firstUpdate;

        private Scheduler()
        {
            // When the auction scheduler is created (when the application starts), runs the update process.
//            Run();

            _firstUpdate = true;

            if (ShouldStartTimer)
            {
                LoadTasks();
                InitializeTimer();
            }
        }

        /// <summary>
        /// Gets the singleton instance of the class (which starts the timer) and saves the httpcontext
        /// </summary>
        /// <returns>The singleton instance of this class</returns>
        public static Scheduler Start()
        {
            lock (Padlock)
            {
                return _instance ?? (_instance = new Scheduler());
            }
        }

        public static Scheduler Instance{get { return _instance; }}

        #endregion

        #region Properties

        /// <summary>
        /// Sets the default interval of the timer, if not specified it will use the one defined in the config file.
        /// </summary>
        public int? DefaultInterval { get; set; }

        public double Interval { get { return _timer.Interval; }}

        /// <summary>
        /// Sets the auction scheduler configuration object
        /// </summary>
        public static ConfigurationManager.SchedulerConfiguration SchedulerConfiguration { get; set; }

        /// <summary>
        /// Tells if the timer is enabled in the configuration, if not the timer will not start
        /// </summary>
        public bool ShouldStartTimer
        {
            get
            {
                return (SchedulerConfiguration != null)
                           ? SchedulerConfiguration.Enabled
                           : ConfigurationManager.Instance.SchedulerSettings.Enabled;
            }
        }

        #endregion

    	readonly List<TaskBase> _tasks = new List<TaskBase>();

        #region Methods

        /// <summary>
        /// Initializes a timer object that will check the server time.
        /// </summary>
        private void InitializeTimer()
        {
            _timer = new Timer();

            // Sets the timer interval from the config file if not specified manually
            var interval = (SchedulerConfiguration != null)
                                ? SchedulerConfiguration.Interval
                                : ConfigurationManager.Instance.SchedulerSettings.Interval;

            if (_firstUpdate)
            {
                //var currentDate = DateTimeHelper.Now();
                                                               // TODO: Check the initial time interval
                _timer.Interval = DefaultInterval ?? interval; //GetFirstTimeInterval(currentDate, interval);
            }
            else
            {
                _timer.Interval = DefaultInterval ?? interval;
            }
            
			_timer.Elapsed += ServerTimerElapsed;

            _timer.Start();
        }
        private void LoadTasks()
        {
            var userCleanup = new UserCleanUpTask();
            userCleanup.Init();
            _tasks.Add(userCleanup);
                    
        }
        /// <summary>
        /// Gets the timer interval to be used the first time that the timer is started. In order to synchronize the timer
        /// with the current time.
        /// </summary>
        internal float GetFirstTimeInterval(DateTime currentDate, long interval)
        {
            int secondsToCompare = (currentDate.Minute <= 30) ? 1800 : 3600;

            // Gets the time left to half-hour or next hour (in seconds)
            int secondsLeftToHalfHour = secondsToCompare - ((currentDate.Minute*60) + currentDate.Second);

            // Gets the configured interval in seconds
            long intervalInSeconds = interval/1000;

            // If the interval is greater than the time left to the next half/hour use the remainer as the first interval 
            if(secondsLeftToHalfHour <= intervalInSeconds)
                return secondsLeftToHalfHour*1000;

            // Otherwise use the remainder of dividing the time left by the interval
            return (intervalInSeconds - (secondsLeftToHalfHour % intervalInSeconds)) * 1000;
        }

        internal void ServerTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_firstUpdate)
            {
                _firstUpdate = false;
                InitializeTimer();
            }
                
            Run();
		}

        #region Private methods

        /// <summary>
        /// Updates the auctions
        /// </summary>
        private void Run()
        {
            lock (Padlock)
            {
                foreach (var taskBase in _tasks)
                {
                    if (taskBase.ShouldRun())
                        taskBase.Run();
                }
            }
        }

        #endregion

        #endregion
    }
}
