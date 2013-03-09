using System;

namespace Nostradabus.BusinessComponents.Exceptions
{
    public class PendingChangeTemporalPassException : ApplicationException
    {
        public PendingChangeTemporalPassException()
        {
        }

        public PendingChangeTemporalPassException(string message) : base(message)
        {
        }

		//public PendingChangeTemporalPassException(string message, User user) : base(message)
		//{
		//    User = user;
		//}

		//public PendingChangeTemporalPassException(string message, Exception innerException) : base(message, innerException)
		//{
		//}

		//public PendingChangeTemporalPassException(string message, Exception innerException, User user)
		//    : this(message, innerException)
		//{
		//    User = user;
		//}

		//public User User { get; set; }
    }
}
