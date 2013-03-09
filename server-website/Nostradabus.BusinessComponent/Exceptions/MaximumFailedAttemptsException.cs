using System;

namespace Nostradabus.BusinessComponents.Exceptions
{
    /// <summary>
    /// Exception throw by flawed.
    /// </summary>
    public class MaximumFailedAttemptsException : ApplicationException
    {
        public MaximumFailedAttemptsException()
        {
        }

        public MaximumFailedAttemptsException(string message) : base(message)
        {
        }

        public MaximumFailedAttemptsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}