using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nostradabus.BusinessComponents;
using Nostradabus.BusinessComponents.Exceptions;
using Nostradabus.BusinessEntities;
using Nostradabus.BusinessEntities.Common;
using Nostradabus.WebSite.CustomActionResults;
using Nostradabus.WebSite.Models;
using Nostradabus.Website.Models;

namespace Nostradabus.Website.Controllers
{
    public class MobileController : Controller
    {
		[HttpGet]
		public ActionResult GetNearbyLines(double? latitude, double? longitude)
        {
			try
			{
				#region Validations

				if (!latitude.HasValue) throw new ValidationException("El parámetro 'latitude' es requerido.");

				if (!longitude.HasValue) throw new ValidationException("El parámetro 'longitude' es requerido");

				#endregion Validations

				var model = new NearbyLinesModel();

				// TODO: remove this and get line list from DB
				// then call RouteComponent.GetNearbyLines()

				var rnd = new Random();
				var responseSelector = rnd.Next(1, 7); // creates a number between 1 and 6
				switch(responseSelector)
				{
					case 1:
						model.Lines = new[] { 5, 7, 19, 32, 41, 51, 61, 64, 68, 71, 75, 86, 88, 98, 101, 104, 105, 115, 118, 129, 132, 151, 165, 168, 188 };
						break;

					case 2:
						model.Lines = new[] { 34, 162, 16, 172, 181 };
						break;

					case 3:
						model.Lines = new[] { 53, 106, 135, 107 };
						break;

					case 4:
						model.Lines = new[] { 61, 95, 97, 118, 143, 188 };
						break;

					case 5:
						model.Lines = new[] { 12, 39, 41, 60, 61, 68, 95, 101, 106, 109, 111, 118, 132, 142, 152 };
						break;

					default:
						model.Lines = new int[0];
						break;
				}
				
				return new JsonActionResult(model);
			}
			catch (ValidationException ex)
			{
				return new JsonActionResult(new JsonResponse(ex));
			}
			catch (Exception ex)
			{
				return new JsonActionResult(new JsonResponse(false, Resources.Global.ErrorMessage));
			}
        }

		[HttpPost]
		public ActionResult Checkpoint(DataEntryModel data)
		{
			try
			{
				#region Validations

				if (string.IsNullOrEmpty(data.keyCode)) throw new ValidationException("El parámetro 'keyCode' es requerido.");
				
				if (string.IsNullOrEmpty(data.serialNumber)) throw new ValidationException("El parámetro 'serialNumber' es requerido.");
				
				if (!data.latitude.HasValue) throw new ValidationException("El parámetro 'latitude' es requerido.");

				if (!data.longitude.HasValue) throw new ValidationException("El parámetro 'longitude' es requerido");

				if (string.IsNullOrEmpty(data.datetime)) throw new ValidationException("El parámetro 'datetime' es requerido.");

				if (!IsValidDateTime(data.datetime)) throw new ValidationException("El parámetro 'datetime' es incorrecto.");
				
				if (!IsValidKeyCode(data)) throw new ValidationException("El keyCode es incorrecto.");
				
				#endregion Validations
				
				return new JsonActionResult(new JsonResponse(true));
			}
			catch (ValidationException ex)
			{
				return new JsonActionResult(new JsonResponse(ex));
			}
			catch (Exception ex)
			{
				return new JsonActionResult(new JsonResponse(false, Resources.Global.ErrorMessage));
			}
		}
		
		#region Private Methods

		private static bool IsValidDateTime(string datetime)
		{
			if (string.IsNullOrEmpty(datetime)) return false;

			if (datetime.Length != 14) return false;
			
			try
			{
				var year = Convert.ToInt32(datetime.Substring(0, 4));
				var month = Convert.ToInt32(datetime.Substring(4, 2));
				var day = Convert.ToInt32(datetime.Substring(6, 2));
				var hours = Convert.ToInt32(datetime.Substring(8, 2));
				var minutes = Convert.ToInt32(datetime.Substring(10, 2));
				var seconds = Convert.ToInt32(datetime.Substring(12, 2));

				new DateTime(year, month, day, hours, minutes, seconds);

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
		
		private static bool IsValidKeyCode(DataEntryModel data)
		{
			if (string.IsNullOrEmpty(data.keyCode)) return false;

			var seedString = string.Concat(data.serialNumber, "elgamugol", data.datetime);

			// use Password object to get MD5 hash of seedString
			// and compare to data.keyCode
			return new Password(seedString).Hash.Equals(data.keyCode, StringComparison.InvariantCultureIgnoreCase);
		}

    	#endregion Provate Methods
	}
}
