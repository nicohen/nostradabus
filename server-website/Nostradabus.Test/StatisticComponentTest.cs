using System;
using Nostradabus.BusinessComponents;
using Nostradabus.BusinessEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nostradabus.Test
{
	[TestClass]
	public class StatisticComponentTest
	{
		[TestMethod]
		public void GetTimeRange()
		{
			var baseDate = new DateTime(2013, 1, 8);

			// ZeroToFourAm
			Assert.AreEqual(TimeRange.ZeroToFourAm, StatisticComponent.GetTimeRange(baseDate));
			Assert.AreEqual(TimeRange.ZeroToFourAm, StatisticComponent.GetTimeRange(baseDate.Add(new TimeSpan(2,0,0))));
			Assert.AreEqual(TimeRange.ZeroToFourAm, StatisticComponent.GetTimeRange(baseDate.Add(new TimeSpan(3, 59, 59))));

			// FourAmToEightAm
			Assert.AreEqual(TimeRange.FourAmToEightAm, StatisticComponent.GetTimeRange(baseDate.Add(new TimeSpan(4, 0, 0))));
			Assert.AreEqual(TimeRange.FourAmToEightAm, StatisticComponent.GetTimeRange(baseDate.Add(new TimeSpan(6, 0, 0))));
			Assert.AreEqual(TimeRange.FourAmToEightAm, StatisticComponent.GetTimeRange(baseDate.Add(new TimeSpan(7, 59, 59))));

			// EightAmToTwelvePm
			Assert.AreEqual(TimeRange.EightAmToTwelvePm, StatisticComponent.GetTimeRange(baseDate.Add(new TimeSpan(8, 0, 0))));
			Assert.AreEqual(TimeRange.EightAmToTwelvePm, StatisticComponent.GetTimeRange(baseDate.Add(new TimeSpan(10, 0, 0))));
			Assert.AreEqual(TimeRange.EightAmToTwelvePm, StatisticComponent.GetTimeRange(baseDate.Add(new TimeSpan(11, 59, 59))));

			// TwelvePmToFourPm
			Assert.AreEqual(TimeRange.TwelvePmToFourPm, StatisticComponent.GetTimeRange(baseDate.Add(new TimeSpan(12, 0, 0))));
			Assert.AreEqual(TimeRange.TwelvePmToFourPm, StatisticComponent.GetTimeRange(baseDate.Add(new TimeSpan(14, 0, 0))));
			Assert.AreEqual(TimeRange.TwelvePmToFourPm, StatisticComponent.GetTimeRange(baseDate.Add(new TimeSpan(15, 59, 59))));

			// FourPmToEightPm
			Assert.AreEqual(TimeRange.FourPmToEightPm, StatisticComponent.GetTimeRange(baseDate.Add(new TimeSpan(16, 0, 0))));
			Assert.AreEqual(TimeRange.FourPmToEightPm, StatisticComponent.GetTimeRange(baseDate.Add(new TimeSpan(18, 0, 0))));
			Assert.AreEqual(TimeRange.FourPmToEightPm, StatisticComponent.GetTimeRange(baseDate.Add(new TimeSpan(19, 59, 59))));

			// EightPmToZero
			Assert.AreEqual(TimeRange.EightPmToZero, StatisticComponent.GetTimeRange(baseDate.Add(new TimeSpan(20, 0, 0))));
			Assert.AreEqual(TimeRange.EightPmToZero, StatisticComponent.GetTimeRange(baseDate.Add(new TimeSpan(22, 0, 0))));
			Assert.AreEqual(TimeRange.EightPmToZero, StatisticComponent.GetTimeRange(baseDate.Add(new TimeSpan(23, 59, 59))));
		}
	}
}
