using System;
using System.Collections.Generic;
using System.Linq;

namespace Nostradabus.BusinessComponents.Common
{
    [Serializable]
    public class ValidationSummary
    {
        private readonly List<ValidationError> _errors;

        public ValidationSummary()
        {
            _errors = new List<ValidationError>();
        }

        public ValidationSummary(IEnumerable<ValidationError> errors)
        {
            _errors = new List<ValidationError>();

            _errors.AddRange(errors);
        }

        public List<ValidationError> Errors
        {
            get { return _errors; }
        }

        public bool HasErrors
        {
            get { return Errors.Count > 0; }
        }

        public string ErrorsToString()
        {
            return ErrorsToString("||");
        }

        public string ErrorsToString(string separator)
        {
            return Errors.Aggregate(string.Empty, (current, error) => current + error.ErrorCode + separator);
        }

        /// <summary>
        /// Concatenates the error messages (the message itself, not the code) using the specified separator
        /// </summary>
        public string ErrorMessagesToString(string separator)
        {
            string errors = Errors.Aggregate(string.Empty, (current, error) => current + error.Message + separator);

            if (errors.EndsWith(separator))
            {
                errors = errors.TrimEnd(separator.ToCharArray());
            }

            return errors;
        }

        public string ErrorMessagesToString()
        {
            return ErrorMessagesToString("<br />");
        }

        public override string ToString()
        {
            return ErrorMessagesToString(",");
        }
    }
}