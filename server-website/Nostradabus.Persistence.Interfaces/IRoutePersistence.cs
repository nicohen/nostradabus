﻿using System.Collections.Generic;
using Nostradabus.BusinessEntities;
using Nostradabus.Persistence.Interfaces.Common;

namespace Nostradabus.Persistence.Interfaces
{
	public interface IRoutePersistence : IPersistence<Route>
	{
		IList<RouteStop> GetStopsByRoute(Route route);
		IList<RouteStop> GetStopsByRoute(int routeId);
	}
}
