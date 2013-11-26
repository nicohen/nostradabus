using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nostradabus.BusinessEntities;
using Nostradabus.Common;
using Nostradabus.Persistence.Interfaces;
using Nostradabus.Persistence.Nhibernate.Common;
using NHibernate.Linq;

namespace Nostradabus.Persistence.Nhibernate
{
	public class StatisticItemPersistence : PersistenceManager<StatisticItem>, IStatisticItemPersistence
	{
		public List<StatisticItem> GetByCalculationAndDayType(StatisticCalculation calculation, DayType dayType)
		{
			IQueryable<StatisticItem> query = from s in CurrentSession.Query<StatisticItem>()
												where s.Calculation.ID == calculation.ID && s.DayType.ID == dayType.ID
												select s;
			return query.ToList();
		}

		public List<StatisticItem> GetByCalculation(StatisticCalculation calculation)
		{
			IQueryable<StatisticItem> query = from s in CurrentSession.Query<StatisticItem>()
											  where s.Calculation.ID == calculation.ID
											  select s;
			
			return query.ToList();
		}

	}
}
