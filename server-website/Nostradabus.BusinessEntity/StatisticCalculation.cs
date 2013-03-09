using System;
using Nostradabus.BusinessEntities.Common;

namespace Nostradabus.BusinessEntities
{
	public class StatisticCalculation : BusinessEntity<int>
	{
		public StatisticCalculation() : base() { }

		public StatisticCalculation(int id) : base(id) { }

		public virtual DateTime StartDate { get; set; }

		public virtual DateTime? EndDate { get; set; }
	}
}
