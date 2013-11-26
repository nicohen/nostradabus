using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using Nostradabus.BusinessComponents.Common;
using Nostradabus.BusinessEntities;
using Nostradabus.Common;
using Nostradabus.Persistence.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace Nostradabus.BusinessComponents
{
	public class RouteComponent : BusinessComponent<Route>
	{
		#region Protected variables

		static RouteComponent _instance;

		static readonly object Padlock = new object();

		Dictionary<int, Route> _routesCache;

		#endregion

		#region Singleton implementation

		/// <summary>
		/// Gets the instance of RouteComponent.
        /// </summary>
        /// <value>The instance.</value>
        public static RouteComponent Instance
        {
            get
            {
                lock (Padlock)
                {
					return _instance ?? (_instance = new RouteComponent());
                }
            }
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="RouteComponent"/> class.
        /// </summary>
		private RouteComponent()
        {
			Initialize();
        }
		
		#endregion

		#region Methods

		public override ValidationSummary Validate(Route entity)
		{
			var validationSummary = new ValidationSummary();

			if(entity.LineNumber <= 0) validationSummary.Errors.Add(new ValidationError("La línea del recorrido es obligatoria", entity));
			
			return validationSummary;
		}

		/// <summary>
		/// This will retrieve all routes from the cache.
		/// WARNING!!!: Stops property maybe not loaded yet, because it is lazy loaded.
		/// If you need to use Stops you can call GetById() method that will cause the loading of Stops property.
		/// Also you can use the GetStops() method (see the "getFromDatabase" parameter)
		/// </summary>
		/// <returns></returns>
		public override IList<Route> GetAll()
		{
			return _routesCache.Values.ToList();
		}

		public Route GetById(int id)
		{
			if (!_routesCache.ContainsKey(id)) return null;

			var route = _routesCache[id];
			
			// verify if we have to load Stops property
			if(route.Stops == null)
			{
				var stops = GetStops(route).Select(s => new GeoCoordinate(s.Latitude, s.Longitude));

				route.Stops = new List<GeoCoordinate>(stops.Count());

				route.Stops.AddRange(stops);
			}

			return route;
		}
		
		public IList<RouteStop> GetStops(Route route)
		{
			return ServiceLocator.Current.GetInstance<IRoutePersistence>().GetStopsByRoute(route);
		}

		public IList<RouteStop> GetStops(int routeId)
		{
			return ServiceLocator.Current.GetInstance<IRoutePersistence>().GetStopsByRoute(routeId);
		}

		/// <summary>
		/// Obtiene el recorrido para la linea, el ramal y sentido (ida/vuelta) indicados
		/// </summary>
		/// <param name="line"></param>
		/// <param name="branchCode"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		public Route GetRoute(int line, string branchCode, RouteDirection direction)
		{
			//#region Validations

			//if (!_routesCache.ContainsKey(line)) throw new Exception("Line not found");

			//if (_routesCache[line].Branches == null || !_routesCache[line].Branches.Any(b => b.Code.Equals(branchCode))) throw new Exception("Branch not found");

			//#endregion

			//var branch = _routesCache[line].Branches.First(b => b.Code.Equals(branchCode));

			//return direction == RouteDirection.Going ? branch.Going : branch.Return;

			var route = _routesCache.Values.FirstOrDefault(r => r.LineNumber == line && r.BranchCode == branchCode && r.RouteDirection == direction);

			// force complete load for route
			route = route != null
			        	? GetById(route.ID)
			        	: null;

			return route;
		}

		/// <summary>
		/// Get the closest stop index (within the route) 
		/// from specific point sent as param 
		/// </summary>
		/// <param name="route"></param>
		/// <param name="point"></param>
		/// <returns></returns>
		public int GetClosestStop(Route route, GeoCoordinate point)
		{
			// get closest stop using a binary search
			double? distMin = null;
			double? distMax = null;

			int min = 0;
			int max = route.Stops.Count - 1;

			while(min+1 < max)
			{
				int mid = (min + max)/2;

				// calculate distance to each stop
				if (!distMin.HasValue) distMin = GeoHelper.Distance(point, route.Stops[min]);
				if (!distMax.HasValue) distMax = GeoHelper.Distance(point, route.Stops[max]);
				double? distMid = GeoHelper.Distance(point, route.Stops[mid]);
				
				// choose next boundaries based on minimum distance
				if(distMin <= distMid && distMin < distMax) // min point has the minimum distance
				{
					//max = (mid + min)/2;
					max = mid-1;
					distMax = null;
				}
				else if (distMid < distMin && distMid < distMax) // mid point has the minimum distance
				{
					min = (min + mid) / 2;
					distMin = null;

					max = (mid + max) / 2;
					distMax = null;
				}
				else if (distMax < distMin && distMax <= distMid) // max point has the minimum distance
				{
					//min = (mid + max)/2;
					min = mid + 1;
					distMin = null;
				}
				else // distMin == distMax
				{
					max = (mid + min)/2;
					distMax = null;
				}
			}

			if (!distMin.HasValue) distMin = GeoHelper.Distance(point, route.Stops[min]);
			if (!distMax.HasValue) distMax = GeoHelper.Distance(point, route.Stops[max]);

			// this is what we found with the "binary search" heuristic, but this is not exact
			var foundDist = Math.Min(distMin.Value, distMax.Value);
			var foundIndex = distMin < distMax ? min : max;
			
			// therefore, we gonna take a look to the neighbour stops (+/-1)
			// to see if they are closer
			double? distLowerOffset = null, distUpperOffset = null;

			// verify if we have a stop before the found one
			if(foundIndex > 0)
			{
				distLowerOffset = GeoHelper.Distance(point, route.Stops[foundIndex - 1]);
			}

			// verify if we have a stop after the found one
			if (foundIndex < route.Stops.Count - 1)
			{
				distUpperOffset = GeoHelper.Distance(point, route.Stops[foundIndex + 1]);
			}

			// if the stop before the found one is the closest one, take that one
			if (distLowerOffset.HasValue && distLowerOffset.Value < foundDist && (!distUpperOffset.HasValue || distLowerOffset < distUpperOffset))
			{
				return foundIndex - 1;
			}

			// if the stop after the found one is the closest one, take that one
			if (distUpperOffset.HasValue && distUpperOffset.Value < foundDist && (!distLowerOffset.HasValue || distUpperOffset < distLowerOffset))
			{
				return foundIndex + 1;
			}

			// otherwise, what we found in first place is the closest stop
			return foundIndex;
		}

		/// <summary>
		/// Get the closest stop index (within the route) 
		/// from specific point sent as param 
		/// </summary>
		/// <param name="line"></param>
		/// <param name="branchCode"></param>
		/// <param name="direction"></param>
		/// <param name="point"></param>
		/// <returns></returns>
		public int GetClosestStop(int line, string branchCode, RouteDirection direction, GeoCoordinate point)
		{
			var route = GetRoute(line, branchCode, direction);

			return GetClosestStop(route, point);
		}

		/// <summary>
		/// Get the next stop index (within the route) from specific point. 
		/// Assuming the point is traveling through the route.
		/// </summary>
		/// <param name="route"></param>
		/// <param name="point"></param>
		/// <returns></returns>
		public int GetNextStop(Route route, GeoCoordinate point)
		{
			var closestStop = GetClosestStop(route, point);
			int nextStop;

			// now we want to figure out whether the point 
			// is before or after the closest stop (based on route direction)
			if (closestStop == 0) nextStop = 1;
			else if (closestStop == route.Stops.Count - 1) nextStop = closestStop;
			else
			{
				var distanceToPrevStop = GeoHelper.Distance(point, route.Stops[closestStop - 1]);
				var distanceToNextStop = GeoHelper.Distance(point, route.Stops[closestStop + 1]);

				if (distanceToPrevStop <= GetDistanceToPrevStop(route, closestStop) && 
					distanceToNextStop > GetDistanceToNextStop(route, closestStop))
				{
					// the point is before closestStop
					nextStop = closestStop;
				}
				else if (distanceToPrevStop > GetDistanceToPrevStop(route, closestStop) &&
					distanceToNextStop <= GetDistanceToNextStop(route, closestStop))
				{
					// the point is after closestStop
					nextStop = closestStop + 1;
				}
				else // is not clear, we have to choose
				{
					// both distances are greater than the distances between stops
					// decide based on major difference
					if(distanceToPrevStop > GetDistanceToPrevStop(route, closestStop) && distanceToNextStop > GetDistanceToNextStop(route, closestStop))
					{
						var prevDifference = distanceToPrevStop - GetDistanceToPrevStop(route, closestStop);
						var nextDifference = distanceToNextStop - GetDistanceToNextStop(route, closestStop);

						nextStop = nextDifference > prevDifference ? closestStop : closestStop + 1;
					}
					// both distances are lower than the distances between stops
					// decide based on major difference
					else
					{
						// distanceToPrevStop <= GetDistanceToPrevStop(route, closestStop) && distanceToNextStop <= GetDistanceToNextStop(route, closestStop)
						var prevDifference = GetDistanceToPrevStop(route, closestStop) - distanceToPrevStop;
						var nextDifference = GetDistanceToNextStop(route, closestStop) - distanceToNextStop;

						nextStop = prevDifference > nextDifference ? closestStop : closestStop + 1;
					}
				}
			}

			return nextStop;
		}

		public double GetDistanceToStop(Route route, GeoCoordinate point, int toStop)
		{
			#region Validations

			if (toStop < 0 || toStop >= route.Stops.Count) throw new Exception("Invalid destination Stop, must be in the route");

			#endregion Validations

			return GeoHelper.Distance(point, route.Stops[toStop]);
		}

		/// <summary>
		/// Get the distance (in meters) within the route between two stops
		/// </summary>
		/// <param name="route"></param>
		/// <param name="fromStop"></param>
		/// <param name="toStop"></param>
		/// <returns></returns>
		public double GetDistanceBetweenStops(Route route, int fromStop, int toStop)
		{
			return route.DistanceBetweenStops(fromStop, toStop);
		}
		
		/// <summary>
		/// Get the distance (in meters) to the next stop
		/// </summary>
		/// <param name="route"></param>
		/// <param name="fromStop"></param>
		/// <returns></returns>
		public double GetDistanceToNextStop(Route route, int fromStop)
		{
			return route.DistanceToNextStop(fromStop);
		}

		/// <summary>
		/// Get the distance (in meters) to the previous stop
		/// </summary>
		/// <param name="route"></param>
		/// <param name="fromStop"></param>
		/// <returns></returns>
		public double GetDistanceToPrevStop(Route route, int fromStop)
		{
			return route.DistanceToPrevStop(fromStop);
		}

		/// <summary>
		/// Gets the distance between both coordinates but within the route, not just the euclidean distance
		/// </summary>
		/// <param name="route"></param>
		/// <param name="coordinate"></param>
		/// <param name="coordinate2"></param>
		/// <returns></returns>
		public double GetDistanceBetweenPoints(Route route, GeoCoordinate coordinate, GeoCoordinate coordinate2)
		{
			var coordinateNextStop = GetNextStop(route, coordinate);
			var coordinate2NextStop = GetNextStop(route, coordinate2);
			
			// distance from the stop of the fromCheckIndex checkpoint and the stop of the current checkpoint
			// the distance is calculated based on stops because of the grid shape of the map, we dont want the shortest distance
			var distance = GetDistanceBetweenStops(route, coordinateNextStop - 1, coordinate2NextStop - 1);

			var substractDistance = GetDistanceToStop(route, coordinate, coordinateNextStop - 1);

			var aditionalDistance = GetDistanceToStop(route, coordinate2, coordinate2NextStop - 1);

			return distance + aditionalDistance - substractDistance;
		}

		public IList<int> GetNearbyLines(GeoCoordinate coordinate)
		{
			const int nearbyRadius = 400; // meters
			
			// we need a known coordinate, otherwise return empty list
			if (coordinate.IsUnknown) return new int[]{};

			// calculate a nearby bounding box centered in the coordinate
			var boundingBox = GeoHelper.GetBoundingBox(coordinate, nearbyRadius);

			// if bounding box is not defined return empty list
			if(boundingBox == null) return new int[] { };

			return Persistence<IRoutePersistence>().GetLinesInBoundingBox(boundingBox);
		}

		#endregion Methods
		
		#region Private Methods

		public void Initialize()
		{
			_routesCache = new Dictionary<int, Route>(5);

			LoadCache();
		}

		private void LoadCache()
		{
			var routes = Persistence().GetAll();

			foreach (var route in routes)
			{
				// Stops are lazy loaded from the GetById() method

				_routesCache.Add(route.ID, route);
			}
		}

		#endregion Private Methods
	}
}
