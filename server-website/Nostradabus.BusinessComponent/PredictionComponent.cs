using System;
using System.Collections.Generic;
using System.Linq;
using Nostradabus.BusinessEntities;
using Nostradabus.Common;

namespace Nostradabus.BusinessComponents
{
	public class PredictionComponent
	{
		public PredictionComponent()
		{
			SpeedAverageMaxSamples = 4;
			UseStatistics = true;
			UseSpeedWeightCoef = false;
			SpeedExtrapolationMaxDistance = 100;
			UseAverageTimeOffsetCoef = false;
			AverageTimeOffsetMinSamples = 6;
		}

		#region Properties

		public int SpeedAverageMaxSamples { get; set; }

		// determines if speed samples have the same or different weight 
		// (true = use different weight, false = uniform)
		public bool UseSpeedWeightCoef { get; set; }

		public bool UseStatistics { get; set; }

		// determines the max distance (in meters) to use only speed extrapolation,
		// when UseStatistics = true
		public int SpeedExtrapolationMaxDistance { get; set; }

		public bool UseAverageTimeOffsetCoef { get; set; }

		public int AverageTimeOffsetMinSamples { get; set; }

		#endregion Properties
		
		public PredictionResult CalculateArrivalTime(int lineNumber, string branchCode, RouteDirection direction, int toStop)
		{
			var route = RouteComponent.Instance.GetRoute(lineNumber, branchCode, direction);

			// TODO: handle different bus line branches issue
			// i.e.: the user needs specific branch vs. any branch is ok
			// also: the checkpoints could be for specific branch or maybe UNKOWN branch

			return CalculateArrivalTime(route, toStop);
		}

		public PredictionResult CalculateArrivalTime(Route route, int toStop)
		{
			var checkpoints = CheckpointComponent.Instance.GetByRouteFromCache(route);

			// TODO: add logic to get the right checkpoints to use to estimate times
			// analyze: from X stops before, total of X checkpoints, progressive search, etc
			//checkpoints = checkpoints.Where(c => c.NextStopIndex <= toStop && c.DateTime >= DateTime.Now.AddHours(-2)).ToList();
			checkpoints = checkpoints.Where(c => c.NextStopIndex <= toStop).ToList();

			return CalculateArrivalTime(route, toStop, checkpoints);
		}
		
		public PredictionResult CalculateArrivalTime(Route route, int toStop, List<Checkpoint> checkpoints)
		{
			var result = new PredictionResult();

			// if we dont have any checkpoint, 
			// now we cannot make an estimation
			// TODO: figure out how to use statistic or time tables here and return some result
			if (!checkpoints.Any()) return result;


			// distiguish checkpoints by UUID
			List<string> uuids = checkpoints.Select(c => c.UUID).Distinct().ToList();
			
			var totalEstimation = new TimeSpan(0);

			// TODO: set priority by date (recently first)

			// TODO: figure out how to distinguish different bus units
			// we shouldn't calculate time average using different units, 
			// better is to inform those times separately
			// Possible ways to distinguish bus units:
			//	- comparing checkpoints from different UUIDs
			//	- comparing final time estimation and using some threshold

			foreach (var uuid in uuids)
			{
				// calculate the arrival time using the informer checkpoints
				var informerEstimation = CalculateArrivalTimeByInformer(route, toStop, checkpoints.Where(c => c.UUID.Equals(uuid)).ToList());
				if (informerEstimation != null)
				{
					result.EstimationByInformer.Add(uuid, informerEstimation.Value);

					// accumulate time for future average calculation
					totalEstimation = totalEstimation.Add(informerEstimation.Value);

					result.InformersCount++;
				}
			}

			// if we dont have any valid data by Informers, 
			// we cannot make an estimation..
			if (result.InformersCount == 0) return result;

			result.ArrivalTimeByEstimation = new TimeSpan(0, 0, 0, Convert.ToInt32(totalEstimation.TotalSeconds / result.InformersCount));

			return result;
		}
		
