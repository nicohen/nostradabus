using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nostradabus.BusinessEntities;
using Nostradabus.Persistence.Interfaces;
using Nostradabus.Persistence.Nhibernate.Common;
using NHibernate.Linq;

namespace Nostradabus.Persistence.Nhibernate
{
	public class StatisticCheckpointPersistence : PersistenceManager<Checkpoint>, IStatisticCheckpointPersistence
	{
		public List<HistoricalCheckpoint> GetByRoute(Route route, DateTime fromDate, DateTime toDate)
		{
			IQueryable<HistoricalCheckpoint> query = from c in CurrentSession.Query<HistoricalCheckpoint>()
										   where c.DateTime >= fromDate && c.DateTime < toDate && c.Route.ID == route.ID
										   orderby c.DateTime
										   select c;
			return query.ToList();
		}
	}

	
}
