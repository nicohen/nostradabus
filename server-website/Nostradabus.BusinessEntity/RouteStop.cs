using Nostradabus.BusinessEntities.Common;

namespace Nostradabus.BusinessEntities
{
	public class RouteStop : BusinessEntity<int>
	{
		public virtual int RouteId { get; set; }
		public virtual int Stop { get; set; }
		public virtual double Latitude { get; set; }
		public virtual double Longitude { get; set; }
	}
}
