using System;
using System.Collections.Generic;
using Nostradabus.BusinessComponents.Common;
using Nostradabus.BusinessEntities;
using Nostradabus.Common;
using Nostradabus.Persistence.Interfaces;

namespace Nostradabus.BusinessComponents
{
    public class HolidayScheduleComponent : BusinessComponent<HolidaySchedule>
    {
		#region Protected variables

		static HolidayScheduleComponent _instance;

		static readonly object Padlock = new object();

		Dictionary<DateTime, HolidaySchedule> _yearCache;

    	private int _currentYear;

		#endregion

		#region Singleton implementation

		/// <summary>
		/// Gets the instance of HolidayScheduleComponent.
        /// </summary>
        /// <value>The instance.</value>
        public static HolidayScheduleComponent Instance
        {
            get
            {
                lock (Padlock)
                {
					return _instance ?? (_instance = new HolidayScheduleComponent());
                }
            }
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="RouteComponent"/> class.
        /// </summary>
		private HolidayScheduleComponent()
        {
			Initialize();
        }
		
		#endregion

		#region Methods

		public int GetCount()
		{
			var count = Persistence<IHolidaySchedulePersistence>().GetCount();
			return count;
		}

    	public override ValidationSummary Validate(HolidaySchedule entity)
		{
			var result = new ValidationSummary();
			
			return result;
		}

		public void ReloadCache()
		{
			var year = DateTimeHelper.Today().Year;

			var holidays = GetByYear(year);

			_yearCache.Clear();

			foreach (var holiday in holidays)
			{
				_yearCache.Add(holiday.Date.Date, holiday);
			}

			_currentYear = year;
		}

		/// <summary>
        /// Determines whether the specified date time is holiday.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>
        /// 	<c>true</c> if the specified date time is holiday; otherwise, <c>false</c>.
        /// </returns>
        public bool IsHoliday(DateTime dateTime)
		{
			VerifyYearCache();

			if(dateTime.Year != _currentYear)
			{
				 var holidaydate = Persistence<IHolidaySchedulePersistence>().GetHolidayScheduleByDate(dateTime);
				 return holidaydate != null && holidaydate.Active;
			}

			return _yearCache.ContainsKey(dateTime) && _yearCache[dateTime].Active;
        }

		public bool IsLaboralDay(DateTime dateTime)
		{
			VerifyYearCache();

			if (dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday || IsHoliday(dateTime))
				return false;

			return true;
		}

    	/// <summary>
        /// Gets the first labor day.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public DateTime GetFirstLaborDay(DateTime dateTime)
        {
            while (!IsLaboralDay(dateTime))
            {
                dateTime = dateTime.AddDays(1);
            }

            return dateTime;
        }
		
        /// <summary>
        /// Gets the first labor day. Bypass the number of working days to be spent.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="quantity">Number of working days to be spent.</param>
        /// <returns></returns>
        public DateTime GetFirstLaborDay(DateTime dateTime, int quantity)
        {
			for (int i = 0; i < quantity; i++)
			{
				dateTime = dateTime.AddDays(1);
				dateTime = GetFirstLaborDay(dateTime);
			}
			
            return dateTime;
		}

		public IList<HolidaySchedule> GetByYear(int year)
		{
			return Persistence<IHolidaySchedulePersistence>().GetByYear(year);
		}

    	#endregion Methods

		#region Private Methods

		private void Initialize()
		{
			_yearCache = new Dictionary<DateTime, HolidaySchedule>(10);

			ReloadCache();
		}

		private void VerifyYearCache()
		{
			var year = DateTimeHelper.Today().Year;

			if(year != _currentYear) ReloadCache();
		}
		
    	#endregion Private Methods
    }
}