		public TimeSpan? CalculateArrivalTimeByInformer(Route route, int toStop, List<Checkpoint> informerCheckpoints)
		{
			// filter only the checkpoints before the destination stop
			informerCheckpoints = informerCheckpoints.Where(c => c.NextStopIndex <= toStop).ToList();

			// TODO: filter checkpoints to reduce list items, remove further checkpoints that will not be used
			// (sampleLength), figure out how to know if we can trim lower (by nextStopIndex) checkpoints

			// we need at least one point to estimate some time
			if (informerCheckpoints.Count < 1) return null;

			// we need at least two points to calculate speed (if we are not using stats)
			if (!UseStatistics && informerCheckpoints.Count < 2) return null;
			
			// first order by NextStopIndex, DateTime
			informerCheckpoints = informerCheckpoints.OrderBy(c => c.NextStopIndex).ThenBy(c => c.DateTime).ToList();

			var stopsTimePerMeter = new List<double>();

			// this list stores the offset between real/registered time or speed
			// and the time or speed from the statistics, later will be used to calculate 
			// an adjustment coeficient for the statistic times between stops. 
			var realTimeOffsets = new List<double>();

			int currentNextStop = informerCheckpoints.First().NextStopIndex;
			int fromCheckIndex = 0;

			for (int i = 0; i < informerCheckpoints.Count; i++)
			{
				// verify if the checkpoint jumped to another stop
				if (informerCheckpoints[i].NextStopIndex != currentNextStop)
				{
					// distance from the stop of the fromCheckIndex checkpoint and the stop of the current checkpoint
					// the distance is calculated based on stops because of the grid shape of the map, we dont want the shortest distance
					var distance = RouteComponent.Instance.GetDistanceBetweenStops(route, currentNextStop - 1, informerCheckpoints[i].NextStopIndex - 1);

					var substractDistance = RouteComponent.Instance.GetDistanceToStop(route, informerCheckpoints[fromCheckIndex].Coordinate, currentNextStop - 1);

					var aditionalDistance = RouteComponent.Instance.GetDistanceToStop(route, informerCheckpoints[i].Coordinate, informerCheckpoints[i].NextStopIndex - 1);

					distance = distance + aditionalDistance - substractDistance;

					if (distance > 0)
					{
						var time = informerCheckpoints[i].DateTime.Subtract(informerCheckpoints[fromCheckIndex].DateTime);

						if(time.TotalSeconds > 0)
						{
							// time (in seconds) it takes to go 1 meter
							var timePerMeter = time.TotalSeconds / distance;
							stopsTimePerMeter.Add(timePerMeter);

							// calculate average time per meter from the stats
							var stepDistance = RouteComponent.Instance.GetDistanceBetweenStops(route, currentNextStop - 1, informerCheckpoints[i].NextStopIndex - 1);
							var stepStatsTime = 0.0;

							for(var j = currentNextStop-1; j < informerCheckpoints[i].NextStopIndex-1; j++)
							{
								var statisticItem = StatisticComponent.Instance.Get(route, j, informerCheckpoints[fromCheckIndex].DateTime);
								if (statisticItem != null)
								{
									stepStatsTime = stepStatsTime + statisticItem.TimeToNextStop;
								}
								else
								{
									stepStatsTime = stepStatsTime + (timePerMeter * RouteComponent.Instance.GetDistanceBetweenStops(route, j, j+1));
								}
							}

							// add the offset for this step
							var realTimeOffset = timePerMeter/(stepStatsTime/stepDistance);
							if (!double.IsNaN(realTimeOffset) && !double.IsInfinity(realTimeOffset)) 
								realTimeOffsets.Add(realTimeOffset);
						}
					}
					
					currentNextStop = informerCheckpoints[i].NextStopIndex;
					fromCheckIndex = i;
				}
			}

			// there are some checkpoints with the same nextStop 
			// that have not been calculated yet
			if(fromCheckIndex < informerCheckpoints.Count-1)
			{
				var distance = GeoHelper.Distance(informerCheckpoints[fromCheckIndex].Coordinate, informerCheckpoints.Last().Coordinate);

				if (distance > 0)
				{
					var time = informerCheckpoints.Last().DateTime.Subtract(informerCheckpoints[fromCheckIndex].DateTime);

					if (time.TotalSeconds > 0)
					{
						// time (in seconds) it takes to go 1 meter
						stopsTimePerMeter.Add(time.TotalSeconds / distance);
					}
				}
			}

			// now, calculate the average speed between the first and last checkpoint
			// if both checkpoints are not in the same stop, otherwise we have already calculate that
			if (informerCheckpoints.First().NextStopIndex < informerCheckpoints.Last().NextStopIndex)
			{
				var totalDistance = RouteComponent.Instance.GetDistanceBetweenStops(route, informerCheckpoints.First().NextStopIndex - 1, informerCheckpoints.Last().NextStopIndex - 1);

				var substractDistance = RouteComponent.Instance.GetDistanceToStop(route, informerCheckpoints.First().Coordinate, informerCheckpoints.First().NextStopIndex - 1);

				var aditionalDistance = RouteComponent.Instance.GetDistanceToStop(route, informerCheckpoints.Last().Coordinate, informerCheckpoints.Last().NextStopIndex - 1);

				totalDistance = totalDistance + aditionalDistance - substractDistance;
				if (totalDistance > 0)
				{
					var time = informerCheckpoints.Last().DateTime.Subtract(informerCheckpoints.First().DateTime);
					if (time.TotalSeconds > 0)
					{
						// time (in seconds) it takes to go 1 meter
						var totalTimePerMeter = time.TotalSeconds / totalDistance;
						
						// copy last time in the new last position
						stopsTimePerMeter.Add(stopsTimePerMeter[stopsTimePerMeter.Count-1]);

						// put the totalTimePerMeter in second place of relevance
						stopsTimePerMeter[stopsTimePerMeter.Count - 2] = totalTimePerMeter;
					}
				}
			}

			var stopsTimePerMeterSubset = stopsTimePerMeter.Count > SpeedAverageMaxSamples
											? stopsTimePerMeter.Skip(stopsTimePerMeter.Count - SpeedAverageMaxSamples).ToList()
			                              	: stopsTimePerMeter;


			var averageTimePerMeter = 0.0;

			if(UseSpeedWeightCoef)
			{
				var weightCoefficients = GetWeightCoefficients(stopsTimePerMeterSubset.Count);
				
				for (var i = 0; i < stopsTimePerMeterSubset.Count; i++)
				{
					averageTimePerMeter += weightCoefficients[i] * stopsTimePerMeterSubset[i];
				}
			}
			else
			{
				averageTimePerMeter = stopsTimePerMeterSubset.Sum() / stopsTimePerMeterSubset.Count;
			}
			
			
			const int extrapolationThreshold = 200; // meters between points

			var futureDistance = RouteComponent.Instance.GetDistanceBetweenStops(route, informerCheckpoints.Last().NextStopIndex - 1, toStop);
			var substractFutureDistance = RouteComponent.Instance.GetDistanceToStop(route, informerCheckpoints.Last().Coordinate, informerCheckpoints.Last().NextStopIndex - 1);
			futureDistance = futureDistance - substractFutureDistance;

			// if destination stop is close from last checkpoint, 
			// just extrapolate delay using current average speed
			// (and we have enough checkpoints to calculate average speed)
			if (!UseStatistics || futureDistance <= SpeedExtrapolationMaxDistance)
			{
				if (!stopsTimePerMeterSubset.Any()) return null;

				if (futureDistance <= 0) return null;

				return new TimeSpan(0, 0, Convert.ToInt32(futureDistance * averageTimePerMeter));
			}
			else // destination is a little further from the last known checkpoint
			{
				var result = new TimeSpan(0, 0, 0);

				var averageTimeOffsetCoef = realTimeOffsets.Count >= AverageTimeOffsetMinSamples ? realTimeOffsets.Sum() / realTimeOffsets.Count : 1;

				// extrapolate first steps
				var extrapolationDistance = RouteComponent.Instance.GetDistanceBetweenStops(route, informerCheckpoints.Last().NextStopIndex - 1, informerCheckpoints.Last().NextStopIndex);
				var substractExtrapolatioDistance = RouteComponent.Instance.GetDistanceToStop(route, informerCheckpoints.Last().Coordinate, informerCheckpoints.Last().NextStopIndex - 1);
				extrapolationDistance = extrapolationDistance - substractExtrapolatioDistance;
				if (extrapolationDistance > 0)
				{
					if (!stopsTimePerMeterSubset.Any()) return null;

					result = result.Add(new TimeSpan(0, 0, Convert.ToInt32(extrapolationDistance * averageTimePerMeter)));
				}
				
				// use statistics if possible to calculate next steps
				for (var i = informerCheckpoints.Last().NextStopIndex; i < toStop; i++)
				{
					var statisticItem = StatisticComponent.Instance.Get(route, i, informerCheckpoints.Last().DateTime.Add(result));
					if(statisticItem != null)
					{
						if(UseAverageTimeOffsetCoef)
						{
							result = result.Add(new TimeSpan(0, 0, Convert.ToInt32(Math.Round(statisticItem.TimeToNextStop * averageTimeOffsetCoef))));
						}
						else
						{
							result = result.Add(new TimeSpan(0, 0, statisticItem.TimeToNextStop));	
						}
					}
					else
					{
						if (!stopsTimePerMeterSubset.Any()) return null;

						var distanceToNextStop = RouteComponent.Instance.GetDistanceBetweenStops(route, i, i+1);
						result = result.Add(new TimeSpan(0, 0, Convert.ToInt32(distanceToNextStop * averageTimePerMeter)));
					}
				}

				return result;
			}
		}

		#region Private Methods

		/// <summary>
		/// Gets the weight coefficients, 
		/// assuming that last position (in the time array) is the closest one
		/// and the first one is the furthest, therefore the less important
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		private static double[] GetWeightCoefficients(int count)
		{
			if(count <= 0) return new double[0];

			var reminder = 100.0;

			var result = new double[count];

			for (var i = 0; i < count - 1; i++)
			{
				reminder = reminder / 2.0;

				// fill result array from the last position 
				// to the first position
				result[count-i-1] = reminder / 100.0;
			}

			result[0] = reminder / 100.0;

			return result;
		}
		
		#endregion Private Methods
	}


	public class PredictionResult
	{
		public PredictionResult()
		{
			EstimationByInformer = new Dictionary<string, TimeSpan>(5);
		}

		public int InformersCount { get; set; }

		public Dictionary<string, TimeSpan> EstimationByInformer { get; set; }

		public TimeSpan ArrivalTimeByTimeTable { get; set; }

		public TimeSpan ArrivalTimeByStatistics { get; set; }
 
		public TimeSpan ArrivalTimeByEstimation { get; set; }
	}

}
