using System;
using System.Collections.Generic;
using Nostradabus.BusinessEntities;
using Nostradabus.Persistence.Interfaces.Common;

namespace Nostradabus.Persistence.Interfaces
{
    /// <summary>
    /// Interface of Persistence for HolidaySchedule.
    /// </summary>
    public interface IHolidaySchedulePersistence : IPersistence<HolidaySchedule>
    {
        /// <summary>
        /// Gets the holiday schedule by date.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        HolidaySchedule GetHolidayScheduleByDate(DateTime dateTime);

		IList<HolidaySchedule> GetByYear(int year);

    	int GetCount();
    }
}
