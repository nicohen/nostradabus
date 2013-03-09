using System;

namespace Nostradabus.BusinessComponents.Exceptions
{
    /// <summary>
    /// Exception throw by flawed.
    /// </summary>
    public class InvalidCredentialsException : ApplicationException
    {
        public InvalidCredentialsException()
        {
        }

        public InvalidCredentialsException(string message) : base(message)
        {
        }

        public InvalidCredentialsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}