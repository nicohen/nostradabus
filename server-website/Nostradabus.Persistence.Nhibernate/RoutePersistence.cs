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
	public class RoutePersistence : PersistenceManager<Route>, IRoutePersistence
	{
		public IList<RouteStop> GetStopsByRoute(Route route)
		{
			return GetStopsByRoute(route.ID);
		}

		public IList<RouteStop> GetStopsByRoute(int routeId)
		{
			IQueryable<RouteStop> query = from s in CurrentSession.Query<RouteStop>()
										   where s.RouteId == routeId
										   orderby s.Stop
										   select s;
			return query.ToList();
		}

		public IList<int> GetLinesInBoundingBox(BoundingBox boundingBox)
		{
			return CurrentSession.GetNamedQuery("GetLinesInBoundingBox")
				.SetDouble("MinLatitude", boundingBox.MinPoint.Latitude)
				.SetDouble("MinLongitude", boundingBox.MinPoint.Longitude)
				.SetDouble("MaxLatitude", boundingBox.MaxPoint.Latitude)
				.SetDouble("MaxLongitude", boundingBox.MaxPoint.Longitude)
				.List<int>();
		}
	}
}
