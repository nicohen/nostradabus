using System;
using System.Globalization;

namespace Nostradabus.Common
{
	public class DateTimeHelper
	{
		#region Properties

        protected static Configuration.ConfigurationManager configuration = Configuration.ConfigurationManager.Instance;

		protected static TimeZoneInfo baseTimeZone;
		public static TimeZoneInfo TimeZoneInfo
		{
			get
			{
				if (baseTimeZone == null)
				{
                    //baseTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
					baseTimeZone = TimeZoneInfo.Utc;
				}

				return baseTimeZone;
			}
		}

		protected static CultureInfo cultureDefault = CultureInfo.CreateSpecificCulture(configuration.Localization.DefaultCultureInfo);

		protected static string shortDatePattern = configuration.Localization.ShortDatePattern;
		protected static string shortTimePattern = configuration.Localization.ShortTimePattern;
		protected static string shortDateTimePattern = configuration.Localization.ShortDateTimePattern;
		protected static string alternativeShortDatePattern = configuration.Localization.AlternativeShortDatePattern;
		protected static string alternativeShortDateTimePattern = configuration.Localization.AlternativeShortDateTimePattern;

		#endregion Properties

		#region Methods

		public static DateTime Now()
		{
			return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo); 
		}

        public static DateTime Today()
        {
			return Now().Date;
        }

        public static DateTime EndOfToday()
        {
            DateTime today = Now();

            return new DateTime(today.Year, today.Month, today.Day, 23, 59, 59);
        }

		#region Formatting

		/// <summary>
		///Get string from a Date in ShortDate 
        /// </summary>
        /// <param name="date">The date.</param>
        public static string ToShortDateFormat(DateTime? date)
        {
            return date.HasValue ? ToShortDateFormat(date.Value) : string.Empty;
        }

		/// <summary>
		///Get string from a Date in ShortDate 
		/// </summary>
		/// <param name="date">The date.</param>
		public static string ToAlternativeShortDateFormat(DateTime? date)
		{
			return date.HasValue ? ToAlternativeShortDateFormat(date.Value) : string.Empty;
		}

        /// <summary>
        /// Get Date in format define in Localization Section.
        /// </summary>
        /// <param name="date">The date.</param>
	    public static string ToShortDateFormat(DateTime date)
        {
			return date.Date.ToString(shortDatePattern, cultureDefault);
        }

		/// <summary>
		/// Get Date in Alternative format define in Localization Section.
		/// </summary>
		/// <param name="date">The date.</param>
		public static string ToAlternativeShortDateFormat(DateTime date)
		{
			return date.Date.ToString(alternativeShortDatePattern, cultureDefault);
		}

        /// <summary>
        /// Get Hour in format define in Localization Section.
        /// </summary>
        /// <param name="date">The date.</param>
        public static string ToShortTimeFormat(DateTime? date)
        {
            return date.HasValue ? ToShortTimeFormat(date.Value) : string.Empty;
        }


        /// <summary>
        /// Get Hour in format define in Localization Section.
        /// </summary>
        /// <param name="date">The date.</param>
        public static string ToShortTimeFormat(DateTime date)
        {
			return date.ToString(shortTimePattern, cultureDefault);
        }

		/// <summary>
		/// Get Hour in format define in Localization Section.
		/// </summary>
		/// <param name="date">The date.</param>
		public static string ToShortDateTimeFormat(DateTime? date)
		{
			return date.HasValue ? ToShortDateTimeFormat(date.Value) : string.Empty;
		}


		/// <summary>
		/// Get Hour in format define in Localization Section.
		/// </summary>
		/// <param name="date">The date.</param>
		public static string ToShortDateTimeFormat(DateTime date)
		{
			return date.ToString(shortDateTimePattern, cultureDefault);
		}

		/// <summary>
		/// Get date in alternative short date time format define in Localization Section.
		/// </summary>
		/// <param name="date">The date.</param>
		/// <returns></returns>
		public static string ToAlternativeShortDateTimeFormat(DateTime? date)
		{
			return date.HasValue ? ToAlternativeShortDateTimeFormat(date.Value) : string.Empty;
		}

		/// <summary>
		/// Get date in alternative short date time format define in Localization Section.
		/// </summary>
		/// <param name="date">The date.</param>
		/// <returns></returns>
		public static string ToAlternativeShortDateTimeFormat(DateTime date)
		{
			return date.ToString(alternativeShortDateTimePattern, cultureDefault);
		}

		#endregion

		#region SetTime

		public static DateTime SetTime(string time)
		{
			return SetTime(Now(), time);
		}

		public static DateTime SetTime(TimeSpan time)
		{
			return SetTime(Now(), time);
		}

		public static DateTime SetTime(DateTime dateTime,string time)
        {
			var timeSpan = ParseShortTime(time);
			return SetTime(dateTime, timeSpan);
        }

		public static DateTime SetTime(DateTime dateTime, TimeSpan time)
		{
			return dateTime.Date.Add(time);
		}

		#endregion

		#region Parses

		/// <summary>
		/// Return Today with hour define.
		/// </summary>
		public static TimeSpan ParseShortTime(string time)
		{
			return DateTime.ParseExact(time, shortTimePattern, cultureDefault).TimeOfDay;
		}

		public static DateTime ParseShortDateTime(string date,string hour)
		{
			string datetime = date + " " + hour;
			return ParseShortDateTime(datetime);
		}

		public static DateTime ParseShortDateTime(string datetime)
		{
			return DateTime.ParseExact(datetime, shortDateTimePattern, cultureDefault);
		}

		public static DateTime ParseShortDate(string date)
		{
			return DateTime.ParseExact(date, shortDatePattern, cultureDefault);
		}

		#endregion

		#region TryParse

		public static bool TryParseShortTime(string time)
		{
			try
			{
				DateTime.ParseExact(time, shortTimePattern, cultureDefault);
			}
			catch(Exception ex)
			{
				return false;
			}

			return true;
		}

		public static bool TryParseShortDateTime(string time)
		{
			try
			{
				DateTime.ParseExact(time, shortDateTimePattern, cultureDefault);
			}
			catch (Exception ex)
			{
				return false;
			}

			return true;
		}

		public static bool TryParseShortDate(string time)
		{
			try
			{
				DateTime.ParseExact(time, shortDatePattern, cultureDefault);
			}
			catch (Exception ex)
			{
				return false;
			}

			return true;
		}

		#endregion

        /// <summary>
        /// Checks if a datetime is today (according to EST time)
        /// </summary>
        public static bool IsToday(DateTime date)
        {
            DateTime timeZoneToday = Now();

            return date.Year == timeZoneToday.Year && date.Month == timeZoneToday.Month && date.Day == timeZoneToday.Day;
        }

		#endregion Methods
	}
}
