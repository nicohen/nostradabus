using System;
using System.Runtime.Serialization.Json;
using System.Web.Mvc;
using Nostradabus.WebSite.Models;
using Nostradabus.Website.Models;

namespace Nostradabus.WebSite.CustomActionResults
{
    /// <summary>
    /// Return one Json with the serialization of one SerializeViewModel.
    /// </summary>
    public class JsonActionResult: ActionResult 
    {
		private string ContentType { get; set; }

        public JsonActionResult(SerializeViewModel data)     
        {         
            this.Data = data;     
        }

		public JsonActionResult(SerializeViewModel data,string contentType)
		{
			this.Data = data;
			this.ContentType = contentType;
		}

        public SerializeViewModel Data { get; private set; }
      
        public override void ExecuteResult(ControllerContext context)
        {
            var serializer = new DataContractJsonSerializer(this.Data.GetType());

            String output = String.Empty;

            output = Data.Serialize();

            context.HttpContext.Response.ContentType = String.IsNullOrWhiteSpace(ContentType)? "application/json":ContentType;

            context.HttpContext.Response.Write(output);
        }
    }
}