using Nostradabus.BusinessComponents;
using Nostradabus.BusinessEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nostradabus.Test
{
	[TestClass]
	public class RouteComponentTest
	{
		private readonly Route _route = RouteComponent.Instance.GetRoute(71, "A", RouteDirection.Going);

		#region GetDistanceBetweenStops

		[TestMethod]
		public void GetDistanceBetweenStops_SamePointTest()
		{
			Assert.AreEqual(0, RouteComponent.Instance.GetDistanceBetweenStops(_route, 14, 14));

			Assert.AreEqual(0, RouteComponent.Instance.GetDistanceBetweenStops(_route, 37, 37));

			Assert.AreEqual(0, RouteComponent.Instance.GetDistanceBetweenStops(_route, 78, 78));

			Assert.AreEqual(0, RouteComponent.Instance.GetDistanceBetweenStops(_route, 63, 63));
		}

		[TestMethod]
		public void GetDistanceBetweenStops_DifferentPointsTest()
		{
			Assert.IsTrue(0 < RouteComponent.Instance.GetDistanceBetweenStops(_route, 14, 16));

			Assert.IsTrue(0 < RouteComponent.Instance.GetDistanceBetweenStops(_route, 45, 16));
		}

		[TestMethod]
		public void GetDistanceBetweenStops_SameInBothDirectionsTest()
		{
			var distance = RouteComponent.Instance.GetDistanceBetweenStops(_route, 45, 16);

			var distanceR = RouteComponent.Instance.GetDistanceBetweenStops(_route, 16, 45);

			Assert.AreEqual(distance, distanceR);
		}

		[TestMethod]
		public void GetDistanceBetweenStops_TransitivityTest()
		{
			var distance1 = RouteComponent.Instance.GetDistanceBetweenStops(_route, 26, 27);

			var distance2 = RouteComponent.Instance.GetDistanceBetweenStops(_route, 27, 28);

			var distance3 = RouteComponent.Instance.GetDistanceBetweenStops(_route, 28, 29);

			var distance = RouteComponent.Instance.GetDistanceBetweenStops(_route, 26, 29);

			Assert.AreEqual(distance1 + distance2 + distance3, distance);
		}

		#endregion GetDistanceBetweenStops

		[TestMethod]
		public void GetDistanceToNextStopTest()
		{
			var distance1 = RouteComponent.Instance.GetDistanceBetweenStops(_route, 26, 27);
			
			var distance = RouteComponent.Instance.GetDistanceToNextStop(_route, 26);

			Assert.AreEqual(distance1, distance);
		}

		[TestMethod]
		public void GetDistanceToPrevStopTest()
		{
			var distance1 = RouteComponent.Instance.GetDistanceBetweenStops(_route, 26, 25);

			var distance = RouteComponent.Instance.GetDistanceToPrevStop(_route, 26);

			Assert.AreEqual(distance1, distance);
		}

	}
}
