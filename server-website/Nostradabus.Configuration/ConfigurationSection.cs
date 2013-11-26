using System;

namespace Nostradabus.Configuration
{
	/// <summary>
	/// Descripción breve de ConfigurationSection.
	/// </summary>
	public class ConfigurationSection
	{
		//public static ConfigurationSection DAL = new ConfigurationSection("/DAL");
		//public static ConfigurationSection Password = new ConfigurationSection("/DataTypes/Password");
		//public static ConfigurationSection Security = new ConfigurationSection("/Security");
		//public static ConfigurationSection Common = new ConfigurationSection("/Common");
		
		#region Variables protegidas
		protected string _Name;
		protected static string _CommonName = "Nostradabus";
		#endregion
		
		#region Constructores
		public ConfigurationSection(string name)
		{
			_Name = name;
		}
		#endregion

		public override string ToString()
		{
			if (_Name.StartsWith("/")) 
			{
				return String.Concat(_CommonName, _Name);
			}
			
			return _Name;
		}
	}
}
