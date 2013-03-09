using System;
using System.Device.Location;
using Nostradabus.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nostradabus.Test
{
	/// <summary>
	/// Summary description for GeoHelperTest
	/// </summary>
	[TestClass]
	public class GeoHelperTest
	{
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion


		[TestMethod]
		public void DistanceSamePoint0()
		{
			var distance = GeoHelper.Distance(new GeoCoordinate { Latitude = 90, Longitude = -90 }, new GeoCoordinate { Latitude = 90, Longitude = -90 });

			Assert.AreEqual(0, distance, "");
		}
		
		[TestMethod]
		public void DistanceSampleDataFirstLastStop71A()
		{
			var distance = GeoHelper.Distance(new GeoCoordinate { Latitude = -34.609068, Longitude = -58.40889800000002 }, new GeoCoordinate { Latitude = -34.525404, Longitude = -58.54359899999997 });

			Assert.AreEqual(15450, distance);
		}

		[TestMethod]
		public void DistanceSampleDataKilometer()
		{
			var distance = GeoHelper.Distance(new GeoCoordinate { Latitude = 45, Longitude = 0 }, new GeoCoordinate { Latitude = 0, Longitude = 45 });

			Assert.AreEqual(6671374.6175999995d, distance, "");
		}
		
		[TestMethod]
		public void BearingsTests()
		{
			var bearing = GeoHelper.Bearing(new GeoCoordinate{ Latitude = 45, Longitude = 1 },new GeoCoordinate{ Latitude = 45, Longitude = 0 });

			Assert.AreEqual(270.35355787806577d, bearing, "");
			Assert.AreEqual(CardinalPoints.W, GeoHelper.ToCardinalMark(bearing));
		}

		[TestMethod]
		public void CardinalMarkValues()
		{
			Assert.AreEqual(CardinalPoints.N, GeoHelper.ToCardinalMark(2D));
			Assert.AreEqual(CardinalPoints.NE, GeoHelper.ToCardinalMark(46D));
			Assert.AreEqual(CardinalPoints.SE, GeoHelper.ToCardinalMark(120D));
			Assert.AreEqual(CardinalPoints.S, GeoHelper.ToCardinalMark(170D));
			Assert.AreEqual(CardinalPoints.SW, GeoHelper.ToCardinalMark(220D));
			Assert.AreEqual(CardinalPoints.W, GeoHelper.ToCardinalMark(273D));
			Assert.AreEqual(CardinalPoints.NW, GeoHelper.ToCardinalMark(320D));
		}

		[TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ToCardinalMarkOutOfRange()
		{
			GeoHelper.ToCardinalMark(390D);
		}

		[TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void CoordinateLatitudeGreater90()
		{
			new GeoCoordinate{ Latitude = 100, Longitude = -90 };
		}

		[TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void CoordinateLatitudeLessN90()
		{
			new GeoCoordinate{ Latitude = -91, Longitude = -90 };
		}

		[TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void CoordinateLongitudeGreater180()
		{
			new GeoCoordinate{ Latitude = 90, Longitude = 190 };
		}

		[TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void CoordinateLongitudeLessN180()
		{
			new GeoCoordinate{ Latitude = 90, Longitude = -190 };
		}
	}
}
