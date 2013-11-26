using System;
using System.Web.Mvc;
using Nostradabus.Website.Models;

namespace Nostradabus.WebSite.Models.CustomModelBinders
{
	/// <summary>
	/// Binding for Serialezer SerializeViewModels.
	/// </summary>
	public class SerializeViewModelBinder : DefaultModelBinder
	{
		protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
		{
			return base.CreateModel(controllerContext, bindingContext, modelType);
		}

		/// <summary>
		/// Return one entitie Serialize if is a SerializeViewModel.
		/// </summary>
		public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			if(bindingContext.ModelType.IsSubclassOf(typeof(SerializeViewModel)))
			{
				var serializeModel = SerializeViewModel.Deserealize(bindingContext.ModelType, controllerContext.RequestContext.HttpContext.Request.InputStream);
				return serializeModel;

			}

			return base.BindModel(controllerContext, bindingContext);
		}
	}
}