using System;
using System.Collections.Generic;
using Nostradabus.BusinessEntities;
using Nostradabus.Persistence.Interfaces.Common;

namespace Nostradabus.Persistence.Interfaces
{
	public interface IStatisticCalculationPersistence : IPersistence<StatisticCalculation>
	{
		StatisticCalculation GetLast();

		StatisticCalculation BulkSave(List<StatisticItem> statItems, DateTime startDate);
	}
}
