using System;
using System.Device.Location;
using Nostradabus.BusinessEntities.Common;

namespace Nostradabus.BusinessEntities
{
	public class Checkpoint : BusinessEntity<int>
	{
		public virtual Route Route { get; set; }

		public virtual string UUID { get; set; }

		//public virtual int LineNumber { get; set; }

		//public virtual string BranchCode { get; set; }

		//public virtual RouteDirection RouteDirection { get; set; }

		protected virtual double? Latitude { get; set; }
		protected virtual double? Longitude { get; set; }

		private GeoCoordinate _coordinate;
		public virtual GeoCoordinate Coordinate
		{
			get
			{
				if (_coordinate != null) return _coordinate;

				if(Latitude.HasValue && Longitude.HasValue)
				{
					_coordinate = new GeoCoordinate(Latitude.Value, Longitude.Value);
					return _coordinate;
				}

				return GeoCoordinate.Unknown;
			}

			set {
				_coordinate = value;
				Latitude = _coordinate.Latitude;
				Longitude = _coordinate.Longitude;
			}
		}
		
		public virtual double? Speed { get; set; }

		// just to use OpenGpsTracker source, just for test, REMOVE IT LATER
		public virtual string CoordString { get; set; }

		public virtual DateTime DateTime { get; set; }

		public virtual int NextStopIndex { get; set; }
	}
}
