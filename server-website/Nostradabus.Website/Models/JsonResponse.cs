using System;
using System.Runtime.Serialization;
using Nostradabus.Website.Models;

namespace Nostradabus.WebSite.Models
{
    [DataContract]
    public class JsonResponse : SerializeViewModel
    {
		#region Constructors

		public JsonResponse() : this(true) {}

		public JsonResponse(bool success)
		{
			Success = success;
			MustLogin = false;
		}

		public JsonResponse(bool success, string message) : this(success)
		{
			Message = message;
		}

		public JsonResponse(Exception exception) : this(false, exception.Message) {}

		#endregion Constructors

		#region Properties

		[DataMember(Name = "success")]
		public virtual bool Success { get; set; }

		[DataMember(Name = "mustLogin")]
		public virtual bool MustLogin { get; set; }

		[DataMember(Name = "message")]
		public virtual string Message { get; set; }

		[DataMember(Name = "redirectUrl")]
		public virtual string RedirectUrl { get; set; }

		#endregion Properties
	}
}