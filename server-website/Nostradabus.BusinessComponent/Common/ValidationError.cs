using System;
using Nostradabus.BusinessEntities.Interfaces;

namespace Nostradabus.BusinessComponents.Common
{
    [Serializable]
    public class ValidationError
    {
        #region Public Properties

        public string ErrorCode { get; set; }

        public string ResourceName { get; set; }

        public IBusinessEntity BusinessEntity { get; set; }

        public string Message { get; set; }

		public int? Line { get; set; }

        #endregion Public Properties

        public ValidationError(string message, IBusinessEntity businessEntity, string errorCode)
        {
            Message = message;

            ErrorCode = errorCode;

            BusinessEntity = businessEntity;
        }

		public ValidationError(string message, IBusinessEntity businessEntity)
		{
			Message = message;
			BusinessEntity = businessEntity;
		}

        public ValidationError(string errorCode, string resourceName, IBusinessEntity businessEntity)
        {
            ErrorCode = errorCode;
            ResourceName = resourceName;
            BusinessEntity = businessEntity;
        }

		public ValidationError(string message, IBusinessEntity businessEntity,int line)
		{
			Message = message;
			BusinessEntity = businessEntity;
			Line = line;
		}

        public ValidationError(string message, IBusinessEntity businessEntity, string errorCode, int line)
        {
            Message = message;

            BusinessEntity = businessEntity;

            ErrorCode = errorCode;

            Line = line;
        }
    }
}