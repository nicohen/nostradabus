using Nostradabus.BusinessEntities.Common;

namespace Nostradabus.BusinessEntities
{
	public class StatisticItem : BusinessEntity<int>
	{
		public virtual StatisticCalculation Calculation { get; set; }
		
		public virtual Route Route { get; set; }
		
		public virtual DayType DayType { get; set; }

		public virtual TimeRange TimeRange { get; set; }

		public virtual int FromStop { get; set; }

		// time in seconds
		public virtual int TimeToNextStop { get; set; }

		// average speed
		public virtual double? Speed { get; set; }

		// TODO: review how to calculate this
		public virtual int? Frequency { get; set; }

		public virtual int SampleCount { get; set; } 
	}
}
