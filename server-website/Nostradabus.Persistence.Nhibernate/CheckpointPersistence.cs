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

		public int InsertDataEntryCheckpoint(DataEntryCheckpoint dataEntryCheckpoint)
		{
			return CurrentSession.GetNamedQuery("InsertDataEntryCheckpoint")
				.SetString("SerialNumber", dataEntryCheckpoint.SerialNumber)
				.SetInt32("LineNumber", dataEntryCheckpoint.LineNumber)
				.SetDouble("Latitude", dataEntryCheckpoint.Coordinate.Latitude)
				.SetDouble("Longitude", dataEntryCheckpoint.Coordinate.Longitude)
				.SetDateTime("UserDate", dataEntryCheckpoint.UserDateTime)
				.SetDateTime("Date", dataEntryCheckpoint.DateTime)
				.UniqueResult<int>();
		}
	}
}
