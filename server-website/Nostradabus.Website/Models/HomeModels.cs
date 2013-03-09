
using System;
using System.Device.Location;
using System.Globalization;

namespace Nostradabus.Website.Models
{
	public class TestsModel
	{
		public string Message { get; set; }

		public int Line { get; set; }

		public string Branch { get; set; }

		public string RouteDirection { get; set; }
		
		#region Distance Between Points
		
		public string DistanceBetweenPoints_Coord1String
		{
			set
			{
				if (string.IsNullOrEmpty(value)) return;
				
				var coordParts = value.Trim().Split(',');

				if(coordParts.Length == 2)
				{
					DistanceBetweenPoints_Coord1Lat = ParseCoordComponent(coordParts[0]);
					DistanceBetweenPoints_Coord1Long = ParseCoordComponent(coordParts[1]);
				}
			}

			get { return DistanceBetweenPoints_Coord1.IsUnknown ? "" : DistanceBetweenPoints_Coord1.ToString(); }
		}

		public double? DistanceBetweenPoints_Coord1Lat { get; set; }
		public double? DistanceBetweenPoints_Coord1Long { get; set; }
		
		public string DistanceBetweenPoints_Coord2String
		{
			set
			{
				if (string.IsNullOrEmpty(value)) return;

				var coordParts = value.Trim().Split(',');

				if (coordParts.Length == 2)
				{
					DistanceBetweenPoints_Coord2Lat = ParseCoordComponent(coordParts[0]);
					DistanceBetweenPoints_Coord2Long = ParseCoordComponent(coordParts[1]);
				}
			}

			get { return DistanceBetweenPoints_Coord2.IsUnknown ? "" : DistanceBetweenPoints_Coord2.ToString(); }
		}

		public double? DistanceBetweenPoints_Coord2Lat { get; set; }
		public double? DistanceBetweenPoints_Coord2Long { get; set; }

		public GeoCoordinate DistanceBetweenPoints_Coord1
		{
			get
			{
				if(DistanceBetweenPoints_Coord1Lat.HasValue && DistanceBetweenPoints_Coord1Long.HasValue)
					return new GeoCoordinate(DistanceBetweenPoints_Coord1Lat.Value, DistanceBetweenPoints_Coord1Long.Value);

				return GeoCoordinate.Unknown;
			}
		}
		public GeoCoordinate DistanceBetweenPoints_Coord2
		{
			get
			{
				if (DistanceBetweenPoints_Coord2Lat.HasValue && DistanceBetweenPoints_Coord2Long.HasValue)
					return new GeoCoordinate(DistanceBetweenPoints_Coord2Lat.Value, DistanceBetweenPoints_Coord2Long.Value);

				return GeoCoordinate.Unknown;
			}
		}

		public double? DistanceBetweenPoints_Result { get; set; }

		#endregion Distance Between Points

		#region Distance Between Point and Stop

		public string DistanceBetweenPointAndStop_Coord1String
		{
			set
			{
				if (string.IsNullOrEmpty(value)) return;

				var coordParts = value.Trim().Split(',');

				if (coordParts.Length == 2)
				{
					DistanceBetweenPointAndStop_Coord1Lat = ParseCoordComponent(coordParts[0]);
					DistanceBetweenPointAndStop_Coord1Long = ParseCoordComponent(coordParts[1]);
				}
			}

			get { return DistanceBetweenPointAndStop_Coord1.IsUnknown ? "" : DistanceBetweenPointAndStop_Coord1.ToString(); }
		}

		public double? DistanceBetweenPointAndStop_Coord1Lat { get; set; }
		public double? DistanceBetweenPointAndStop_Coord1Long { get; set; }

		public int? DistanceBetweenPointAndStop_Stop { get; set; }

