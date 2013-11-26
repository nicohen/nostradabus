using System.Collections.Generic;
using System.Runtime.Serialization;
using Nostradabus.BusinessComponents.Exceptions;

namespace Nostradabus.Website.Models
{
    /// <summary>
    /// Represent a ViewModel Base class.
    /// </summary>
    [DataContract]
    public abstract class BaseViewModel
    {
    	protected BaseViewModel()
		{
			Messages = new List<string>();
			ErrorMessages = new List<string>();
		}

		[DataMember(Name = "messages")]
		public List<string> Messages { get; set; }

		[DataMember(Name = "errorMessages")]
		public List<string> ErrorMessages { get; set; }

		public void AddValidationErrors(ValidationException validationException)
		{
			if (validationException.Summary != null && validationException.Summary.HasErrors)
			{
				foreach (var error in validationException.Summary.Errors)
				{
					ErrorMessages.Add(error.Message);	
				}
			}
			else if (!string.IsNullOrEmpty(validationException.Message))
			{
				ErrorMessages.Add(validationException.Message);
			}
		}
    }
}