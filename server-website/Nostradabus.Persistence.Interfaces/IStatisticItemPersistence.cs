using System.Collections.Generic;
using Nostradabus.BusinessEntities;
using Nostradabus.Persistence.Interfaces.Common;

namespace Nostradabus.Persistence.Interfaces
{
	public interface IStatisticItemPersistence : IPersistence<StatisticItem>
	{
		List<StatisticItem> GetByCalculationAndDayType(StatisticCalculation calculation, DayType dayType);

		List<StatisticItem> GetByCalculation(StatisticCalculation lastCalculation);
	}
}
