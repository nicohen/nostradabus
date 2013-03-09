using System;
using Nostradabus.BusinessComponents.Common;

namespace Nostradabus.BusinessComponents.Exceptions
{
    /// <summary>
    /// Exception throw by flawed.
    /// </summary>
    public class ValidationException : ApplicationException
    {
        public ValidationException()
        {
        }

        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ValidationException(string message, ValidationSummary summary, Exception innerException)
            : base(message, innerException)
        {
            Summary = summary;
        }

        public ValidationException(ValidationSummary summary)
        {
            Summary = summary;
        }

        public ValidationException(ValidationError error)
        {
            var summary = new ValidationSummary();

            summary.Errors.Add(error);

            Summary = summary;
        }

        public ValidationSummary Summary { get; set; }

        public override string Message
        {
            get
            {
                return string.Format("{0}", (Summary != null) ? Summary.ToString() : base.Message);
            }
        }
    }
}