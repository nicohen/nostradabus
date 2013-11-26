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

		public Route GetById(int id)
		{
			if (!_routesCache.ContainsKey(id)) return null;
			
			return _routesCache[id];
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

			return _routesCache.Values.FirstOrDefault(r => r.LineNumber == line && r.BranchCode == branchCode && r.RouteDirection == direction);
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
			const int nearbyRadius = 200; // meters
			
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
				var stops = GetStops(route).Select(s => new GeoCoordinate(s.Latitude, s.Longitude));

				route.Stops = new List<GeoCoordinate>(stops.Count());
					
				route.Stops.AddRange(stops);

				_routesCache.Add(route.ID, route);
			}
			
			//#region Line 71
			
			//_routesCache.Add(1, new Route(71, "A", RouteDirection.Going) { Stops = { new GeoCoordinate(-34.609068, -58.40889800000002), new GeoCoordinate(-34.608909999999995, -58.406131000000016), new GeoCoordinate(-34.608802999999995, -58.404736000000014), new GeoCoordinate(-34.607585, -58.40480000000002), new GeoCoordinate(-34.606526, -58.40471500000001), new GeoCoordinate(-34.60656, -58.405799), new GeoCoordinate(-34.606667, -58.40886699999999), new GeoCoordinate(-34.606772, -58.41006900000002), new GeoCoordinate(-34.606366, -58.41234199999997), new GeoCoordinate(-34.606067, -58.41378099999997), new GeoCoordinate(-34.605767, -58.414918), new GeoCoordinate(-34.605430999999996, -58.41725600000001), new GeoCoordinate(-34.605024, -58.42026099999998), new GeoCoordinate(-34.604883, -58.421246999999994), new GeoCoordinate(-34.604618, -58.423265000000015), new GeoCoordinate(-34.604441, -58.424401999999986), new GeoCoordinate(-34.604264, -58.42577499999999), new GeoCoordinate(-34.603912, -58.43210499999998), new GeoCoordinate(-34.604335999999996, -58.43324200000001), new GeoCoordinate(-34.603011, -58.433820999999966), new GeoCoordinate(-34.602321999999994, -58.43448799999999), new GeoCoordinate(-34.603948, -58.436954000000014), new GeoCoordinate(-34.603082, -58.43777), new GeoCoordinate(-34.60045, -58.440000999999995), new GeoCoordinate(-34.595399, -58.445667000000014), new GeoCoordinate(-34.59485, -58.44626800000003), new GeoCoordinate(-34.597289, -58.449442999999974), new GeoCoordinate(-34.596121999999994, -58.45066700000001), new GeoCoordinate(-34.595186999999996, -58.45171700000003), new GeoCoordinate(-34.594039, -58.450237000000016), new GeoCoordinate(-34.592554, -58.448734), new GeoCoordinate(-34.591901, -58.44952799999999), new GeoCoordinate(-34.590946, -58.45045099999999), new GeoCoordinate(-34.589658, -58.45180400000004), new GeoCoordinate(-34.588827, -58.451009), new GeoCoordinate(-34.588526, -58.451031), new GeoCoordinate(-34.587873, -58.452661000000035), new GeoCoordinate(-34.586937999999996, -58.45504399999999), new GeoCoordinate(-34.586106, -58.45446400000003), new GeoCoordinate(-34.585205, -58.453971000000024), new GeoCoordinate(-34.586158999999995, -58.45309099999997), new GeoCoordinate(-34.584887, -58.452382), new GeoCoordinate(-34.583863, -58.45178199999998), new GeoCoordinate(-34.583297, -58.452877), new GeoCoordinate(-34.582962, -58.45392700000002), new GeoCoordinate(-34.582574, -58.45534299999997), new GeoCoordinate(-34.582096, -58.45673799999997), new GeoCoordinate(-34.581867, -58.45841300000001), new GeoCoordinate(-34.581867, -58.46189900000002), new GeoCoordinate(-34.585118, -58.46207100000004), new GeoCoordinate(-34.585011, -58.46344499999998), new GeoCoordinate(-34.582979, -58.469817000000035), new GeoCoordinate(-34.581654, -58.47389399999997), new GeoCoordinate(-34.579994, -58.47537399999999), new GeoCoordinate(-34.579076, -58.47900200000004), new GeoCoordinate(-34.575543, -58.484216), new GeoCoordinate(-34.574782, -58.482958999999994), new GeoCoordinate(-34.573121, -58.48422500000004), new GeoCoordinate(-34.573829, -58.48536300000001), new GeoCoordinate(-34.573121, -58.48602900000003), new GeoCoordinate(-34.568174, -58.48997700000001), new GeoCoordinate(-34.563721, -58.49332400000003), new GeoCoordinate(-34.560788, -58.48729500000002), new GeoCoordinate(-34.559463, -58.48813200000001), new GeoCoordinate(-34.558421, -58.48885999999999), new GeoCoordinate(-34.557147, -58.486113999999986), new GeoCoordinate(-34.556264999999996, -58.486672999999996), new GeoCoordinate(-34.554957, -58.487551999999994), new GeoCoordinate(-34.55501, -58.485900000000015), new GeoCoordinate(-34.554816, -58.48514799999998), new GeoCoordinate(-34.555275, -58.48487), new GeoCoordinate(-34.555804, -58.48562100000004), new GeoCoordinate(-34.553453999999995, -58.487251000000015), new GeoCoordinate(-34.553082999999994, -58.48662899999999), new GeoCoordinate(-34.551634, -58.48383999999999), new GeoCoordinate(-34.550962999999996, -58.48433299999999), new GeoCoordinate(-34.550131, -58.484977000000015), new GeoCoordinate(-34.549813, -58.48418300000003), new GeoCoordinate(-34.549566999999996, -58.48375299999998), new GeoCoordinate(-34.548665, -58.48216600000001), new GeoCoordinate(-34.548118, -58.482596), new GeoCoordinate(-34.54757, -58.483046), new GeoCoordinate(-34.546756, -58.48362599999996), new GeoCoordinate(-34.546279999999996, -58.483925), new GeoCoordinate(-34.545572, -58.48446200000001), new GeoCoordinate(-34.544864999999994, -58.483068), new GeoCoordinate(-34.544104999999995, -58.481844000000024), new GeoCoordinate(-34.543399, -58.47931199999999), new GeoCoordinate(-34.541807999999996, -58.47484800000001), new GeoCoordinate(-34.541542, -58.473817999999994), new GeoCoordinate(-34.540376, -58.47461199999998), new GeoCoordinate(-34.539226, -58.47540600000002), new GeoCoordinate(-34.538219999999995, -58.47611499999999), new GeoCoordinate(-34.536912, -58.47684400000003), new GeoCoordinate(-34.53524899999999, -58.47776699999997), new GeoCoordinate(-34.532067999999995, -58.47931199999999), new GeoCoordinate(-34.529488, -58.48036400000001), new GeoCoordinate(-34.524484, -58.483517000000006), new GeoCoordinate(-34.522399, -58.484504000000015), new GeoCoordinate(-34.520578, -58.48538500000001), new GeoCoordinate(-34.517907, -58.48693000000003), new GeoCoordinate(-34.515732, -58.488067), new GeoCoordinate(-34.513098, -58.489247999999975), new GeoCoordinate(-34.511277, -58.49055499999997), new GeoCoordinate(-34.508536, -58.492121999999995), new GeoCoordinate(-34.506273, -58.49336600000004), new GeoCoordinate(-34.504734, -58.493967999999995), new GeoCoordinate(-34.500615, -58.496028000000024), new GeoCoordinate(-34.497785, -58.497615999999994), new GeoCoordinate(-34.499872, -58.50212099999999), new GeoCoordinate(-34.50157, -58.50562000000002), new GeoCoordinate(-34.502789, -58.508366000000024), new GeoCoordinate(-34.50348, -58.50976000000003), new GeoCoordinate(-34.504027, -58.51111300000002), new GeoCoordinate(-34.506078, -58.51568299999997), new GeoCoordinate(-34.507829, -58.51947999999999), new GeoCoordinate(-34.510216, -58.52486699999997), new GeoCoordinate(-34.511313, -58.527228000000036), new GeoCoordinate(-34.513186999999995, -58.531132000000014), new GeoCoordinate(-34.513805999999995, -58.531926999999996), new GeoCoordinate(-34.513839999999995, -58.53204900000003), new GeoCoordinate(-34.513275, -58.532489999999996), new GeoCoordinate(-34.514654, -58.534329000000014), new GeoCoordinate(-34.514265, -58.534888000000024), new GeoCoordinate(-34.517395, -58.53853600000002), new GeoCoordinate(-34.522732999999995, -58.54510099999999), new GeoCoordinate(-34.523459, -58.544264999999996), new GeoCoordinate(-34.523954, -58.54362100000003), new GeoCoordinate(-34.524661, -58.54452200000003), new GeoCoordinate(-34.525404, -58.54359899999997) } });
			//_routesCache.Add(2, new Route(71, "A", RouteDirection.Return) { Stops = { new GeoCoordinate(-34.524962, -58.544190000000015), new GeoCoordinate(-34.523583, -58.54594900000001), new GeoCoordinate(-34.521126, -58.54317100000003), new GeoCoordinate(-34.518703, -58.540188), new GeoCoordinate(-34.51842, -58.53988700000002), new GeoCoordinate(-34.519604, -58.538298999999995), new GeoCoordinate(-34.51865, -58.53705500000001), new GeoCoordinate(-34.516563999999995, -58.534373000000016), new GeoCoordinate(-34.515309, -58.53263400000003), new GeoCoordinate(-34.514265, -58.53050999999999), new GeoCoordinate(-34.513117, -58.531218999999965), new GeoCoordinate(-34.509421, -58.52345100000002), new GeoCoordinate(-34.506096, -58.51555400000001), new GeoCoordinate(-34.500757, -58.50388099999998), new GeoCoordinate(-34.497749999999996, -58.49757299999999), new GeoCoordinate(-34.499518, -58.49669299999999), new GeoCoordinate(-34.504504999999995, -58.494140000000016), new GeoCoordinate(-34.508218, -58.492187), new GeoCoordinate(-34.510746999999995, -58.490770999999995), new GeoCoordinate(-34.513734, -58.48903200000001), new GeoCoordinate(-34.515804, -58.48798199999999), new GeoCoordinate(-34.517023, -58.487617), new GeoCoordinate(-34.518278, -58.48675800000001), new GeoCoordinate(-34.52169, -58.484977000000015), new GeoCoordinate(-34.525863, -58.48274500000002), new GeoCoordinate(-34.528798, -58.48098600000003), new GeoCoordinate(-34.529788, -58.48044900000002), new GeoCoordinate(-34.535320999999996, -58.47759500000001), new GeoCoordinate(-34.538396999999996, -58.47622200000001), new GeoCoordinate(-34.539687, -58.475279), new GeoCoordinate(-34.540022, -58.47529900000001), new GeoCoordinate(-34.540534, -58.475213999999994), new GeoCoordinate(-34.540923, -58.475126999999986), new GeoCoordinate(-34.541559, -58.476608999999996), new GeoCoordinate(-34.542197, -58.47881899999999), new GeoCoordinate(-34.542568, -58.479934000000014), new GeoCoordinate(-34.542938, -58.48111499999999), new GeoCoordinate(-34.543309, -58.482272999999964), new GeoCoordinate(-34.543751, -58.48308800000001), new GeoCoordinate(-34.544494, -58.48442), new GeoCoordinate(-34.545183, -58.485964999999965), new GeoCoordinate(-34.544511, -58.48648000000003), new GeoCoordinate(-34.544616999999995, -58.48697199999998), new GeoCoordinate(-34.545042, -58.48752999999999), new GeoCoordinate(-34.546332, -58.48654399999998), new GeoCoordinate(-34.551377, -58.48267499999997), new GeoCoordinate(-34.551811, -58.48230000000001), new GeoCoordinate(-34.552323, -58.48316999999997), new GeoCoordinate(-34.552968, -58.484208999999964), new GeoCoordinate(-34.55379, -58.485937000000035), new GeoCoordinate(-34.554462, -58.48548600000004), new GeoCoordinate(-34.554886, -58.485112000000015), new GeoCoordinate(-34.554938, -58.485915999999975), new GeoCoordinate(-34.554886, -58.486527000000024), new GeoCoordinate(-34.554912, -58.48754600000001), new GeoCoordinate(-34.554603, -58.487814000000014), new GeoCoordinate(-34.553931999999996, -58.48825499999998), new GeoCoordinate(-34.554125, -58.488674), new GeoCoordinate(-34.554374, -58.48917799999998), new GeoCoordinate(-34.555504, -58.488405), new GeoCoordinate(-34.556238, -58.48786899999999), new GeoCoordinate(-34.558552999999996, -58.486238000000014), new GeoCoordinate(-34.560372, -58.490003), new GeoCoordinate(-34.56123, -58.49178499999999), new GeoCoordinate(-34.561681, -58.49280299999998), new GeoCoordinate(-34.562387, -58.49416600000001), new GeoCoordinate(-34.563501, -58.496645), new GeoCoordinate(-34.56374, -58.496388000000024), new GeoCoordinate(-34.564067, -58.496172), new GeoCoordinate(-34.569977, -58.49175300000002), new GeoCoordinate(-34.570481, -58.491399), new GeoCoordinate(-34.571036, -58.490904), new GeoCoordinate(-34.572679, -58.489575), new GeoCoordinate(-34.572361, -58.488887999999974), new GeoCoordinate(-34.573678, -58.487054), new GeoCoordinate(-34.574314, -58.486387000000036), new GeoCoordinate(-34.57488, -58.48551800000001), new GeoCoordinate(-34.57549, -58.48443600000002), new GeoCoordinate(-34.575868, -58.48389900000001), new GeoCoordinate(-34.576734, -58.48262199999999), new GeoCoordinate(-34.577397999999995, -58.48152700000003), new GeoCoordinate(-34.577874, -58.48087399999997), new GeoCoordinate(-34.578404000000006, -58.48013300000002), new GeoCoordinate(-34.579067, -58.47909200000004), new GeoCoordinate(-34.579543, -58.47707500000001), new GeoCoordinate(-34.579923, -58.475476000000015), new GeoCoordinate(-34.581152, -58.47443499999997), new GeoCoordinate(-34.581654, -58.47393099999999), new GeoCoordinate(-34.582158, -58.47228999999999), new GeoCoordinate(-34.58229, -58.471903999999995), new GeoCoordinate(-34.582706, -58.47084100000001), new GeoCoordinate(-34.583112, -58.46954299999999), new GeoCoordinate(-34.583535999999995, -58.46828800000003), new GeoCoordinate(-34.583748, -58.46749399999999), new GeoCoordinate(-34.584014, -58.466421999999966), new GeoCoordinate(-34.584226, -58.46582000000001), new GeoCoordinate(-34.584623, -58.46476999999999), new GeoCoordinate(-34.585099, -58.46342800000002), new GeoCoordinate(-34.585161, -58.460757), new GeoCoordinate(-34.585109, -58.46056299999998), new GeoCoordinate(-34.584243, -58.46054300000003), new GeoCoordinate(-34.582926, -58.46058500000004), new GeoCoordinate(-34.582971, -58.45945900000004), new GeoCoordinate(-34.582988, -58.45882599999999), new GeoCoordinate(-34.583404, -58.45756), new GeoCoordinate(-34.584288, -58.45485500000001), new GeoCoordinate(-34.585161, -58.453985999999986), new GeoCoordinate(-34.585921, -58.45444800000001), new GeoCoordinate(-34.586937999999996, -58.455123000000015), new GeoCoordinate(-34.587184, -58.45527500000003), new GeoCoordinate(-34.587353, -58.45515599999999), new GeoCoordinate(-34.587626, -58.454522999999995), new GeoCoordinate(-34.588138, -58.453010000000006), new GeoCoordinate(-34.588774, -58.450918), new GeoCoordinate(-34.589445999999995, -58.44998399999997), new GeoCoordinate(-34.590868, -58.44846100000001), new GeoCoordinate(-34.594692, -58.444394999999986), new GeoCoordinate(-34.595779, -58.44316200000003), new GeoCoordinate(-34.596360999999995, -58.442678), new GeoCoordinate(-34.597174, -58.44172400000002), new GeoCoordinate(-34.5981, -58.44075799999996), new GeoCoordinate(-34.59918699999999, -58.43949199999997), new GeoCoordinate(-34.599805999999994, -58.43889200000001), new GeoCoordinate(-34.599981, -58.438514999999995), new GeoCoordinate(-34.600282, -58.43754999999999), new GeoCoordinate(-34.601872, -58.43277499999999), new GeoCoordinate(-34.602039999999995, -58.432120999999995), new GeoCoordinate(-34.602321999999994, -58.429236), new GeoCoordinate(-34.602888, -58.424041999999986), new GeoCoordinate(-34.603382, -58.41958999999997), new GeoCoordinate(-34.60362, -58.416597000000024), new GeoCoordinate(-34.604017999999996, -58.41481599999997), new GeoCoordinate(-34.604176, -58.413152999999966), new GeoCoordinate(-34.604044, -58.410567000000015), new GeoCoordinate(-34.604061, -58.409193000000016), new GeoCoordinate(-34.604194, -58.40805599999999), new GeoCoordinate(-34.604469, -58.40659800000003), new GeoCoordinate(-34.604582, -58.40547000000004), new GeoCoordinate(-34.605588999999995, -58.405686), new GeoCoordinate(-34.606676, -58.40597600000001), new GeoCoordinate(-34.60763, -58.40612599999997), new GeoCoordinate(-34.608388999999995, -58.40606100000002), new GeoCoordinate(-34.610137, -58.40596500000004), new GeoCoordinate(-34.610191, -58.40739199999996), new GeoCoordinate(-34.610331, -58.40867800000001), new GeoCoordinate(-34.610331, -58.40892600000001), new GeoCoordinate(-34.60936, -58.40893700000004), new GeoCoordinate(-34.609104, -58.408839), new GeoCoordinate(-34.608988999999994, -58.40749900000003) }});

			//#endregion Line 71

			//#region Line 42
			
			//_routesCache.Add(3, new Route(42, "A", RouteDirection.Going) { Stops = { new GeoCoordinate(-34.651797, -58.40756399999998), new GeoCoordinate(-34.653791999999996, -58.410217999999986), new GeoCoordinate(-34.654144, -58.41130199999998), new GeoCoordinate(-34.654789, -58.413017999999965), new GeoCoordinate(-34.655442, -58.414176999999995), new GeoCoordinate(-34.656686, -58.41644199999996), new GeoCoordinate(-34.655794, -58.416450999999995), new GeoCoordinate(-34.654198, -58.41634399999998), new GeoCoordinate(-34.652546, -58.41636699999998), new GeoCoordinate(-34.651831, -58.416349999999966), new GeoCoordinate(-34.651792, -58.41524400000003), new GeoCoordinate(-34.651637, -58.414484000000016), new GeoCoordinate(-34.651713, -58.414413999999965), new GeoCoordinate(-34.651788, -58.41444000000001), new GeoCoordinate(-34.651862, -58.41477900000001), new GeoCoordinate(-34.651986, -58.41514799999999), new GeoCoordinate(-34.65218, -58.416135999999995), new GeoCoordinate(-34.65196, -58.41658100000001), new GeoCoordinate(-34.651814, -58.41773999999998), new GeoCoordinate(-34.65162, -58.41920900000002), new GeoCoordinate(-34.65144, -58.42081400000001), new GeoCoordinate(-34.651099, -58.423790999999994), new GeoCoordinate(-34.647891, -58.428248999999994), new GeoCoordinate(-34.645147, -58.43207899999999), new GeoCoordinate(-34.644171, -58.43216899999999), new GeoCoordinate(-34.641712999999996, -58.432293000000016), new GeoCoordinate(-34.640627, -58.43239399999999), new GeoCoordinate(-34.638235, -58.43257700000004), new GeoCoordinate(-34.63624, -58.43363899999997), new GeoCoordinate(-34.635489, -58.434347), new GeoCoordinate(-34.635369999999995, -58.43437499999999), new GeoCoordinate(-34.635131, -58.43409099999997), new GeoCoordinate(-34.634872, -58.43255499999998), new GeoCoordinate(-34.632461, -58.43286699999999), new GeoCoordinate(-34.631433, -58.433021999999994), new GeoCoordinate(-34.629763999999994, -58.433381999999995), new GeoCoordinate(-34.627977, -58.433772999999974), new GeoCoordinate(-34.626767, -58.43407400000001), new GeoCoordinate(-34.625827, -58.43428799999998), new GeoCoordinate(-34.623000999999995, -58.43462599999998), new GeoCoordinate(-34.621245, -58.435130000000015), new GeoCoordinate(-34.620407, -58.435490000000016), new GeoCoordinate(-34.619306, -58.43589800000001), new GeoCoordinate(-34.618234, -58.43646100000001), new GeoCoordinate(-34.616557, -58.43733599999996), new GeoCoordinate(-34.615929, -58.43758200000002), new GeoCoordinate(-34.614031999999995, -58.43780700000002), new GeoCoordinate(-34.608839999999994, -58.440076999999974), new GeoCoordinate(-34.606495, -58.440147000000024), new GeoCoordinate(-34.605884999999994, -58.440157), new GeoCoordinate(-34.605805999999994, -58.440382), new GeoCoordinate(-34.605174999999996, -58.44100400000002), new GeoCoordinate(-34.604317, -58.44183599999997), new GeoCoordinate(-34.60279499999999, -58.443354), new GeoCoordinate(-34.60139, -58.444717999999966), new GeoCoordinate(-34.597791, -58.448751000000016), new GeoCoordinate(-34.596497, -58.450166999999965), new GeoCoordinate(-34.595186999999996, -58.45163100000002), new GeoCoordinate(-34.594347, -58.45058), new GeoCoordinate(-34.592611999999995, -58.448639000000014), new GeoCoordinate(-34.589639999999996, -58.45181500000001), new GeoCoordinate(-34.588827, -58.450999000000024), new GeoCoordinate(-34.588717, -58.450976999999966), new GeoCoordinate(-34.588607, -58.451031), new GeoCoordinate(-34.588478, -58.451153999999974), new GeoCoordinate(-34.588307, -58.45146999999997), new GeoCoordinate(-34.586954999999996, -58.455074999999965), new GeoCoordinate(-34.585143, -58.45395500000001), new GeoCoordinate(-34.582442, -58.45222699999999), new GeoCoordinate(-34.580449, -58.45095500000002), new GeoCoordinate(-34.578034, -58.44948599999998), new GeoCoordinate(-34.577061, -58.448938999999996), new GeoCoordinate(-34.574813999999996, -58.447721), new GeoCoordinate(-34.573528, -58.44705099999999), new GeoCoordinate(-34.571584, -58.44583799999998), new GeoCoordinate(-34.569839, -58.444818999999995), new GeoCoordinate(-34.565342, -58.440372000000025), new GeoCoordinate(-34.564154, -58.439278), new GeoCoordinate(-34.563032, -58.43830700000001), new GeoCoordinate(-34.562891, -58.438289999999995), new GeoCoordinate(-34.562819999999995, -58.43834900000002), new GeoCoordinate(-34.561883, -58.442511999999965), new GeoCoordinate(-34.56172, -58.44330000000002), new GeoCoordinate(-34.56149, -58.44410099999999), new GeoCoordinate(-34.561354, -58.44457699999998), new GeoCoordinate(-34.560986, -58.445447), new GeoCoordinate(-34.560386, -58.446609999999964), new GeoCoordinate(-34.559767, -58.448171), new GeoCoordinate(-34.559096999999994, -58.449342), new GeoCoordinate(-34.558509, -58.450402999999994), new GeoCoordinate(-34.558473, -58.45065099999999), new GeoCoordinate(-34.558726, -58.451153999999974), new GeoCoordinate(-34.557802, -58.45193699999999), new GeoCoordinate(-34.557113, -58.450852999999995), new GeoCoordinate(-34.556422999999995, -58.44974400000001), new GeoCoordinate(-34.557386, -58.448916999999994), new GeoCoordinate(-34.558306, -58.44810100000001), new GeoCoordinate(-34.556974999999994, -58.44583799999998), new GeoCoordinate(-34.556312999999996, -58.44473800000003), new GeoCoordinate(-34.555311, -58.44306), new GeoCoordinate(-34.554312, -58.441385999999966), new GeoCoordinate(-34.55167, -58.43683599999997), new GeoCoordinate(-34.551417, -58.43689499999999), new GeoCoordinate(-34.549619, -58.438413999999966), new GeoCoordinate(-34.549261, -58.438800000000015), new GeoCoordinate(-34.548828, -58.43939599999999), new GeoCoordinate(-34.54825, -58.44052199999999), new GeoCoordinate(-34.547869999999996, -58.44152600000001), new GeoCoordinate(-34.547732999999994, -58.442470000000014), new GeoCoordinate(-34.547096999999994, -58.44665800000001), new GeoCoordinate(-34.546227, -58.452049999999986), new GeoCoordinate(-34.543483, -58.44983500000001), new GeoCoordinate(-34.54316, -58.44943799999999), new GeoCoordinate(-34.543093999999996, -58.44918999999999), new GeoCoordinate(-34.543081, -58.44899800000002), new GeoCoordinate(-34.543163, -58.44859600000001), new GeoCoordinate(-34.543278, -58.448188000000016), new GeoCoordinate(-34.544211999999995, -58.446384999999964), new GeoCoordinate(-34.549495, -58.43663300000003), new GeoCoordinate(-34.549765, -58.436301000000014), new GeoCoordinate(-34.550020999999994, -58.43602699999997), new GeoCoordinate(-34.550233999999996, -58.43588699999998), new GeoCoordinate(-34.550397999999994, -58.43586099999999), new GeoCoordinate(-34.550546999999995, -58.43587200000002), new GeoCoordinate(-34.550653999999994, -58.435963000000015), new GeoCoordinate(-34.550746999999994, -58.43604800000003), new GeoCoordinate(-34.550838999999996, -58.43628899999999), new GeoCoordinate(-34.550864999999995, -58.43642999999997), new GeoCoordinate(-34.550857, -58.436590000000024), new GeoCoordinate(-34.550678999999995, -58.436869), new GeoCoordinate(-34.550283, -58.43714799999998), new GeoCoordinate(-34.549399, -58.43726600000002), new GeoCoordinate(-34.548431, -58.437314000000015), new GeoCoordinate(-34.548068, -58.43741699999998), new GeoCoordinate(-34.547776999999996, -58.43759799999998), new GeoCoordinate(-34.547286, -58.437877000000015), new GeoCoordinate(-34.546597999999996, -58.438289999999995), new GeoCoordinate(-34.545931, -58.438575000000014), new GeoCoordinate(-34.545462, -58.43856900000003), new GeoCoordinate(-34.5454, -58.43855400000001), new GeoCoordinate(-34.545324, -58.43858599999999), new GeoCoordinate(-34.545276, -58.438612000000035), new GeoCoordinate(-34.545245, -58.438665000000015), new GeoCoordinate(-34.545228, -58.43873500000001), new GeoCoordinate(-34.545223, -58.438805), new GeoCoordinate(-34.545284, -58.438950999999975), new GeoCoordinate(-34.545391, -58.43920200000002), new GeoCoordinate(-34.545405, -58.44091300000002), new GeoCoordinate(-34.545333, -58.44104800000002), new GeoCoordinate(-34.545183, -58.44111299999997), new GeoCoordinate(-34.544661999999995, -58.441074000000015), new GeoCoordinate(-34.543486, -58.44097799999997), new GeoCoordinate(-34.543314, -58.440935999999965), new GeoCoordinate(-34.543182, -58.440961000000016), new GeoCoordinate(-34.543134, -58.44099499999999), new GeoCoordinate(-34.543103, -58.441058999999996), new GeoCoordinate(-34.543106, -58.44123999999999), new GeoCoordinate(-34.543134, -58.441422999999986), new GeoCoordinate(-34.543115, -58.441755), new GeoCoordinate(-34.542842, -58.44263000000001), new GeoCoordinate(-34.542571, -58.44362899999999), new GeoCoordinate(-34.542197, -58.444894999999974) } });
			
			//_routesCache.Add(4, new Route(42, "B", RouteDirection.Going) { Stops = { new GeoCoordinate(-34.651797, -58.40756399999998), new GeoCoordinate(-34.653791999999996, -58.410217999999986), new GeoCoordinate(-34.654144, -58.41130199999998), new GeoCoordinate(-34.654789, -58.413017999999965), new GeoCoordinate(-34.655442, -58.414176999999995), new GeoCoordinate(-34.656686, -58.41644199999996), new GeoCoordinate(-34.655794, -58.416450999999995), new GeoCoordinate(-34.654198, -58.41634399999998), new GeoCoordinate(-34.652546, -58.41636699999998), new GeoCoordinate(-34.651831, -58.416349999999966), new GeoCoordinate(-34.651792, -58.41524400000003), new GeoCoordinate(-34.651637, -58.414484000000016), new GeoCoordinate(-34.651713, -58.414413999999965), new GeoCoordinate(-34.651788, -58.41444000000001), new GeoCoordinate(-34.651862, -58.41477900000001), new GeoCoordinate(-34.651986, -58.41514799999999), new GeoCoordinate(-34.65218, -58.416135999999995), new GeoCoordinate(-34.65196, -58.41658100000001), new GeoCoordinate(-34.651814, -58.41773999999998), new GeoCoordinate(-34.65162, -58.41920900000002), new GeoCoordinate(-34.65144, -58.42081400000001), new GeoCoordinate(-34.651099, -58.423790999999994), new GeoCoordinate(-34.647891, -58.428248999999994), new GeoCoordinate(-34.645147, -58.43207899999999), new GeoCoordinate(-34.644171, -58.43216899999999), new GeoCoordinate(-34.641712999999996, -58.432293000000016), new GeoCoordinate(-34.640627, -58.43239399999999), new GeoCoordinate(-34.638235, -58.43257700000004), new GeoCoordinate(-34.63624, -58.43363899999997), new GeoCoordinate(-34.635489, -58.434347), new GeoCoordinate(-34.635369999999995, -58.43437499999999), new GeoCoordinate(-34.635131, -58.43409099999997), new GeoCoordinate(-34.634872, -58.43255499999998), new GeoCoordinate(-34.632461, -58.43286699999999), new GeoCoordinate(-34.631433, -58.433021999999994), new GeoCoordinate(-34.629763999999994, -58.433381999999995), new GeoCoordinate(-34.627977, -58.433772999999974), new GeoCoordinate(-34.626767, -58.43407400000001), new GeoCoordinate(-34.625827, -58.43428799999998), new GeoCoordinate(-34.623000999999995, -58.43462599999998), new GeoCoordinate(-34.621245, -58.435130000000015), new GeoCoordinate(-34.620407, -58.435490000000016), new GeoCoordinate(-34.619306, -58.43589800000001), new GeoCoordinate(-34.618234, -58.43646100000001), new GeoCoordinate(-34.616557, -58.43733599999996), new GeoCoordinate(-34.615929, -58.43758200000002), new GeoCoordinate(-34.614031999999995, -58.43780700000002), new GeoCoordinate(-34.608839999999994, -58.440076999999974), new GeoCoordinate(-34.606495, -58.440147000000024), new GeoCoordinate(-34.605884999999994, -58.440157), new GeoCoordinate(-34.605805999999994, -58.440382), new GeoCoordinate(-34.605174999999996, -58.44100400000002), new GeoCoordinate(-34.604317, -58.44183599999997), new GeoCoordinate(-34.60279499999999, -58.443354), new GeoCoordinate(-34.60139, -58.444717999999966), new GeoCoordinate(-34.597791, -58.448751000000016), new GeoCoordinate(-34.596497, -58.450166999999965), new GeoCoordinate(-34.595186999999996, -58.45163100000002), new GeoCoordinate(-34.594347, -58.45058), new GeoCoordinate(-34.592611999999995, -58.448639000000014), new GeoCoordinate(-34.589639999999996, -58.45181500000001), new GeoCoordinate(-34.588827, -58.450999000000024), new GeoCoordinate(-34.588717, -58.450976999999966), new GeoCoordinate(-34.588607, -58.451031), new GeoCoordinate(-34.588478, -58.451153999999974), new GeoCoordinate(-34.588307, -58.45146999999997), new GeoCoordinate(-34.586954999999996, -58.455074999999965), new GeoCoordinate(-34.585143, -58.45395500000001), new GeoCoordinate(-34.582442, -58.45222699999999), new GeoCoordinate(-34.580449, -58.45095500000002), new GeoCoordinate(-34.578034, -58.44948599999998), new GeoCoordinate(-34.577061, -58.448938999999996), new GeoCoordinate(-34.574813999999996, -58.447721), new GeoCoordinate(-34.573528, -58.44705099999999), new GeoCoordinate(-34.571584, -58.44583799999998), new GeoCoordinate(-34.569839, -58.444818999999995), new GeoCoordinate(-34.565342, -58.440372000000025), new GeoCoordinate(-34.564154, -58.439278), new GeoCoordinate(-34.563032, -58.43830700000001), new GeoCoordinate(-34.562891, -58.438289999999995), new GeoCoordinate(-34.562819999999995, -58.43834900000002), new GeoCoordinate(-34.561883, -58.442511999999965), new GeoCoordinate(-34.56172, -58.44330000000002), new GeoCoordinate(-34.56149, -58.44410099999999), new GeoCoordinate(-34.561354, -58.44457699999998), new GeoCoordinate(-34.560986, -58.445447), new GeoCoordinate(-34.560381, -58.446551), new GeoCoordinate(-34.560223, -58.44646), new GeoCoordinate(-34.558915999999996, -58.44421799999998), new GeoCoordinate(-34.556900999999996, -58.440871000000016), new GeoCoordinate(-34.553687999999994, -58.43545900000004), new GeoCoordinate(-34.553613999999996, -58.435045), new GeoCoordinate(-34.553560999999995, -58.43480899999997), new GeoCoordinate(-34.553405999999995, -58.43467399999997), new GeoCoordinate(-34.5533, -58.434649000000036), new GeoCoordinate(-34.553207, -58.43465900000001), new GeoCoordinate(-34.553109, -58.43471699999998), new GeoCoordinate(-34.553061, -58.434842), new GeoCoordinate(-34.553131, -58.435227999999995), new GeoCoordinate(-34.553061, -58.435464000000024), new GeoCoordinate(-34.55288, -58.43566699999997), new GeoCoordinate(-34.551599, -58.43667500000004), new GeoCoordinate(-34.550909999999995, -58.436954000000014), new GeoCoordinate(-34.550283, -58.43714799999998), new GeoCoordinate(-34.549399, -58.43726600000002), new GeoCoordinate(-34.548431, -58.437314000000015), new GeoCoordinate(-34.548068, -58.43741699999998), new GeoCoordinate(-34.54774999999999, -58.43758700000001), new GeoCoordinate(-34.547286, -58.437877000000015), new GeoCoordinate(-34.546597999999996, -58.438289999999995), new GeoCoordinate(-34.545931, -58.438575000000014), new GeoCoordinate(-34.545462, -58.43856900000003), new GeoCoordinate(-34.5454, -58.43855400000001), new GeoCoordinate(-34.545324, -58.43858599999999), new GeoCoordinate(-34.545276, -58.438612000000035), new GeoCoordinate(-34.545245, -58.438665000000015), new GeoCoordinate(-34.545228, -58.43873500000001), new GeoCoordinate(-34.545223, -58.438805), new GeoCoordinate(-34.545284, -58.438950999999975), new GeoCoordinate(-34.545391, -58.43920200000002), new GeoCoordinate(-34.545405, -58.44091300000002), new GeoCoordinate(-34.545333, -58.44104800000002), new GeoCoordinate(-34.545183, -58.44111299999997), new GeoCoordinate(-34.544661999999995, -58.441074000000015), new GeoCoordinate(-34.543486, -58.44097799999997), new GeoCoordinate(-34.543314, -58.440935999999965), new GeoCoordinate(-34.543182, -58.440961000000016), new GeoCoordinate(-34.543134, -58.44099499999999), new GeoCoordinate(-34.543103, -58.441058999999996), new GeoCoordinate(-34.543106, -58.44123999999999), new GeoCoordinate(-34.543134, -58.441422999999986), new GeoCoordinate(-34.543115, -58.441755), new GeoCoordinate(-34.542842, -58.44263000000001), new GeoCoordinate(-34.542571, -58.44362899999999), new GeoCoordinate(-34.542197, -58.444894999999974) }});
			
			//#endregion Line 42

			// 55 - A Barrancas de Belgrano
			//_routesCache.Add(5, new Route(5) { LineNumber = 55, BranchCode = "A", RouteDirection = RouteDirection.Return, Stops = { new GeoCoordinate(-34.667805, -58.60219000000001), new GeoCoordinate(-34.666515999999994, -58.600881000000015), new GeoCoordinate(-34.668599, -58.59779100000003), new GeoCoordinate(-34.672905, -58.592534), new GeoCoordinate(-34.679541, -58.58460500000001), new GeoCoordinate(-34.680687999999996, -58.58020699999997), new GeoCoordinate(-34.682628, -58.571515999999974), new GeoCoordinate(-34.682223, -58.571302), new GeoCoordinate(-34.681445999999994, -58.570379), new GeoCoordinate(-34.679681, -58.56859800000001), new GeoCoordinate(-34.681357999999996, -58.566366000000016), new GeoCoordinate(-34.676963, -58.561238), new GeoCoordinate(-34.677669, -58.56020799999999), new GeoCoordinate(-34.682187, -58.554156000000035), new GeoCoordinate(-34.679735, -58.550572999999986), new GeoCoordinate(-34.67414, -58.542591000000016), new GeoCoordinate(-34.669005999999996, -58.53454499999998), new GeoCoordinate(-34.6674, -58.531132000000014), new GeoCoordinate(-34.66376399999999, -58.52205500000002), new GeoCoordinate(-34.662458, -58.518343000000016), new GeoCoordinate(-34.660798, -58.515638999999965), new GeoCoordinate(-34.661786, -58.514395000000036), new GeoCoordinate(-34.664248, -58.511032), new GeoCoordinate(-34.663560999999994, -58.51016200000004), new GeoCoordinate(-34.657214999999994, -58.502534999999966), new GeoCoordinate(-34.656528, -58.50170800000001), new GeoCoordinate(-34.657233999999995, -58.50089400000002), new GeoCoordinate(-34.658115, -58.49985300000003), new GeoCoordinate(-34.65706599999999, -58.498469), new GeoCoordinate(-34.654074, -58.494928000000016), new GeoCoordinate(-34.650383999999995, -58.49052900000004), new GeoCoordinate(-34.646906, -58.486463000000015), new GeoCoordinate(-34.644647, -58.483802999999966), new GeoCoordinate(-34.641248999999995, -58.47992999999997), new GeoCoordinate(-34.638821, -58.476045), new GeoCoordinate(-34.637304, -58.47358800000001), new GeoCoordinate(-34.636271, -58.47018800000001), new GeoCoordinate(-34.635264, -58.47069099999999), new GeoCoordinate(-34.631407, -58.47238600000003), new GeoCoordinate(-34.630532, -58.469244), new GeoCoordinate(-34.629235, -58.46419000000003), new GeoCoordinate(-34.628025, -58.459941000000015), new GeoCoordinate(-34.626692999999996, -58.45685100000003), new GeoCoordinate(-34.625323, -58.45327900000001), new GeoCoordinate(-34.623636999999995, -58.448891), new GeoCoordinate(-34.621907, -58.44455700000003), new GeoCoordinate(-34.621262, -58.44273199999998), new GeoCoordinate(-34.620881999999995, -58.44111299999997), new GeoCoordinate(-34.621898, -58.44095199999998), new GeoCoordinate(-34.621712, -58.43930899999998), new GeoCoordinate(-34.621546, -58.43819400000001), new GeoCoordinate(-34.620988999999994, -58.43700200000001), new GeoCoordinate(-34.621501, -58.43678799999998), new GeoCoordinate(-34.621281, -58.43520000000001), new GeoCoordinate(-34.619621, -58.43584499999997), new GeoCoordinate(-34.617607, -58.43679900000001), new GeoCoordinate(-34.615948, -58.43757199999999), new GeoCoordinate(-34.614056999999995, -58.43779699999999), new GeoCoordinate(-34.612512, -58.438537999999994), new GeoCoordinate(-34.60874999999999, -58.44019000000003), new GeoCoordinate(-34.608777999999994, -58.438954999999964), new GeoCoordinate(-34.60874999999999, -58.43744300000003), new GeoCoordinate(-34.608715999999994, -58.437239999999974), new GeoCoordinate(-34.607939, -58.43764599999997), new GeoCoordinate(-34.606641, -58.43849499999999), new GeoCoordinate(-34.605563999999994, -58.43814099999997), new GeoCoordinate(-34.605121999999994, -58.437828999999965), new GeoCoordinate(-34.604785, -58.43815000000001), new GeoCoordinate(-34.604123, -58.43881699999997), new GeoCoordinate(-34.603444, -58.439351999999985), new GeoCoordinate(-34.60286, -58.44077900000002), new GeoCoordinate(-34.602207, -58.44236799999999), new GeoCoordinate(-34.601925, -58.44246399999997), new GeoCoordinate(-34.598852, -58.445726000000036), new GeoCoordinate(-34.598137, -58.444749), new GeoCoordinate(-34.597174, -58.44372999999996), new GeoCoordinate(-34.59635299999999, -58.442678), new GeoCoordinate(-34.595275, -58.440961000000016), new GeoCoordinate(-34.594003, -58.43867599999999), new GeoCoordinate(-34.591476, -58.43447100000003), new GeoCoordinate(-34.589658, -58.431542000000036), new GeoCoordinate(-34.588943, -58.430575999999974), new GeoCoordinate(-34.588915, -58.43016799999998), new GeoCoordinate(-34.588757, -58.42985799999997), new GeoCoordinate(-34.588492, -58.429824999999994), new GeoCoordinate(-34.588067, -58.42920300000003), new GeoCoordinate(-34.58623, -58.426467), new GeoCoordinate(-34.585551, -58.42521199999999), new GeoCoordinate(-34.583793, -58.42297500000001), new GeoCoordinate(-34.58238, -58.42482100000001), new GeoCoordinate(-34.580329, -58.42228799999998), new GeoCoordinate(-34.579402, -58.423721), new GeoCoordinate(-34.578785, -58.424954000000014), new GeoCoordinate(-34.578271, -58.425940999999966), new GeoCoordinate(-34.578015, -58.427089000000024), new GeoCoordinate(-34.577687999999995, -58.42807700000003), new GeoCoordinate(-34.577450999999996, -58.42848500000002), new GeoCoordinate(-34.576743, -58.429096000000015), new GeoCoordinate(-34.57533, -58.43064100000004), new GeoCoordinate(-34.574445999999995, -58.43164899999999), new GeoCoordinate(-34.571760999999995, -58.43418099999997), new GeoCoordinate(-34.568652, -58.437239999999974), new GeoCoordinate(-34.56752, -58.43792500000001), new GeoCoordinate(-34.565894, -58.43969500000003), new GeoCoordinate(-34.563606, -58.442646999999965), new GeoCoordinate(-34.562042999999996, -58.444619999999986), new GeoCoordinate(-34.561813, -58.44416899999999), new GeoCoordinate(-34.561531, -58.44414900000004), new GeoCoordinate(-34.560842, -58.44559600000002), new GeoCoordinate(-34.560425, -58.44643400000001), new GeoCoordinate(-34.560302, -58.44695899999999), new GeoCoordinate(-34.559833999999995, -58.44813999999997), new GeoCoordinate(-34.559065999999994, -58.449479999999994) } });

			// 105 - A Correo Central por Golf
			//_routesCache.Add(22, new Route(22){ LineNumber = 105, BranchCode = "B", RouteDirection = RouteDirection.Going, Stops = { new GeoCoordinate(-34.612116, -58.54392200000001), new GeoCoordinate(-34.61094, -58.54206499999998), new GeoCoordinate(-34.609051, -58.53908200000001), new GeoCoordinate(-34.607258, -58.53637800000001), new GeoCoordinate(-34.606516, -58.535262999999986), new GeoCoordinate(-34.606375, -58.53496200000001), new GeoCoordinate(-34.607073, -58.53285900000003), new GeoCoordinate(-34.607841, -58.530531999999994), new GeoCoordinate(-34.606959, -58.53013499999997), new GeoCoordinate(-34.604857, -58.529157999999995), new GeoCoordinate(-34.603886, -58.528719000000024), new GeoCoordinate(-34.603524, -58.529791999999986), new GeoCoordinate(-34.603144, -58.529448), new GeoCoordinate(-34.602463, -58.52845000000002), new GeoCoordinate(-34.601712, -58.527281000000016), new GeoCoordinate(-34.600963, -58.52601500000003), new GeoCoordinate(-34.602003999999994, -58.525081), new GeoCoordinate(-34.602709999999995, -58.525413000000015), new GeoCoordinate(-34.602790999999996, -58.52535999999998), new GeoCoordinate(-34.602781, -58.525210000000015), new GeoCoordinate(-34.602878999999994, -58.524512000000016), new GeoCoordinate(-34.602242999999994, -58.52419099999997), new GeoCoordinate(-34.601784, -58.523428999999965), new GeoCoordinate(-34.601351, -58.522741999999994), new GeoCoordinate(-34.600158, -58.52080000000001), new GeoCoordinate(-34.598604, -58.51828999999998), new GeoCoordinate(-34.599311, -58.51761399999998), new GeoCoordinate(-34.600043, -58.516970000000015), new GeoCoordinate(-34.601076, -58.51602700000001), new GeoCoordinate(-34.602180999999995, -58.514996999999994), new GeoCoordinate(-34.600529, -58.512324000000035), new GeoCoordinate(-34.600062, -58.51158399999997), new GeoCoordinate(-34.599584, -58.51074699999998), new GeoCoordinate(-34.598745, -58.509438999999986), new GeoCoordinate(-34.597138, -58.506843), new GeoCoordinate(-34.594692, -58.50291500000003), new GeoCoordinate(-34.595407, -58.500985000000014), new GeoCoordinate(-34.595964, -58.49954600000001), new GeoCoordinate(-34.596819999999994, -58.49715400000002), new GeoCoordinate(-34.59694399999999, -58.49475100000001), new GeoCoordinate(-34.597095, -58.49237900000003), new GeoCoordinate(-34.597174, -58.490973999999994), new GeoCoordinate(-34.59727, -58.48995500000001), new GeoCoordinate(-34.597624, -58.484774000000016), new GeoCoordinate(-34.597782, -58.48305700000003), new GeoCoordinate(-34.598171, -58.48229500000002), new GeoCoordinate(-34.598515, -58.481555000000014), new GeoCoordinate(-34.599222999999995, -58.47989100000001), new GeoCoordinate(-34.599663, -58.478830000000016), new GeoCoordinate(-34.6, -58.477305), new GeoCoordinate(-34.600282, -58.47590100000002), new GeoCoordinate(-34.601112, -58.47219899999999), new GeoCoordinate(-34.601933, -58.46862599999997), new GeoCoordinate(-34.602383999999994, -58.46674899999999), new GeoCoordinate(-34.602826, -58.46499), new GeoCoordinate(-34.60363, -58.46182399999998), new GeoCoordinate(-34.6043, -58.45923800000003), new GeoCoordinate(-34.604759, -58.457532000000015), new GeoCoordinate(-34.605191999999995, -58.455708000000016), new GeoCoordinate(-34.605978, -58.452845000000025), new GeoCoordinate(-34.606782, -58.44960400000002), new GeoCoordinate(-34.607399, -58.447028999999986), new GeoCoordinate(-34.607594999999996, -58.44633099999999), new GeoCoordinate(-34.607683, -58.446105999999986), new GeoCoordinate(-34.607753, -58.445419000000015), new GeoCoordinate(-34.607912999999996, -58.44450699999999), new GeoCoordinate(-34.608362, -58.441652999999974), new GeoCoordinate(-34.60879499999999, -58.44145100000003), new GeoCoordinate(-34.60879499999999, -58.43983000000003), new GeoCoordinate(-34.608777999999994, -58.437298999999996), new GeoCoordinate(-34.608777999999994, -58.43472399999996), new GeoCoordinate(-34.608723999999995, -58.434529999999995), new GeoCoordinate(-34.608732999999994, -58.43286699999999), new GeoCoordinate(-34.608723999999995, -58.43049500000001), new GeoCoordinate(-34.608715999999994, -58.42769499999997), new GeoCoordinate(-34.608663, -58.42616099999998), new GeoCoordinate(-34.608636999999995, -58.424638000000016), new GeoCoordinate(-34.608644999999996, -58.423059999999964), new GeoCoordinate(-34.608574999999995, -58.42154800000003), new GeoCoordinate(-34.609951, -58.421268999999995), new GeoCoordinate(-34.609766, -58.42066799999998), new GeoCoordinate(-34.60967, -58.41964899999999), new GeoCoordinate(-34.609617, -58.41866300000004), new GeoCoordinate(-34.609599, -58.417417), new GeoCoordinate(-34.609448, -58.41595899999999), new GeoCoordinate(-34.609377, -58.41459600000002), new GeoCoordinate(-34.609271, -58.413179000000014), new GeoCoordinate(-34.609219, -58.411816999999985), new GeoCoordinate(-34.609158, -58.41039000000001), new GeoCoordinate(-34.609121, -58.408952), new GeoCoordinate(-34.608883999999996, -58.40606700000001), new GeoCoordinate(-34.608732999999994, -58.40470400000004), new GeoCoordinate(-34.608601, -58.40194600000001), new GeoCoordinate(-34.608546999999994, -58.40057300000001), new GeoCoordinate(-34.608371999999996, -58.39775099999997), new GeoCoordinate(-34.608283, -58.396379000000024), new GeoCoordinate(-34.608141999999994, -58.39494100000002), new GeoCoordinate(-34.608096999999994, -58.39350300000001), new GeoCoordinate(-34.607973, -58.392065), new GeoCoordinate(-34.60785, -58.390692), new GeoCoordinate(-34.607779, -58.38885700000003), new GeoCoordinate(-34.607594999999996, -58.38635799999997), new GeoCoordinate(-34.607489, -58.38496199999997), new GeoCoordinate(-34.607408, -58.383557999999994), new GeoCoordinate(-34.60732, -58.382001), new GeoCoordinate(-34.607241, -58.38070400000004), new GeoCoordinate(-34.607081, -58.37871799999999), new GeoCoordinate(-34.60687, -58.375714000000016), new GeoCoordinate(-34.607382, -58.37472700000001), new GeoCoordinate(-34.60786, -58.373740999999995), new GeoCoordinate(-34.60801, -58.373406999999986), new GeoCoordinate(-34.608106, -58.37346200000002), new GeoCoordinate(-34.608211999999995, -58.373493999999994), new GeoCoordinate(-34.608424, -58.37347199999999), new GeoCoordinate(-34.608892999999995, -58.373354000000006), new GeoCoordinate(-34.608971999999994, -58.373193000000015), new GeoCoordinate(-34.608945999999996, -58.37262399999997), new GeoCoordinate(-34.608902, -58.372097999999994), new GeoCoordinate(-34.608821999999996, -58.37077999999997), new GeoCoordinate(-34.60879499999999, -58.37024299999996), new GeoCoordinate(-34.60879499999999, -58.36972800000001), new GeoCoordinate(-34.608802999999995, -58.369609999999966), new GeoCoordinate(-34.608864999999994, -58.369523000000015), new GeoCoordinate(-34.609281, -58.36925600000001), new GeoCoordinate(-34.609149, -58.369042000000036), new GeoCoordinate(-34.609042, -58.368836999999985), new GeoCoordinate(-34.608857, -58.36838599999999), new GeoCoordinate(-34.60868, -58.368138999999985), new GeoCoordinate(-34.608574999999995, -58.36801200000002), new GeoCoordinate(-34.608467999999995, -58.367925000000014), new GeoCoordinate(-34.608194999999995, -58.36773299999999), new GeoCoordinate(-34.60777, -58.367796999999996), new GeoCoordinate(-34.607549999999996, -58.367818), new GeoCoordinate(-34.60732, -58.36790400000001), new GeoCoordinate(-34.607188, -58.36803199999997), new GeoCoordinate(-34.607056, -58.36819400000002), new GeoCoordinate(-34.606985, -58.368439999999964), new GeoCoordinate(-34.606959, -58.36874), new GeoCoordinate(-34.606878, -58.369042000000036), new GeoCoordinate(-34.606765, -58.36940500000003), new GeoCoordinate(-34.60665, -58.36958800000002), new GeoCoordinate(-34.606605, -58.369641), new GeoCoordinate(-34.606507, -58.36969499999998), new GeoCoordinate(-34.60627, -58.36968400000001), new GeoCoordinate(-34.606005, -58.36975000000001), new GeoCoordinate(-34.60532499999999, -58.369823999999994), new GeoCoordinate(-34.602993999999995, -58.37007199999999), new GeoCoordinate(-34.602826, -58.368322000000035), new GeoCoordinate(-34.603135, -58.36827900000003), new GeoCoordinate(-34.603462, -58.36833300000001), new GeoCoordinate(-34.603709, -58.36841900000002), new GeoCoordinate(-34.604053, -58.36850400000003) } });
		}

		#endregion Private Methods

		public override IList<Route> GetAll()
		{
			return _routesCache.Values.ToList();
		}
	}
}
