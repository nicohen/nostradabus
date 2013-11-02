using System;
using System.Collections.Generic;
using System.Device.Location;

namespace Nostradabus.Common
{
	public class GeoHelper
	{
		// Semi-axes of WGS-84 geoidal reference
		private const double WGS84_a = 6378137.0; // Major semiaxis [m]
		private const double WGS84_b = 6356752.3; // Minor semiaxis [m]

		private const Double MilesToKilometers = 1.609344;
		private const Double MilesToNautical = 0.8684;

		/// <summary>
		/// Class is used in a calculation to determin cardinal point enumeration values from degrees.
		/// </summary>
		private struct CardinalRanges
		{
			public CardinalPoints CardinalPoint;
			/// <summary>
			/// Low range value associated with the cardinal point.
			/// </summary>
			public Double LowRange;
			/// <summary>
			/// High range value associated with the cardinal point.
			/// </summary>
			public Double HighRange;
		}

		#region Methods
		
		/// <summary>
		/// Calculates the distance (in meters) between two points of latitude and longitude.
		/// </summary>
		/// <param name="coordinate1">First coordinate.</param>
		/// <param name="coordinate2">Second coordinate.</param>
		public static Double Distance(GeoCoordinate coordinate1, GeoCoordinate coordinate2)
		{
			return coordinate1.GetDistanceTo(coordinate2);
		}

		/// <summary>
		/// Calculates a bounding box centered on the given point and within a given distance (halfSide) in meters.
		/// </summary>
		/// <returns>Returns the bounding box.</returns>
		public static BoundingBox GetBoundingBox(GeoCoordinate point, double halfSide)
		{
			// Bounding box surrounding the point at given coordinates,
			// assuming local approximation of Earth surface as a sphere
			// of radius given by WGS84
			var lat = ToRadian(point.Latitude);
			var lon = ToRadian(point.Longitude);
			
			// Radius of Earth at given latitude
			var radius = WGS84EarthRadius(lat);
			// Radius of the parallel at given latitude
			var pradius = radius * Math.Cos(lat);

			var latMin = lat - halfSide / radius;
			var latMax = lat + halfSide / radius;
			var lonMin = lon - halfSide / pradius;
			var lonMax = lon + halfSide / pradius;

			return new BoundingBox
			{
				MinPoint = new GeoCoordinate { Latitude = ToDegree(latMin), Longitude = ToDegree(lonMin) },
				MaxPoint = new GeoCoordinate { Latitude = ToDegree(latMax), Longitude = ToDegree(lonMax) }
			};
		}

		/// <summary>
		/// Converts degrees to Radians.
		/// </summary>
		/// <returns>Returns a radian from degrees.</returns>
		public static Double ToRadian(Double degree) { return (degree * Math.PI / 180.0); }
		
		/// <summary>
		/// To degress from a radian value.
		/// </summary>
		/// <returns>Returns degrees from radians.</returns>
		public static Double ToDegree(Double radian) { return (radian / Math.PI * 180.0); }
		
		/// <summary>
		/// Calculates the distance between two points of latitude and longitude.
		/// Great Link - http://www.movable-type.co.uk/scripts/latlong.html
		/// </summary>
		/// <param name="coordinate1">First coordinate.</param>
		/// <param name="coordinate2">Second coordinate.</param>
		/// <param name="unitsOfLength">Sets the return value unit of length.</param>
		public static Double Distance2(GeoCoordinate coordinate1, GeoCoordinate coordinate2, UnitsOfLength unitsOfLength)
		{

			var theta = coordinate1.Longitude - coordinate2.Longitude;
			var distance = Math.Sin(ToRadian(coordinate1.Latitude)) * Math.Sin(ToRadian(coordinate2.Latitude)) +
						   Math.Cos(ToRadian(coordinate1.Latitude)) * Math.Cos(ToRadian(coordinate2.Latitude)) *
						   Math.Cos(ToRadian(theta));

			distance = Math.Acos(distance);
			distance = ToDegree(distance);
			distance = distance * 60 * 1.1515;

			if (unitsOfLength == UnitsOfLength.Kilometer)
				distance = distance * MilesToKilometers;
			else if (unitsOfLength == UnitsOfLength.NauticalMiles)
				distance = distance * MilesToNautical;

			return (distance);

		}
		
