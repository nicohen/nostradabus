using System;

namespace Nostradabus.Configuration
{
	/// <summary>
	/// Exception thrown when configuration
	/// settings cannot be accessed.
	/// </summary>
	public class ConfigurationException : ApplicationException
	{
		public ConfigurationException(string message, Exception innerException) : base(message, innerException) { }
		public ConfigurationException(string message) : base(message) { }
		public ConfigurationException() { }
	}
}
