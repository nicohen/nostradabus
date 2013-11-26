using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Diagnostics;
using Nostradabus.BusinessEntities.Common;
using Nostradabus.Common;

namespace Nostradabus.BusinessEntities
{
	public class Route : BusinessEntity<int>
	{
		private double[] _nextStopDistances;

		public Route() : base()
		{
		}

		public Route(int id) : base(id)
		{
			Stops = new List<GeoCoordinate>();
		}

		public Route(int lineNumber, string branchCode, RouteDirection direction)
		{
			LineNumber = lineNumber;
			BranchCode = BranchCode;
			RouteDirection = direction;
			Stops = new List<GeoCoordinate>();
		}

		#region Properties

		public virtual int LineNumber { get; set; }

		public virtual string BranchCode { get; set; }

		public virtual RouteDirection RouteDirection { get; set; }

		public virtual string Name { get; set; }

		public virtual string Description { get; set; }
		
		public virtual List<GeoCoordinate> Stops { get; set; }

		#endregion Properties
		
		#region Methods

		public virtual double DistanceBetweenStops(int fromStop, int toStop)
		{
			#region Validations

			if (fromStop < 0 || fromStop >= Stops.Count) throw new Exception("Invalid source Stop, must be in the route");
			if (toStop < 0 || toStop >= Stops.Count) throw new Exception("Invalid destination Stop, must be in the route");
			
			#endregion Validations

			// lazy load for distance list cache
			if (_nextStopDistances == null) LoadDistanceCache();

			Debug.Assert(_nextStopDistances != null, "nextStopDistances != null");

			var firstStop = Math.Min(fromStop, toStop);
			var lastStop = Math.Max(fromStop, toStop);

			double distance = 0;

			for (var i = firstStop; i < lastStop; i++)
			{
				distance += _nextStopDistances[i];
			}

			return distance;
		}

		public virtual double DistanceToNextStop(int fromStop)
		{
			if (fromStop == Stops.Count - 1) throw new Exception("Last stop does not have next stop.");

			return DistanceBetweenStops(fromStop, fromStop + 1);
		}

		public virtual double DistanceToPrevStop(int fromStop)
		{
			if (fromStop == 0) throw new Exception("First stop does not have previous stop.");

			return DistanceBetweenStops(fromStop, fromStop - 1);
		}

		#endregion Methods

		#region Overrides

		public override string ToString()
		{
			return String.Format("{0} - {1}: {2}", LineNumber, BranchCode, Description);
		}

		#endregion Overrides

		#region Private Methods

		/// <summary>
		/// Initialize cache list for the distances to the next stop 
		/// </summary>
		private void LoadDistanceCache()
		{
			_nextStopDistances = new double[Stops.Count-1];

			for (var i = 0; i < _nextStopDistances.Length; i++)
			{
				_nextStopDistances[i] = GeoHelper.Distance(Stops[i], Stops[i + 1]);
			}
		}

		#endregion Private Methods
	}
}