		// The directional names are also routinely and very conveniently associated with 
		// the degrees of rotation in the unit circle, a necessary step for navigational 
		// calculations (derived from trigonometry) and/or for use with Global 
		// Positioning Satellite (GPS) Receivers. The four cardinal directions 
		// correspond to the following degrees of a compass:
		//
		// North (N): 0° = 360° 
		// East (E): 90° 
		// South (S): 180° 
		// West (W): 270° 
		// An ordinal, or intercardinal, direction is one of the four intermediate 
		// compass directions located halfway between the cardinal directions.
		//
		// Northeast (NE), 45°, halfway between north and east, is the opposite of southwest. 
		// Southeast (SE), 135°, halfway between south and east, is the opposite of northwest. 
		// Southwest (SW), 225°, halfway between south and west, is the opposite of northeast. 
		// Northwest (NW), 315°, halfway between north and west, is the opposite of southeast. 
		// These 8 words have been further compounded, resulting in a total of 32 named 
		// (and numbered) points evenly spaced around the compass. It is noteworthy that 
		// there are languages which do not use compound words to name the points, 
		// instead assigning unique words, colors, and/or associations with phenomena of the natural world.

		/// <summary>
		/// Method extension for Doubles. Converts a degree to a cardinal point enumeration.
		/// </summary>
		/// <returns>Returns a cardinal point enumeration representing a compass direction.</returns>
		public static CardinalPoints ToCardinalMark(Double degree)
		{
			var cardinalRanges = new List<CardinalRanges>
                       {
                         new CardinalRanges {CardinalPoint = CardinalPoints.N, LowRange = 0, HighRange = 22.5},
                         new CardinalRanges {CardinalPoint = CardinalPoints.NE, LowRange = 22.5, HighRange = 67.5},
                         new CardinalRanges {CardinalPoint = CardinalPoints.E, LowRange = 67.5, HighRange = 112.5},
                         new CardinalRanges {CardinalPoint = CardinalPoints.SE, LowRange = 112.5, HighRange = 157.5},
                         new CardinalRanges {CardinalPoint = CardinalPoints.S, LowRange = 157.5, HighRange = 202.5},
                         new CardinalRanges {CardinalPoint = CardinalPoints.SW, LowRange = 202.5, HighRange = 247.5},
                         new CardinalRanges {CardinalPoint = CardinalPoints.W, LowRange = 247.5, HighRange = 292.5},
                         new CardinalRanges {CardinalPoint = CardinalPoints.NW, LowRange = 292.5, HighRange = 337.5},
                         new CardinalRanges {CardinalPoint = CardinalPoints.N, LowRange = 337.5, HighRange = 360.1}
                       };


			if (!(degree >= 0 && degree <= 360))
				throw new ArgumentOutOfRangeException("degree", "Degree value must be greater than or equal to 0 and less than or equal to 360.");


			return cardinalRanges.Find(p => (degree >= p.LowRange && degree < p.HighRange)).CardinalPoint;
		}

		/// <summary>
		/// Accepts two coordinates in degrees.
		/// </summary>
		/// <returns>A double value in degrees.  From 0 to 360.</returns>
		public static Double Bearing(GeoCoordinate coordinate1, GeoCoordinate coordinate2)
		{
			var latitude1 = ToRadian(coordinate1.Latitude);
			var latitude2 = ToRadian(coordinate2.Latitude);

			var longitudeDifference = ToRadian(coordinate2.Longitude - coordinate1.Longitude);

			var y = Math.Sin(longitudeDifference) * Math.Cos(latitude2);
			var x = Math.Cos(latitude1) * Math.Sin(latitude2) -
					Math.Sin(latitude1) * Math.Cos(latitude2) * Math.Cos(longitudeDifference);

			return (ToDegree(Math.Atan2(y, x)) + 360) % 360;
		}
		
		#endregion Methods

		#region Private Methods

		// Earth radius at a given latitude, according to the WGS-84 ellipsoid [m]
		private static double WGS84EarthRadius(double lat)
		{
			// http://en.wikipedia.org/wiki/Earth_radius
			var An = WGS84_a * WGS84_a * Math.Cos(lat);
			var Bn = WGS84_b * WGS84_b * Math.Sin(lat);
			var Ad = WGS84_a * Math.Cos(lat);
			var Bd = WGS84_b * Math.Sin(lat);
			return Math.Sqrt((An * An + Bn * Bn) / (Ad * Ad + Bd * Bd));
		}

		#endregion Private Methods
	}

	public enum UnitsOfLength { Mile, NauticalMiles, Kilometer }

	// ReSharper disable InconsistentNaming
	public enum CardinalPoints { N, E, W, S, NE, NW, SE, SW }
	// ReSharper restore InconsistentNaming

	public class BoundingBox
	{
		public GeoCoordinate MinPoint { get; set; }
		public GeoCoordinate MaxPoint { get; set; }
	}        
}
