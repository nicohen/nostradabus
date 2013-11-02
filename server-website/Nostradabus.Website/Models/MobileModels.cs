using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Nostradabus.WebSite.Models;

namespace Nostradabus.Website.Models
{
	[DataContract]
	public class NearbyLinesModel : JsonResponse
	{
		[DataMember(Name = "lines")]
		public virtual int[] Lines { get; set; }
	}

	[DataContract]
	public class DataEntryModel : JsonResponse
	{
		public virtual string serialNumber { get; set; }

		public virtual string tripKey { get; set; }

		public virtual int line { get; set; }

		public virtual double? latitude { get; set; }

		public virtual double? longitude { get; set; }

		public virtual string datetime { get; set; }
		
		public virtual double? speed { get; set; }

		public virtual double? batteryStatus { get; set; }
		
		public virtual string keyCode { get; set; }
	}
}