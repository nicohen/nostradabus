using System;
using System.Collections.Generic;
using System.Linq;
using Nostradabus.BusinessEntities;
using Nostradabus.Persistence.Interfaces;
using Nostradabus.Persistence.Nhibernate.Common;
using NHibernate.Linq;

namespace Nostradabus.Persistence.Nhibernate
{
    public class HolidaySchedulePersistence : PersistenceManager<HolidaySchedule>,IHolidaySchedulePersistence
    {
        #region IHolidaySchedulePersistence Members

		public int GetCount()
		{
			var query = CurrentSession.CreateSQLQuery("SELECT * FROM holiday_schedule");

			return query.List().Count;
		}

    	public HolidaySchedule GetHolidayScheduleByDate(DateTime dateTime)
        {
            IQueryable<HolidaySchedule> query = from hs in CurrentSession.Query<HolidaySchedule>()
                                                where hs.Date==dateTime.Date && (hs.Active == true)
                                                select hs;
            return query.FirstOrDefault();
        }

    	public IList<HolidaySchedule> GetByYear(int year)
    	{
			var fromDate = new DateTime(year, 1, 1);
			var toDate = new DateTime(year + 1, 1, 1);

			IQueryable<HolidaySchedule> query = from hs in CurrentSession.Query<HolidaySchedule>()
												where hs.Date >= fromDate && hs.Date < toDate 
												select hs;
			return query.ToList();
    	}

    	#endregion
    }
}
