using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Text;
using Nostradabus.BusinessEntities;
using Nostradabus.Common;
using Nostradabus.Persistence.Interfaces;
using Nostradabus.Persistence.Nhibernate.Common;
using NHibernate.Linq;

namespace Nostradabus.Persistence.Nhibernate
{
	public class StatisticCalculationPersistence : PersistenceManager<StatisticCalculation>, IStatisticCalculationPersistence
	{
		private static readonly MySqlConnection Conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
		
		public StatisticCalculation GetLast()
		{
			IQueryable<StatisticCalculation> query = from c in CurrentSession.Query<StatisticCalculation>()
											  where c.EndDate != null
											  orderby c.StartDate descending
											  select c;

			return query.Take(1).FirstOrDefault();
		}

		public StatisticCalculation BulkSave(List<StatisticItem> statItems, DateTime startDate)
		{
			var statCalculation = new StatisticCalculation { StartDate = startDate };

			statCalculation = StartCalculation(statCalculation);

			var errorCount = 0;

			const int maxItemsPerFlush = 100;
			var appendedItems = 0;

			var builder = new StringBuilder();
			
			for (var i = 0; i < statItems.Count; i++)
			{
				statItems[i].Calculation = statCalculation;

				builder.AppendLine(GenerateStatItemInsert(statItems[i]));
				
				appendedItems++;

				if (appendedItems >= maxItemsPerFlush)
				{
					try
					{
						// send queries to the DB
						ExecuteQuery(builder.ToString());
					}
					catch (Exception ex)
					{
						errorCount++;
					}
					finally
					{
						builder.Clear();
						appendedItems = 0;
					}
				}
			}
			
			// flush last appended items
			if (appendedItems > 0)
			{
				try
				{
					// send queries to the DB
					ExecuteQuery(builder.ToString());
				}
				catch (Exception ex)
				{
					errorCount++;
				}
				finally
				{
					builder.Clear();
				}
			}

			statCalculation.EndDate = DateTimeHelper.Now();
			//statCalculation.Errors = errorCount;

			statCalculation = EndCalculation(statCalculation);

			return statCalculation;
		}

		private static StatisticCalculation StartCalculation(StatisticCalculation statCalculation)
		{
			var cmd = new MySqlCommand();

			try
			{
				if (Conn.State != ConnectionState.Open) Conn.Open();

				cmd.Connection = Conn;

				cmd.CommandType = CommandType.Text;
				//cmd.CommandText = "INSERT INTO stats_calculation (start_date) VALUES (@start_date); SELECT LAST_INSERT_ID()";
				cmd.CommandText = "INSERT INTO stats_calculation (start_date) VALUES (?start_date)";
				cmd.Parameters.AddWithValue("?start_date", statCalculation.StartDate);

				cmd.ExecuteNonQuery();

				var newId = Convert.ToInt32(cmd.LastInsertedId);

				statCalculation = new StatisticCalculation(newId)
				{
					StartDate = statCalculation.StartDate
				};
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message + Environment.NewLine + cmd.CommandText);
			}
			finally
			{
				cmd.Dispose();
			}

			return statCalculation;
		}
		
		private static StatisticCalculation EndCalculation(StatisticCalculation statCalculation)
		{
			var cmd = new MySqlCommand();

			try
			{
				if (Conn.State != ConnectionState.Open) Conn.Open();

				cmd.Connection = Conn;
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "UPDATE stats_calculation SET end_date = ?end_date WHERE id = ?stats_calculation_id";
				cmd.Parameters.AddWithValue("?end_date", statCalculation.EndDate);
				cmd.Parameters.AddWithValue("?stats_calculation_id", statCalculation.ID);
				
				cmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message + Environment.NewLine + cmd.CommandText);
			}
			finally
			{
				cmd.Dispose();
			}

			return statCalculation;
		}

		private static string GenerateStatItemInsert(StatisticItem statItems)
		{
			var result = "INSERT INTO stats_detail (stats_calculation_id, route_id, day_type_id, time_range_id, from_stop, time_to_next_stop, speed, frequency, sample_count)";
			result = result + " VALUES ";

			// (@stats_calculation_id, @route_id, @day_type_id, @time_range_id, @from_stop, @time_to_next_stop, @speed, @frequency, @sample_count)	
			result = result + String.Format("({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8});", statItems.Calculation.ID, statItems.Route.ID, statItems.DayType.ID, statItems.TimeRange.ID, statItems.FromStop, statItems.TimeToNextStop, statItems.Speed.HasValue ? statItems.Speed.Value.ToString(CultureInfo.InvariantCulture).Replace(",", ".") : "0", statItems.Frequency.HasValue ? statItems.Frequency.Value.ToString(CultureInfo.InvariantCulture) : "null", statItems.SampleCount);

			return result;
		}

		private static int ExecuteQuery(string query)
		{
			var cmd = new MySqlCommand();

			int rowsAffected = 0;

			try
			{
				if (Conn.State != ConnectionState.Open) Conn.Open();
				
				cmd.CommandText = query;
				cmd.CommandType = CommandType.Text;
				cmd.Connection = Conn;

				rowsAffected = cmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message + Environment.NewLine + cmd.CommandText);
			}
			finally
			{
				cmd.Dispose();
			}

			return rowsAffected;
		}

		~StatisticCalculationPersistence()
		{
			if (Conn != null)
			{
				if (Conn.State != ConnectionState.Closed) Conn.Close();

				Conn.Dispose();
			}
		}
	}
}