		public GeoCoordinate DistanceBetweenPointAndStop_Coord1
		{
			get
			{
				if (DistanceBetweenPointAndStop_Coord1Lat.HasValue && DistanceBetweenPointAndStop_Coord1Long.HasValue)
					return new GeoCoordinate(DistanceBetweenPointAndStop_Coord1Lat.Value, DistanceBetweenPointAndStop_Coord1Long.Value);

				return GeoCoordinate.Unknown;
			}
		}
		
		public double? DistanceBetweenPointAndStop_Result { get; set; }

		#endregion Distance Between Point and Stop

		#region Coordinate Of Stop

		public int? CoordinateOfStop_Stop { get; set; }
		public GeoCoordinate CoordinateOfStop_Result { get; set; }

		#endregion Coordinate Of Stop

		#region Closest Stop From Point

		public string ClosestStop_CoordString
		{
			set
			{
				if (string.IsNullOrEmpty(value)) return;

				var coordParts = value.Trim().Split(',');

				if (coordParts.Length == 2)
				{
					ClosestStop_CoordLat = ParseCoordComponent(coordParts[0]);
					ClosestStop_CoordLong = ParseCoordComponent(coordParts[1]);
				}
			}

			get { return ClosestStop_Coord.IsUnknown ? "" : ClosestStop_Coord.ToString(); }
		}

		public double? ClosestStop_CoordLat { get; set; }
		public double? ClosestStop_CoordLong { get; set; }

		public GeoCoordinate ClosestStop_Coord
		{
			get
			{
				if (ClosestStop_CoordLat.HasValue && ClosestStop_CoordLong.HasValue)
					return new GeoCoordinate(ClosestStop_CoordLat.Value, ClosestStop_CoordLong.Value);

				return GeoCoordinate.Unknown;
			}
		}

		public int? ClosestStop_Result { get; set; }

		#endregion Closest Stop From Point

		#region Next Stop From Point

		public string NextStop_CoordString
		{
			set
			{
				if (string.IsNullOrEmpty(value)) return;

				var coordParts = value.Trim().Split(',');

				if (coordParts.Length == 2)
				{
					NextStop_CoordLat = ParseCoordComponent(coordParts[0]);
					NextStop_CoordLong = ParseCoordComponent(coordParts[1]);
				}
			}

			get { return NextStop_Coord.IsUnknown ? "" : NextStop_Coord.ToString(); }
		}

		public double? NextStop_CoordLat { get; set; }
		public double? NextStop_CoordLong { get; set; }

		public GeoCoordinate NextStop_Coord
		{
			get
			{
				if (NextStop_CoordLat.HasValue && NextStop_CoordLong.HasValue)
					return new GeoCoordinate(NextStop_CoordLat.Value, NextStop_CoordLong.Value);

				return GeoCoordinate.Unknown;
			}
		}

		public int? NextStop_Result { get; set; }

		#endregion Next Stop From Point

		#region Distance Between Stops

		public int? DistanceBetweenStops_Stop1 { get; set; }
		public int? DistanceBetweenStops_Stop2 { get; set; }
		public double? DistanceBetweenStops_Result { get; set; }

		#endregion Distance Between Stops

		private static double? ParseCoordComponent(string componentString)
		{
			double component;

			if (double.TryParse(componentString.Trim().Replace(".", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out component))
				return component;
			
			return null;
		}
	}

	public class ParamTest
	{
		#region Properties

		public int SpeedAverageMaxSamples { get; set; }

		// determines if speed samples have the same or different weight 
		// (true = use different weight, false = uniform)
		public bool UseSpeedWeightCoef { get; set; }

		public bool UseStatistics { get; set; }

		// determines the max distance (in meters) to use only speed extrapolation,
		// when UseStatistics = true
		public int SpeedExtrapolationMaxDistance { get; set; }

		public bool UseAverageTimeOffsetCoef { get; set; }

		public int AverageTimeOffsetMinSamples { get; set; }

		public int SuccessCounter { get; set; }

		public int FailureCounter { get; set; }

		public TimeSpan AverageTimeDifference { get; set; }

		public TimeSpan MaxTimeDifference { get; set; }
		
		#endregion Properties
	}
}