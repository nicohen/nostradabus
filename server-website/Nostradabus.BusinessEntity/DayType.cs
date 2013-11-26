using System;
using Nostradabus.BusinessEntities.Common;

namespace Nostradabus.BusinessEntities
{
	public class DayType : NameDescription<int>
	{
		protected static Configuration.ConfigurationManager Configuration = Nostradabus.Configuration.ConfigurationManager.Instance;

		public static DayType LaborDay = new DayType(Int32.Parse(Configuration.GetBusinessEntityId("DayType", "LaborDay")));
		public static DayType Saturday = new DayType(Int32.Parse(Configuration.GetBusinessEntityId("DayType", "Saturday")));
		public static DayType SundayOrHoliday = new DayType(Int32.Parse(Configuration.GetBusinessEntityId("DayType", "SundayOrHoliday")));

		public DayType() : base() { }
		public DayType(int id) : base(id) { }
	}
}
