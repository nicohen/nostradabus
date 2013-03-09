using System;
using System.Collections.Generic;
using System.Linq;
using Nostradabus.BusinessComponents.Common;
using Nostradabus.BusinessEntities;
using Nostradabus.Common;
using Nostradabus.Persistence.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace Nostradabus.BusinessComponents
{
	public class StatisticComponent
	{
		#region Protected variables

		static StatisticComponent _instance;

		static readonly object Padlock = new object();
		
		private Dictionary<int, RouteStats> _statsCache;

		private DayType _currentDayType;

		private Dictionary<int, List<StatisticItem>> _statsCachePerDay;

		#endregion

		#region Singleton implementation

		/// <summary>
		/// Gets the instance of StatisticComponent.
        /// </summary>
        /// <value>The instance.</value>
        public static StatisticComponent Instance
        {
            get
            {
                lock (Padlock)
                {
					return _instance ?? (_instance = new StatisticComponent());
                }
            }
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="RouteComponent"/> class.
        /// </summary>
		private StatisticComponent()
        {
			Initialize();
        }
		
		#endregion

		#region Methods

		/// <summary>
		/// Get StatisticItem for specified Route, Stop and Date (matching DayType and TimeRange for the specified date)
		/// </summary>
		/// <param name="route"></param>
		/// <param name="stop"></param>
		/// <param name="date"></param>
		/// <returns></returns>
		public StatisticItem Get(Route route, int stop, DateTime date)
		{
			var dayType = GetDayType(date);
			var timeRange = GetTimeRange(date);

			return Get(route, stop, dayType, timeRange);
		}

		/// <summary>
		/// Get StatisticItem for specified Route, Stop, DayType and TimeRange
		/// </summary>
		/// <param name="route"></param>
		/// <param name="stop"></param>
		/// <param name="dayType"></param>
		/// <param name="timeRange"></param>
		/// <returns></returns>
		public StatisticItem Get(Route route, int stop, DayType dayType, TimeRange timeRange)
		{
			VerifyCacheDayTime();

			// verify if we already have the stats for this dayType in the cache
			if (dayType.Equals(_currentDayType))
			{
				if (_statsCache.ContainsKey(route.ID))
				{
					var dayTypeStats = _statsCache[route.ID].GetByDayType(_currentDayType);

					if (dayTypeStats != null)
					{
						var statisticItem = dayTypeStats.GetByStopAndTimeRange(stop, timeRange);

						// if there is no stat for this case, try to estimate it from other stats
						if (statisticItem == null)
						{
							statisticItem = EstimateStatisticItem(route, stop, dayType, timeRange);

							// if we could estimate the stats, save it for future uses
							if (statisticItem != null) AddCacheItem(statisticItem);
						}

						return statisticItem;
					}

					return null;
				}

				return null;
			}

			// dayType != _currentDayType, therefore we don't have desired stats in the cache
			if (!_statsCachePerDay.ContainsKey(dayType.ID)) 
				_statsCachePerDay.Add(dayType.ID, GetByDayType(dayType));

			var statsItems = _statsCachePerDay[dayType.ID];

			return statsItems.FirstOrDefault(s => s.Route.Equals(route) && s.FromStop.Equals(stop) && s.DayType.Equals(dayType) && s.TimeRange.Equals(timeRange));
		}

		private StatisticItem EstimateStatisticItem(Route route, int stop, DayType dayType, TimeRange timeRange)
		{
			// verify if we already have the stats for this dayType in the cache
			if (dayType.Equals(_currentDayType))
			{
				if (_statsCache.ContainsKey(route.ID))
				{
					var dayTypeStats = _statsCache[route.ID].GetByDayType(_currentDayType);

					if (dayTypeStats != null)
					{
						var statisticItem = dayTypeStats.GetByStopAndTimeRange(stop, timeRange);

						// if we already have the stat for this case, 
						// we don't have to do anything else, just return that value
						if (statisticItem != null) return statisticItem;
						
						// we don't have stat for this case, so we will try to estimate it
						
						// count how many time ranges there are
						var timeRangeCount = 1;
						var currentTimeRange = timeRange;
						while (!currentTimeRange.NextTimeRange().Equals(timeRange))
						{
							timeRangeCount++;

							currentTimeRange = currentTimeRange.NextTimeRange();
						}
						
						const int maxNeighbourDistance = 4;

						// try to estimate using first neighbour stops for the same timeRange,
						// if we can't estimate with the same timeRange, step to +/- 1 timeRange and search again
						// if we can't estimate with the same timeRange, step to +/- 2 timeRange and search again
						// ...
						// so on until +/- [timeRangeCount/2] timeRange
						// otherwise, we have to step to different day types...
						for (var timeRangeIndex = 0; timeRangeIndex < timeRangeCount/2; timeRangeIndex++)
						{
							var prevTimeRange = timeRange.PreviousTimeRange(timeRangeIndex);
							var nextTimeRange = timeRange.NextTimeRange(timeRangeIndex);


							double? prevTimeRangeTimePerMeter = null;
							double? nextTimeRangeTimePerMeter = null;
							
							// try to use neighbour stats (based on stop index, for the prevTimeRange of the same day type)
							for (var i = 0; i < maxNeighbourDistance; i++)
							{
								double? prevTimePerMeter = null;
								double? nextTimePerMeter = null;

								if (stop - i >= 0)
								{
									var prevStatisticItem = dayTypeStats.GetByStopAndTimeRange(stop - i, prevTimeRange);
									if (prevStatisticItem != null)
									{
										prevTimePerMeter = prevStatisticItem.TimeToNextStop / RouteComponent.Instance.GetDistanceBetweenStops(route, stop - i, stop - i + 1);
									}
								}

								if (stop + i < route.Stops.Count - 1)
								{
									var nextStatisticItem = dayTypeStats.GetByStopAndTimeRange(stop + i, prevTimeRange);
									if (nextStatisticItem != null)
									{
										nextTimePerMeter = nextStatisticItem.TimeToNextStop / RouteComponent.Instance.GetDistanceBetweenStops(route, stop + i, stop + i + 1);
									}
								}

								if (prevTimePerMeter != null || nextTimePerMeter != null)
								{
									prevTimeRangeTimePerMeter = ((prevTimePerMeter.HasValue ? prevTimePerMeter.Value : nextTimePerMeter.Value) + (nextTimePerMeter.HasValue ? nextTimePerMeter.Value : prevTimePerMeter.Value)) / 2.0;
									break;
								}
							}


							// try to use neighbour stats (based on stop index, for the nextTimeRange of the same day type)
							for (var i = 0; i < maxNeighbourDistance; i++)
							{
								double? prevTimePerMeter = null;
								double? nextTimePerMeter = null;

								if (stop - i >= 0)
								{
									var prevStatisticItem = dayTypeStats.GetByStopAndTimeRange(stop - i, nextTimeRange);
									if (prevStatisticItem != null)
									{
										prevTimePerMeter = prevStatisticItem.TimeToNextStop / RouteComponent.Instance.GetDistanceBetweenStops(route, stop - i, stop - i + 1);
									}
								}

								if (stop + i < route.Stops.Count - 1)
								{
									var nextStatisticItem = dayTypeStats.GetByStopAndTimeRange(stop + i, nextTimeRange);
									if (nextStatisticItem != null)
									{
										nextTimePerMeter = nextStatisticItem.TimeToNextStop / RouteComponent.Instance.GetDistanceBetweenStops(route, stop + i, stop + i + 1);
									}
								}

								if (prevTimePerMeter != null || nextTimePerMeter != null)
								{
									nextTimeRangeTimePerMeter = ((prevTimePerMeter.HasValue ? prevTimePerMeter.Value : nextTimePerMeter.Value) + (nextTimePerMeter.HasValue ? nextTimePerMeter.Value : prevTimePerMeter.Value)) / 2.0;
									break;
								}
							}


							if (prevTimeRangeTimePerMeter != null || nextTimeRangeTimePerMeter != null)
							{
								var timeRangeTimePerMeter = ((prevTimeRangeTimePerMeter.HasValue ? prevTimeRangeTimePerMeter.Value : nextTimeRangeTimePerMeter.Value) + (nextTimeRangeTimePerMeter.HasValue ? nextTimeRangeTimePerMeter.Value : prevTimeRangeTimePerMeter.Value)) / 2.0;

								var distanceToNextStop = RouteComponent.Instance.GetDistanceBetweenStops(route, stop, stop + 1);

								return new StatisticItem
								{
									Calculation = null,
									Route = route,
									DayType = dayType,
									TimeRange = timeRange,
									FromStop = stop,
									SampleCount = 0,
									TimeToNextStop = Convert.ToInt32(distanceToNextStop * timeRangeTimePerMeter),
									Speed = 1.0 / timeRangeTimePerMeter
								};
							}
						}

						// TODO: try to do the same with fetching the other day types

						return null;
					}

					return null;
				}

				return null;
			}

			return null;
		}

		public List<StatisticItem> GetByDayType(DayType dayType)
		{
			var lastCalulation = GetLastCalculation();

			if (lastCalulation == null) return new List<StatisticItem>();

			return  GetByDayType(lastCalulation, dayType);
		}

		public List<StatisticItem> GetByDayType(StatisticCalculation calculation, DayType dayType)
		{
			var result = ServiceLocator.Current.GetInstance<IStatisticItemPersistence>().GetByCalculationAndDayType(calculation, dayType);

			// get the complete route from the cache (mapping from DB does not have Stops)
			foreach (var statisticItem in result)
			{
				statisticItem.Route = RouteComponent.Instance.GetById(statisticItem.Route.ID);
			}

			return result;
		}

		public List<StatisticItem> GetByDayType(StatisticCalculation calculation)
		{
			var result = ServiceLocator.Current.GetInstance<IStatisticItemPersistence>().GetByCalculation(calculation);

			// get the complete route from the cache (mapping from DB does not have Stops)
			foreach (var statisticItem in result)
			{
				statisticItem.Route = RouteComponent.Instance.GetById(statisticItem.Route.ID);
			}

			return result;
		}
		
		public StatisticCalculation GetLastCalculation()
		{
			return ServiceLocator.Current.GetInstance<IStatisticCalculationPersistence>().GetLast();
		}

		public StatisticCalculation CalculateStatistics()
		{
			var samples = new Dictionary<int, RouteStatsSamples>(RouteComponent.Instance.GetAll().Count);

			var lastCalculation = GetLastCalculation();
			
			var fromDate = new DateTime(2000, 1, 1); // the beginning, we are assuming that there is no record before year 2000
			var toDate = DateTimeHelper.Today(); // and we take complete days, so we want records before today

			if(lastCalculation != null)
			{
				fromDate = lastCalculation.StartDate.Date;

				var lastCalculationDetails = GetByDayType(lastCalculation);

				LoadSamplesFromCalculationDetails(samples, lastCalculationDetails);
			}
			
			foreach (var route in RouteComponent.Instance.GetAll())
			{
				if(!samples.ContainsKey(route.ID)) samples.Add(route.ID, new RouteStatsSamples(route));

				CalculateSamplesByRoute(samples[route.ID], route, fromDate, toDate);
			}

			var statsItems = TotalizeStatsItems(samples);

			return ServiceLocator.Current.GetInstance<IStatisticCalculationPersistence>().BulkSave(statsItems, toDate);
		}
		
		#endregion Methods
		
		#region Private Methods

		private void Initialize()
		{
			_statsCache = new Dictionary<int, RouteStats>(RouteComponent.Instance.GetAll().Count);

			ReloadCache();

			// TODO: remove this, just to run ParamTest (db timeout issue, so we get all the stats to memory)
			_statsCachePerDay = new Dictionary<int, List<StatisticItem>>(3);
			_statsCachePerDay.Add(DayType.LaborDay.ID, GetByDayType(DayType.LaborDay));
			_statsCachePerDay.Add(DayType.Saturday.ID, GetByDayType(DayType.Saturday));
			_statsCachePerDay.Add(DayType.SundayOrHoliday.ID, GetByDayType(DayType.SundayOrHoliday));
		}

		private void ReloadCache()
		{
			var dayTpe = GetDayType(DateTimeHelper.Today());

			var statisticItems = GetByDayType(dayTpe);

			_statsCache.Clear();

			foreach (var statisticItem in statisticItems)
			{
				AddCacheItem(statisticItem);
			}

			// TODO: fill statistics for empty stops

			_currentDayType = dayTpe;
		}

		private StatisticItem AddCacheItem(StatisticItem statistic)
		{
			if (!_statsCache.ContainsKey(statistic.Route.ID))
			{
				_statsCache.Add(statistic.Route.ID, new RouteStats(statistic.Route));
			}

			_statsCache[statistic.Route.ID].Add(statistic);

			return statistic;
		}

		private void VerifyCacheDayTime()
		{
			var dayType = GetDayType(DateTimeHelper.Today());

			if (!dayType.Equals(_currentDayType)) ReloadCache();
		}
		
		public static DayType GetDayType(DateTime date)
		{
			if(HolidayScheduleComponent.Instance.IsHoliday(date)) return DayType.SundayOrHoliday;
			
			if(date.DayOfWeek == DayOfWeek.Sunday) return DayType.SundayOrHoliday;

			if(date.DayOfWeek == DayOfWeek.Saturday) return DayType.Saturday;

			return DayType.LaborDay;
		}

		public static TimeRange GetTimeRange(DateTime dateTime)
		{
			var timeRanges = new[] { TimeRange.ZeroToFourAm, TimeRange.FourAmToEightAm, TimeRange.EightAmToTwelvePm, TimeRange.TwelvePmToFourPm, TimeRange.FourPmToEightPm, TimeRange.EightPmToZero };
			
			return timeRanges[dateTime.TimeOfDay.Hours / (24 / timeRanges.Count())];
		}

		private static RouteStatsSamples CalculateSamplesByRoute(RouteStatsSamples prevStats, Route route, DateTime fromDate, DateTime toDate)
		{
			var result = prevStats;

			var pendingsByRoute = CheckpointComponent.Instance.GetByRoute(route, fromDate, toDate);

			var uuids = pendingsByRoute.Select(c => c.UUID).Distinct();

			foreach (var uuid in uuids)
			{
				var uuid1 = uuid;

				var pendingsByRouteAndUUID = pendingsByRoute.Where(c => c.UUID == uuid1).OrderBy(c => c.DateTime).ToList();

				// start from i=1 because we always need two neighour points
				var lastReferencePoint = 0;

				// TODO: remove the NextStop calculation if the checkpoint already have this value in the DB
				// now is doing this because at the beginning the sample checkpoint didn't have the nextStop field
				if (pendingsByRouteAndUUID[lastReferencePoint].NextStopIndex == 0)
					pendingsByRouteAndUUID[lastReferencePoint].NextStopIndex = RouteComponent.Instance.GetNextStop(route, pendingsByRouteAndUUID[lastReferencePoint].Coordinate);
				
				for (var i = 1; i < pendingsByRouteAndUUID.Count(); i++)
				{
					// TODO: remove the NextStop calculation if the checkpoint already have this value in the DB
					// now is doing this because at the beginning the sample checkpoint didn't have the nextStop field
					if (pendingsByRouteAndUUID[i].NextStopIndex == 0)
						pendingsByRouteAndUUID[i].NextStopIndex = RouteComponent.Instance.GetNextStop(route, pendingsByRouteAndUUID[i].Coordinate);

					var timeFromLastReference = pendingsByRouteAndUUID[i].DateTime.Subtract(pendingsByRouteAndUUID[lastReferencePoint].DateTime);

					// verify if we step to another sample set
					if (timeFromLastReference.TotalMinutes > 30 ||
						pendingsByRouteAndUUID[i].NextStopIndex < pendingsByRouteAndUUID[lastReferencePoint].NextStopIndex)
					{
						// set this point as the first reference point
						lastReferencePoint = i;
						continue;
					}

					if (pendingsByRouteAndUUID[i].NextStopIndex == pendingsByRouteAndUUID[lastReferencePoint].NextStopIndex)
					{
						continue;
					}

					// at this point we have a checkpoint with a greater stop number from the one in the lastReferencePoint
					// and the time elapsed between them is not that big, so it seems we are in the same trip
					var fromStop = pendingsByRouteAndUUID[lastReferencePoint].NextStopIndex - 1;
					var toStop = pendingsByRouteAndUUID[i].NextStopIndex - 1;
					var distanceBetweenPoints = RouteComponent.Instance.GetDistanceBetweenPoints(route, pendingsByRouteAndUUID[lastReferencePoint].Coordinate, pendingsByRouteAndUUID[i].Coordinate);

					for (var j = fromStop; j < toStop; j++)
					{
						var distanceBetweenStops = RouteComponent.Instance.GetDistanceBetweenStops(route, j, j + 1);

						var statisticItem = new StatisticItem
						{
							Route = route,
							DayType = GetDayType(pendingsByRouteAndUUID[lastReferencePoint].DateTime),
							TimeRange = GetTimeRange(pendingsByRouteAndUUID[lastReferencePoint].DateTime),
							FromStop = j,
							TimeToNextStop = Convert.ToInt32(Math.Round(distanceBetweenStops * Convert.ToDouble(timeFromLastReference.TotalSeconds) / distanceBetweenPoints)),
							Speed = distanceBetweenStops / Convert.ToDouble(timeFromLastReference.TotalSeconds),
							SampleCount = 1
						};

						// add statistic
						result.Add(statisticItem);
					}

					// set this point as last reference point
					lastReferencePoint = i;
				}
			}

			// TODO: calculate average timeToNextStop, speed, etc. (plain multiple values of statisticItem for each DayType+TimeRange+FromStop)

			return result;
		}

		private static List<StatisticItem> TotalizeStatsItems(Dictionary<int, RouteStatsSamples> samples)
		{
			var result = new List<StatisticItem>();

			foreach (var route in samples.Keys)
			{
				foreach (var dayType in samples[route].DayTypes)
				{
					var dayTypeStatsCalculation = samples[route].GetByDayType(dayType);
					if (dayTypeStatsCalculation != null)
					{
						foreach (var timeRange in dayTypeStatsCalculation.TimeRanges)
						{
							var timeRangeStatsCalculation = dayTypeStatsCalculation.GetByTimeRange(timeRange);
							if (timeRangeStatsCalculation != null)
							{
								for (var stop = 0; stop < timeRangeStatsCalculation.Route.Stops.Count; stop++)
								{
									// assuming the first one is the previous stat value
									var statsSamples = timeRangeStatsCalculation.GetByStop(stop);
									if (statsSamples != null)
									{
										var statsItem = new StatisticItem
										{
											DayType = dayType,
											TimeRange = timeRange,
											Route = timeRangeStatsCalculation.Route,
											FromStop = stop,
											TimeToNextStop = statsSamples.Sum(s => s.TimeToNextStop) / statsSamples.Count,
											Speed = statsSamples.Sum(s => s.Speed) / statsSamples.Count,
											SampleCount = statsSamples.Count
										};

										result.Add(statsItem);
									}
								}
							}
						}
					}
				}
			}

			return result;
		}

		private static void LoadSamplesFromCalculationDetails(Dictionary<int, RouteStatsSamples> samples, IEnumerable<StatisticItem> lastCalculationDetails)
		{
			foreach (var lastCalculationDetail in lastCalculationDetails)
			{
				if (!samples.ContainsKey(lastCalculationDetail.Route.ID))
					samples.Add(lastCalculationDetail.Route.ID, new RouteStatsSamples(lastCalculationDetail.Route));

				samples[lastCalculationDetail.Route.ID].Add(lastCalculationDetail);
			}
		}

		#endregion Private Methods
	}

	#region Internal Classes

	#region Stats Cache

	internal class RouteStats
	{
		private readonly Dictionary<int, DayTypeStats> _dayTypeStats;

		public RouteStats(Route route)
		{
			Route = route;

			_dayTypeStats = new Dictionary<int, DayTypeStats>(6);
		}

		public Route Route { get; private set; }

		public StatisticItem Add(StatisticItem statistic)
		{
			if (!statistic.Route.Equals(Route)) throw new Exception("Statistic Route does not match.");

			if (!_dayTypeStats.ContainsKey(statistic.DayType.ID))
			{
				_dayTypeStats.Add(statistic.DayType.ID, new DayTypeStats(statistic.DayType));
			}

			_dayTypeStats[statistic.DayType.ID].Add(statistic);

			return statistic;
		}

		public DayTypeStats GetByDayType(DayType dayType)
		{
			if (_dayTypeStats.ContainsKey(dayType.ID)) return _dayTypeStats[dayType.ID];

			return null;
		}

		public List<int> DayTypeIds { get { return _dayTypeStats.Keys.ToList(); } }
	}

	internal class DayTypeStats
	{
		private readonly Dictionary<int, TimeRangeStats> _timeRangeStats;

		public DayTypeStats(DayType dayType)
		{
			DayType = dayType;

			_timeRangeStats = new Dictionary<int, TimeRangeStats>(6);
		}

		public DayType DayType { get; private set; }

		public StatisticItem Add(StatisticItem statistic)
		{
			if (!statistic.DayType.Equals(DayType)) throw new Exception("Statistic DayType does not match.");

			if (!_timeRangeStats.ContainsKey(statistic.TimeRange.ID))
			{
				_timeRangeStats.Add(statistic.TimeRange.ID, new TimeRangeStats(statistic.TimeRange, statistic.Route));
			}

			_timeRangeStats[statistic.TimeRange.ID].Add(statistic);

			return statistic;
		}

		public TimeRangeStats GetByTimeRange(TimeRange timeRange)
		{
			if (_timeRangeStats.ContainsKey(timeRange.ID)) return _timeRangeStats[timeRange.ID];

			return null;
		}

		public List<int> TimeRangeIds { get { return _timeRangeStats.Keys.ToList(); } }

		public StatisticItem GetByStopAndTimeRange(int stop, TimeRange timeRange)
		{
			if (_timeRangeStats.ContainsKey(timeRange.ID))
			{
				

				return _timeRangeStats[timeRange.ID].GetByStop(stop);
			}

			return null;
		}
	}

	internal class TimeRangeStats
	{
		private readonly StatisticItem[] _statisticItems;

		public TimeRangeStats(TimeRange timeRange, Route route)
		{
			Route = route;

			TimeRange = timeRange;

			_statisticItems = new StatisticItem[route.Stops.Count];
		}

		#region Properties

		public Route Route { get; private set; }

		public TimeRange TimeRange { get; private set; }

		#endregion Properties

		public StatisticItem Add(StatisticItem statistic)
		{
			if (!Route.Equals(statistic.Route)) throw new Exception("Statistic Route does not match TimeRange Route.");

			if (!TimeRange.Equals(statistic.TimeRange)) throw new Exception("Statistic TimeRange does not match TimeRange.");

			if (statistic.FromStop < 0 || statistic.FromStop >= Route.Stops.Count) throw new Exception("Invalid statistic FromStop for statistic Route.");

			_statisticItems[statistic.FromStop] = statistic;

			return statistic;
		}
		
		public StatisticItem GetByStop(int stop)
		{
			if(stop >= 0 && stop < _statisticItems.Length) 
				return _statisticItems[stop];

			return null;
		}

		public List<StatisticItem> GetAll()
		{
			return _statisticItems.Where(s => s != null).ToList();
		}
	}

	#endregion Stats Cache
	
	#region Stats Calculation

	internal class RouteStatsSamples
	{
		private readonly Dictionary<int, DayTypeStatsSamples> _dayTypeRangeStats;

		public RouteStatsSamples(Route route)
		{
			Route = route;

			_dayTypeRangeStats = new Dictionary<int, DayTypeStatsSamples>(6);
		}

		public Route Route { get; private set; }

		public StatisticItem Add(StatisticItem statistic)
		{
			if (!statistic.Route.Equals(Route)) throw new Exception("Statistic Route does not match.");

			if (!_dayTypeRangeStats.ContainsKey(statistic.DayType.ID))
			{
				_dayTypeRangeStats.Add(statistic.DayType.ID, new DayTypeStatsSamples(statistic.DayType));
			}

			_dayTypeRangeStats[statistic.DayType.ID].Add(statistic);

			return statistic;
		}

		public DayTypeStatsSamples GetByDayType(DayType dayType)
		{
			if (_dayTypeRangeStats.ContainsKey(dayType.ID)) return _dayTypeRangeStats[dayType.ID];

			return null;
		}

		public List<int> DayTypeIds { get { return _dayTypeRangeStats.Keys.ToList(); } }

		public List<DayType> DayTypes { get { return _dayTypeRangeStats.Values.Select(d => d.DayType).ToList(); } }
	}

	internal class DayTypeStatsSamples
	{
		private readonly Dictionary<int, TimeRangeStatsSamples> _timeRangeStats;

		public DayTypeStatsSamples(DayType dayType)
		{
			DayType = dayType;

			_timeRangeStats = new Dictionary<int, TimeRangeStatsSamples>(6);
		}

		public DayType DayType { get; private set; }

		public StatisticItem Add(StatisticItem statistic)
		{
			if (!statistic.DayType.Equals(DayType)) throw new Exception("Statistic DayType does not match.");

			if (!_timeRangeStats.ContainsKey(statistic.TimeRange.ID))
			{
				_timeRangeStats.Add(statistic.TimeRange.ID, new TimeRangeStatsSamples(statistic.TimeRange, statistic.Route));
			}

			_timeRangeStats[statistic.TimeRange.ID].Add(statistic);

			return statistic;
		}

		public TimeRangeStatsSamples GetByTimeRange(TimeRange timeRange)
		{
			if (_timeRangeStats.ContainsKey(timeRange.ID)) return _timeRangeStats[timeRange.ID];

			return null;
		}

		public List<int> TimeRangeIds { get { return _timeRangeStats.Keys.ToList(); } }

		public List<TimeRange> TimeRanges { get { return _timeRangeStats.Values.Select(t => t.TimeRange).ToList(); } }

		public StatisticItem GetByStopAndTimeRange(int stop, TimeRange timeRange)
		{
			if (_timeRangeStats.ContainsKey(timeRange.ID))
			{
				_timeRangeStats[timeRange.ID].GetByStop(stop);
			}

			return null;
		}
	}

	internal class TimeRangeStatsSamples
	{
		private readonly List<StatisticItem>[] _statisticItems;

		public TimeRangeStatsSamples(TimeRange timeRange, Route route)
		{
			Route = route;

			TimeRange = timeRange;

			_statisticItems = new List<StatisticItem>[route.Stops.Count];
		}

		#region Properties

		public Route Route { get; private set; }

		public TimeRange TimeRange { get; private set; }

		#endregion Properties

		public StatisticItem Add(StatisticItem statistic)
		{
			if (!Route.Equals(statistic.Route)) throw new Exception("Statistic Route does not match TimeRange Route.");

			if (!TimeRange.Equals(statistic.TimeRange)) throw new Exception("Statistic TimeRange does not match TimeRange.");

			if (statistic.FromStop < 0 || statistic.FromStop >= Route.Stops.Count) throw new Exception("Invalid statistic FromStop for statistic Route.");

			if (_statisticItems[statistic.FromStop] == null)
				_statisticItems[statistic.FromStop] = new List<StatisticItem>();

			_statisticItems[statistic.FromStop].Add(statistic);

			return statistic;
		}

		public List<StatisticItem> GetByStop(int stop)
		{
			if (stop >= 0 && stop < _statisticItems.Length)
				return _statisticItems[stop];

			return null;
		}

		public List<List<StatisticItem>> GetAll()
		{
			return _statisticItems.Where(s => s != null).ToList();
		}
	}

	#endregion Stats Calculation

	#endregion Internal Classes
}
