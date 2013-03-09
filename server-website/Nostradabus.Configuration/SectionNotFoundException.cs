using System;

namespace Nostradabus.Configuration
{
	/// <summary>
	/// Exception thrown when configuration
	/// section cannot be accessed.
	/// </summary>
	public class SectionNotFoundException : ConfigurationException
	{
		public SectionNotFoundException(string message, Exception innerException) : base(message, innerException) { }
		public SectionNotFoundException(string message) : base(message) { }
		public SectionNotFoundException() { }
	}
}