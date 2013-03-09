using System;

namespace Nostradabus.BusinessComponents.Exceptions
{
    /// <summary>
    /// Exception throw by flawed.
    /// </summary>
    public class NonActiveUserException : ApplicationException
    {
        public NonActiveUserException()
        {
        }

        public NonActiveUserException(string message) : base(message)
        {
        }

		//public NonActiveUserException(string message, User user) : base(message)
		//{
		//    User = user;
		//}

		//public NonActiveUserException(string message, Exception innerException) : base(message, innerException)
		//{
		//}

		//public NonActiveUserException(string message, Exception innerException, User user) : this(message, innerException)
		//{
		//    User = user;
		//}

		//public User User { get; set; }
    }
}