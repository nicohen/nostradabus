using System;
using System.Collections.Generic;
using System.Linq;
using Nostradabus.BusinessEntities;
using Nostradabus.Persistence.Interfaces;
using Nostradabus.Persistence.Nhibernate.Common;
using NHibernate.Linq;

namespace Nostradabus.Persistence.Nhibernate
{
	public class CheckpointPersistence : PersistenceManager<Checkpoint>, ICheckpointPersistence
	{
		public List<Checkpoint> GetByRoute(Route route, DateTime fromDate, DateTime toDate)
		{
			IQueryable<Checkpoint> query = from c in CurrentSession.Query<Checkpoint>()
										   where c.DateTime >= fromDate && c.DateTime < toDate && c.Route.ID == route.ID
										   orderby c.DateTime 
										   select c;
			return query.ToList();
		}
	}
}
