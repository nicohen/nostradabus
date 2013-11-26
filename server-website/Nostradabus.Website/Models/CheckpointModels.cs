using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nostradabus.Website.Models
{
	public class CheckpointModel : BaseViewModel 
	{
		public virtual int RouteId { get; set; }

		public virtual string UUID { get; set; }

		public virtual double? Latitude { get; set; }

		public virtual double? Longitude { get; set; }

		public virtual double? Speed { get; set; }

		public virtual DateTime DateTime { get; set; }
	}
}