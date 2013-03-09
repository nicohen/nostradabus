using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Globalization;

namespace Nostradabus.Configuration
{
	/// <summary>
	/// Proxy of System.Configuration.ConfigurationSettings.
	/// Provide a mechanism to read application settings.
	/// </summary>
	public class ConfigurationManager
	{
		#region Singleton Implementation
		private static readonly ConfigurationManager _Instance;

		/// <summary>
		/// Static Constructor of the class.
		/// Creates a new instance of ConfigurationManager,
		/// the unique instance of the Singleton.
		/// </summary>
		static ConfigurationManager()
		{
			_Instance = new ConfigurationManager
			{
			    Common = new CommonConfiguration(),
			    Localization = new LocalizationConfiguration(),
				EmailSettings = new EmailConfiguration(),
			    SchedulerSettings = new SchedulerConfiguration()
			};
		}

		/// <summary>
		/// Reference to the unique ConfigurationManager Instance.
		/// </summary>
		public static ConfigurationManager Instance
		{
			get { return _Instance; }
		}
		#endregion Singleton Implementation

		#region GetConfig()
		/// <summary>
		/// Returns the values cofigured in the section by the user.
		/// </summary>
		/// <param name="sectionName">Clave de la sección a leer.</param>
		/// <returns>Valores de configuración de sectionName.</returns>
		public object GetConfig(string sectionName)
		{
			return System.Configuration.ConfigurationManager.GetSection(sectionName);
		}
		#endregion

		#region this[]
		/// <summary>
		/// Provides a simple way to access to the app configuration
		/// </summary>
		public NameValueCollection this[string sectionName]
		{
			get
			{
				try
				{
					object section = this.GetConfig(sectionName);

					if (section == null)
					{
						throw new SectionNotFoundException();
					}

					return (NameValueCollection)section;
				}
				catch (SectionNotFoundException)
				{
					throw;
				}
				catch (InvalidCastException exception)
				{
					Exception e = exception;
					return null;
				}
				catch (Exception exception)
				{
					Exception e = exception;
					throw;
				}
			}
		}

		public NameValueCollection this[ConfigurationSection section]
		{
			get
			{
				return this[section.ToString()];
			}
		}
		#endregion

		#region AppSettings
		/// <summary>
		/// Gets the values of the app configuration.
		/// </summary>
		public NameValueCollection AppSettings
		{
			get
			{
				return ConfigurationSettings.AppSettings;
			}
		}

		public T GetAppSetting<T>(string key, T defaultValue) where T : IConvertible
		{
			string value = AppSettings[key];

			return string.IsNullOrEmpty(value) ? defaultValue : (T)Convert.ChangeType(AppSettings[key], typeof(T));
		}

		#endregion

		#region Methods

		public string GetBusinessEntityId(string businessEntity, string code)
		{
			string section = String.Concat("Nostradabus/BusinessEntitiesConstants/", businessEntity);

			return this[section][code];
		}

		public string GetCommonSetting(string settingId)
		{
			return GetSectionValue("CommonSettings", settingId);
		}

		public string GetLocalizationSetting(string settingId)
		{
			return GetSectionValue("LocalizationFormats", settingId);
		}
		
		public string GetEmailSetting(string settingId)
		{
			return GetSectionValue("EmailSettings", settingId);
		}

		public string GetSchedulerSetting(string settingId)
		{
			return GetSectionValue("Scheduler", settingId);
		}

		#endregion Methods

		#region Properties

		public CommonConfiguration Common { get; set; }

		public LocalizationConfiguration Localization { get; set; }
		
		public EmailConfiguration EmailSettings { get; set; }
		
		public SchedulerConfiguration SchedulerSettings { get; set; }
		
		#endregion Properties

		#region Internal Classes

		public class CommonConfiguration
		{
			#region Properties
			
			public bool SslEnabled
			{
				get
				{
					return bool.Parse(_Instance.GetCommonSetting("UseSSL"));
				}
			}
			
