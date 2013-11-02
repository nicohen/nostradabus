using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nostradabus.BusinessComponents;
using Nostradabus.BusinessEntities;
using Nostradabus.Common;
using Nostradabus.Website.Models;

namespace Nostradabus.Website.Controllers
{
	public class HomeController : Controller
	{
		// GET: /Home/
		public ActionResult Index()
		{
			var model = new TestsModel();

			return View(model);
		}

		[HttpPost]
		public ActionResult Index(TestsModel model)
		{
			try
			{
				var route = RouteComponent.Instance.GetRoute(model.Line, model.Branch, model.RouteDirection == "going" ? RouteDirection.Going : RouteDirection.Return);
			
				if(route != null)
				{
					// Perímetro de radio X desde coordenada
					if (!model.BoundingBox_Coord1.IsUnknown && model.BoundingBox_Radius.HasValue)
					{
						var boundingBox = GeoHelper.GetBoundingBox(model.BoundingBox_Coord1, Convert.ToDouble(model.BoundingBox_Radius.Value));
						model.BoundingBox_ResultMin = boundingBox.MinPoint;
						model.BoundingBox_ResultMax = boundingBox.MaxPoint;
					}
					
					// Distancia entre coordenadas: coord1, coord2
					if(!model.DistanceBetweenPoints_Coord1.IsUnknown && !model.DistanceBetweenPoints_Coord2.IsUnknown)
					{
						model.DistanceBetweenPoints_Result = GeoHelper.Distance(model.DistanceBetweenPoints_Coord1, model.DistanceBetweenPoints_Coord2);
					}

					// Distancia entre coord y parada: coord1, parada1
					if (!model.DistanceBetweenPointAndStop_Coord1.IsUnknown && model.DistanceBetweenPointAndStop_Stop.HasValue)
					{
						if (model.DistanceBetweenPointAndStop_Stop.Value < 0 || model.DistanceBetweenPointAndStop_Stop.Value > route.Stops.Count - 1)
						{
							model.Message += "<br/>El número de parada es inválida para la operación 'Distancia entre punto y parada'";
						}
						else
						{
							model.DistanceBetweenPointAndStop_Result = RouteComponent.Instance.GetDistanceToStop(route, model.DistanceBetweenPointAndStop_Coord1, model.DistanceBetweenPointAndStop_Stop.Value);
						}
					}

					// Ubicación de la parada: parada1
					if(model.CoordinateOfStop_Stop.HasValue)
					{
						model.CoordinateOfStop_Result = route.Stops[model.CoordinateOfStop_Stop.Value];
					}
					
					//Parada mas cercana desde: coord1
					if (!model.ClosestStop_Coord.IsUnknown)
					{
						model.ClosestStop_Result = RouteComponent.Instance.GetClosestStop(route, model.ClosestStop_Coord);
					}

					//Próxima parada desde: coord1
					if (!model.NextStop_Coord.IsUnknown)
					{
						model.NextStop_Result = RouteComponent.Instance.GetNextStop(route, model.NextStop_Coord);
					}

					//Distancia entre paradas: parada1, parada2
					if (model.DistanceBetweenStops_Stop1.HasValue && model.DistanceBetweenStops_Stop2.HasValue)
					{
						var valid = true;

						if (model.DistanceBetweenStops_Stop1.Value < 0 || model.DistanceBetweenStops_Stop1.Value > route.Stops.Count - 1)
						{
							valid = false; 
							model.Message += "<br/>El número de parada 'desde' es inválida para la operación 'Distancia entre paradas'";
						}

						if (model.DistanceBetweenStops_Stop2.Value < 0 || model.DistanceBetweenStops_Stop2.Value > route.Stops.Count - 1)
						{
							valid = false; 
							model.Message += "<br/>El número de parada 'hasta' es inválida para la operación 'Distancia entre paradas'";
						}

						if(valid) 
							model.DistanceBetweenStops_Result = RouteComponent.Instance.GetDistanceBetweenStops(route, model.DistanceBetweenStops_Stop1.Value, model.DistanceBetweenStops_Stop2.Value);
					}
				}
				else
				{
					model.Message = "No se encontró la ruta, no se pueden realizar los cálculos";
				}
			}
			catch (Exception ex)
			{
				model.Message += "<br/>" + ex.Message;
			}

			return View(model);
		}

		public ActionResult Test()
		{
			return View();
		}

		public ActionResult ParamTest()
		{
			return View();
		}

	}
}
