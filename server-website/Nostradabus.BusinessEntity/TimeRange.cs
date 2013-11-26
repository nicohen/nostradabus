using System;
using Nostradabus.BusinessEntities.Common;
using Nostradabus.Common;

namespace Nostradabus.BusinessEntities
{
	public class TimeRange : NameDescription<int>
	{
		protected static Configuration.ConfigurationManager Configuration = Nostradabus.Configuration.ConfigurationManager.Instance;

		public static TimeRange ZeroToFourAm = new TimeRange(Int32.Parse(Configuration.GetBusinessEntityId("TimeRange", "ZeroToFourAm"))) { Start = new TimeSpan(0, 0, 0), End = new TimeSpan(3, 59, 59) };
		public static TimeRange FourAmToEightAm = new TimeRange(Int32.Parse(Configuration.GetBusinessEntityId("TimeRange", "FourAmToEightAm"))) { Start = new TimeSpan(4, 0, 0), End = new TimeSpan(7, 59, 59) };
		public static TimeRange EightAmToTwelvePm = new TimeRange(Int32.Parse(Configuration.GetBusinessEntityId("TimeRange", "EightAmToTwelvePm"))) { Start = new TimeSpan(8, 0, 0), End = new TimeSpan(11, 59, 59) };
		public static TimeRange TwelvePmToFourPm = new TimeRange(Int32.Parse(Configuration.GetBusinessEntityId("TimeRange", "TwelvePmToFourPm"))) { Start = new TimeSpan(12, 0, 0), End = new TimeSpan(15, 59, 59) };
		public static TimeRange FourPmToEightPm = new TimeRange(Int32.Parse(Configuration.GetBusinessEntityId("TimeRange", "FourPmToEightPm"))) { Start = new TimeSpan(16, 0, 0), End = new TimeSpan(19, 59, 59) };
		public static TimeRange EightPmToZero = new TimeRange(Int32.Parse(Configuration.GetBusinessEntityId("TimeRange", "EightPmToZero"))) { Start = new TimeSpan(20, 0, 0), End = new TimeSpan(23, 59, 59) };
		
		public TimeRange() : base() { }
		public TimeRange(int id) : base(id) { }
		
		//// torubles with MySql TIME type
		//protected string StartDateHack {
		//    get { return Start.ToString("HH:mm:ss"); } 
		//    set
		//    {
		//        var timeParts = value.Split(':');
		//        Start = new TimeSpan(Convert.ToInt32(timeParts[0]), Convert.ToInt32(timeParts[1]), Convert.ToInt32(timeParts[2]));
		//    }
		//}
		public virtual TimeSpan Start { get; set; }

		//// torubles with MySql TIME type
		//protected string EndDateHack
		//{
		//    get { return End.ToString("HH:mm:ss"); }
		//    set
		//    {
		//        var timeParts = value.Split(':');
		//        End = new TimeSpan(Convert.ToInt32(timeParts[0]), Convert.ToInt32(timeParts[1]), Convert.ToInt32(timeParts[2]));
		//    }
		//}
		public virtual TimeSpan End { get; set; }

		public virtual bool Match(TimeSpan time)
		{
			return time >= Start && time <= End;
		}

		public virtual bool MatchNow()
		{
			return Match(DateTimeHelper.Now().TimeOfDay);
		}

		public virtual TimeRange PreviousTimeRange()
		{
			if (Equals(ZeroToFourAm)) return EightPmToZero;

			if (Equals(FourAmToEightAm)) return ZeroToFourAm;

			if (Equals(EightAmToTwelvePm)) return FourAmToEightAm;

			if (Equals(TwelvePmToFourPm)) return EightAmToTwelvePm;

			if (Equals(FourPmToEightPm)) return TwelvePmToFourPm;
			
			// if (Equals(TimeRange.EightPmToZero)) 
			return FourPmToEightPm;
		}

		public virtual TimeRange PreviousTimeRange(int steps)
		{
			if (steps == 0) return this;

			if (steps < 0) return NextTimeRange(steps*-1);

			// steps > 0
			var result = this;
			for (var i = 0; i < steps; i++)
			{
				result = result.PreviousTimeRange();
			}

			return result;
		}

		public virtual TimeRange NextTimeRange()
		{
			if (Equals(ZeroToFourAm)) return FourAmToEightAm;

			if (Equals(FourAmToEightAm)) return EightAmToTwelvePm;

			if (Equals(EightAmToTwelvePm)) return TwelvePmToFourPm;

			if (Equals(TwelvePmToFourPm)) return FourPmToEightPm;

			if (Equals(FourPmToEightPm)) return EightPmToZero;

			// if (Equals(TimeRange.EightPmToZero)) 
			return ZeroToFourAm;
		}

		public virtual TimeRange NextTimeRange(int steps)
		{
			if (steps == 0) return this;

			if (steps < 0) return PreviousTimeRange(steps * -1);

			// steps > 0
			var result = this;
			for (var i = 0; i < steps; i++)
			{
				result = result.NextTimeRange();
			}

			return result;
		}

	}
}