			/// <summary>
			/// Indicates the emails to send error notifications for JJK comunication.
			/// </summary>
			public string BackendServiceUsername
			{
				get
				{
					return _Instance.GetCommonSetting("BackendServiceUsername");
				}
			}
			
			#endregion
		}

		public class LocalizationConfiguration
		{
			public CultureInfo defaultCultureInfo = null;

			public string ShortDatePattern
			{
				get
				{
					return _Instance.GetLocalizationSetting("ShortDatePattern");
				}
			}

			public string ShortTimePattern
			{
				get
				{
					return _Instance.GetLocalizationSetting("ShortTimePattern");
				}
			}

			public string ShortDateTimePattern
			{
				get
				{
					return _Instance.GetLocalizationSetting("ShortDateTimePattern");
				}
			}

			public string ShortDateTimePatternImport
			{
				get
				{
					return _Instance.GetLocalizationSetting("ShortDateTimePatternImport");
				}
			}

			public string AlternativeShortDateTimePatternImport
			{
				get
				{
					return _Instance.GetLocalizationSetting("AlternativeShortDateTimePatternImport");
				}
			}

			public string ShortDatePatternImport
			{
				get
				{
					return _Instance.GetLocalizationSetting("ShortDatePatternImport");
				}
			}

			public string ShortTimePatternImport
			{
				get
				{
					return _Instance.GetLocalizationSetting("ShortTimePatternImport");
				}
			}

			public string AlternativeShortDatePatternImport
			{
				get
				{
					return _Instance.GetLocalizationSetting("AlternativeShortDatePatternImport");
				}
			}

			public string AlternativeShortDatePattern
			{
				get
				{
					return _Instance.GetLocalizationSetting("AlternativeShortDatePattern");
				}
			}

			public string AlternativeShortDateTimePattern
			{
				get { return _Instance.GetLocalizationSetting("AlternativeShortDateTimePattern"); }
			}

			public string DefaultCultureInfo
			{
				get
				{
					return _Instance.GetLocalizationSetting("DefaultCultureInfo");
				}
			}

			public string SenchaDecimalFormat
			{
				get
				{
					return _Instance.GetLocalizationSetting("SenchaDecimalFormat");
				}
			}

			public CultureInfo GetDefaultCutureInfo
			{
				get { return defaultCultureInfo ?? (defaultCultureInfo = CultureInfo.CreateSpecificCulture(this.DefaultCultureInfo)); }
			}
		}

		public class EmailConfiguration
		{
			#region Properties

			public bool Enabled
			{
				get
				{
					return bool.Parse(_Instance.GetEmailSetting("EmailEnabled"));
				}
			}

			public string SiteEmailAddress
			{
				get
				{
					return _Instance.GetEmailSetting("FromEmailAddress");
				}
			}

			#endregion
		}
		
		/// <summary>
		/// Container class for the Scheduler configuration
		/// </summary>
		public class SchedulerConfiguration
		{
			/// <summary>
			/// If this is set to false in the config file the timer should not start
			/// </summary>
			public bool Enabled
			{
				get { return bool.Parse(_Instance.GetSchedulerSetting("Enabled")); }
			}

			/// <summary>
			/// Interval of the timer in milliseconds
			/// </summary>
			public long Interval { get { return long.Parse(_Instance.GetSchedulerSetting("Interval")); } }

			/// <summary>
			/// Time of the day when the UserCleanUpTsak has to run
			/// </summary>
			public TimeSpan UserCleanUpTime
			{
				get
				{
					CultureInfo cultureDefault = CultureInfo.CreateSpecificCulture(_Instance.Localization.DefaultCultureInfo);
					return DateTime.ParseExact(_Instance.GetSchedulerSetting("UserCleanUpTime"), _Instance.Localization.ShortTimePattern, cultureDefault).TimeOfDay;
				}
			}
		}

		#endregion Internal Classes

		#region Private Methods

		private string GetSectionValue(string section, string settingId)
		{
			string sectionNode = String.Concat("Nostradabus/Configurations/", section);

			return this[sectionNode][settingId];
		}

		#endregion Private Methods
	}
}
