using System;

namespace Nostradabus.BusinessComponents.Exceptions
{
	/// <summary>
	/// Exception for configuration error.
	/// </summary>
	public class NostradabusConfigurationException : NostradabusApplicationException
	{
		public NostradabusConfigurationException()
        {
        }

        public NostradabusConfigurationException(string message) : base(message)
        {
        }

        public NostradabusConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
	}
}
