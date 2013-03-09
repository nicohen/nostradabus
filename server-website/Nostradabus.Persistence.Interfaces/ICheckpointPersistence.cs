using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nostradabus.BusinessEntities;
using Nostradabus.Persistence.Interfaces.Common;

namespace Nostradabus.Persistence.Interfaces
{
	public interface ICheckpointPersistence : IPersistence<Checkpoint>
	{
		List<Checkpoint> GetByRoute(Route route, DateTime fromDate, DateTime toDate);
	}
}
