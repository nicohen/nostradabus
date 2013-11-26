using System;

namespace Nostradabus.Configuration
{
	/// <summary>
	/// Exception thrown when configuration
	/// settings cannot be accessed.
	/// </summary>
	public class ConfigurationNotFoundException : ConfigurationException
	{
		public ConfigurationNotFoundException(string message, Exception innerException) : base(message, innerException) { }
		public ConfigurationNotFoundException(string message) : base(message) { }
		public ConfigurationNotFoundException() { }
	}
}
