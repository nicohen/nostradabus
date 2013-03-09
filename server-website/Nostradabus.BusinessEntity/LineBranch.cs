using System;
using Nostradabus.BusinessEntities.Common;

namespace Nostradabus.BusinessEntities
{
	public class LineBranch : BusinessEntity<int>
	{
		public virtual string Code { get; set; }

		public virtual string Name { get; set; }

		public virtual string Description { get; set; }

		public virtual Route Going { get; set; }

		public virtual Route Return { get; set; }

		public override string ToString()
		{
			return String.Format("{0} - {1}", Code, Name);
		}
	}
}
