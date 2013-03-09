using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nostradabus.BusinessComponents;
using Nostradabus.BusinessComponents.Exceptions;
using Nostradabus.BusinessEntities;
using Nostradabus.Website.Models;

namespace Nostradabus.Website.Controllers
{
    public class CheckpointController : Controller
    {
        public ActionResult Add()
        {
        	var model = new CheckpointModel();

			return View(model);
        }

		[HttpPost]
		public ActionResult Add(CheckpointModel model)
		{
			try
			{
				var checkpoint = ToEntity(model);

				CheckpointComponent.Instance.Add(checkpoint);

				model.Messages.Add("Se ha registrado el checkpoint con éxito");
			}
			catch (ValidationException ex)
			{
				model.AddValidationErrors(ex);
			}
			catch (Exception ex)
			{
				model.ErrorMessages.Add(Resources.Global.ErrorMessage);

				Common.Logger.LoggingManager.Logging.Error(ex);
			}
			return View(model);
		}


		#region Provate Methods

		private static Checkpoint ToEntity(CheckpointModel model)
		{
			var entity = new Checkpoint
			{
			    Route = RouteComponent.Instance.GetById(model.RouteId), 
				UUID = model.UUID,
				DateTime = model.DateTime
			};
			
			if(model.Latitude.HasValue && model.Longitude.HasValue)
			{
				entity.Coordinate = new GeoCoordinate(model.Latitude.Value, model.Longitude.Value);
			}
			
			return entity;
		}

    	#endregion Provate Methods

	}
}
