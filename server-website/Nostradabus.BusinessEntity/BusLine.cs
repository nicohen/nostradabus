using System.Collections.Generic;
using System.Globalization;
using Nostradabus.BusinessEntities.Common;

namespace Nostradabus.BusinessEntities
{
	public class BusLine : BusinessEntity<int>
	{
		public BusLine()
		{
			Branches = new List<LineBranch>(5);
		}

		public virtual int Number { get; set; }

		public virtual string Description { get; set; }

		public virtual List<LineBranch> Branches { get; set; }

		public override string ToString()
		{
			return Number.ToString(CultureInfo.InvariantCulture);
		}
	}
}
