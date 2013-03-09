using System;
using Nostradabus.BusinessComponents.Common;

namespace Nostradabus.BusinessComponents.Exceptions
{
	/// <summary>
	/// Generic Exception for Nostradabus App.
	/// </summary>
	public class NostradabusApplicationException : ApplicationException
	{
		public NostradabusApplicationException()
        {
        }

        public NostradabusApplicationException(string message) : base(message)
        {
        }

        public NostradabusApplicationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public NostradabusApplicationException(string message, ValidationSummary summary, Exception innerException)
            : base(message, innerException)
        {
            Summary = summary;
        }

		public NostradabusApplicationException(ValidationSummary summary)
        {
            Summary = summary;
        }

        public ValidationSummary Summary { get; set; }
	}
}
