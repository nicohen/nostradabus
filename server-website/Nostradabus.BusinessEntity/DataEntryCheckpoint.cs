using System;
using System.Device.Location;
using Nostradabus.BusinessEntities.Common;

namespace Nostradabus.BusinessEntities
{
	public class DataEntryCheckpoint
	{
		public DataEntryCheckpoint() {}

		public DataEntryCheckpoint(int id)
		{
			ID = id;
		}

		public int ID { get; set; }

		public virtual string SerialNumber { get; set; }

		public virtual int LineNumber { get; set; }
		
		protected virtual double Latitude { get; set; }
		
		protected virtual double Longitude { get; set; }

		public virtual DateTime UserDateTime { get; set; }

		private GeoCoordinate _coordinate;
		public virtual GeoCoordinate Coordinate
		{
			get
			{
				return _coordinate ?? (_coordinate = new GeoCoordinate(Latitude, Longitude));
			}

			set {
				_coordinate = value;
				Latitude = _coordinate.Latitude;
				Longitude = _coordinate.Longitude;
			}
		}
		
		public virtual DateTime DateTime { get; set; }
	}
}
