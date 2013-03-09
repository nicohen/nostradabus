using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nostradabus.BusinessEntities;

namespace Nostradabus.Persistence.Interfaces
{
	public interface IStatisticCheckpointPersistence
	{
		List<HistoricalCheckpoint> GetByRoute(Route route, DateTime fromDate, DateTime toDate);
	}
}
