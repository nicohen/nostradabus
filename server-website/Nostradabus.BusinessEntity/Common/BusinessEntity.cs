using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Nostradabus.BusinessEntities.Interfaces;

namespace Nostradabus.BusinessEntities.Common
{
    /// <summary>
    /// Class base for BusinessEntities.
    /// </summary>
    /// <typeparam name="TId">The type of the id.</typeparam>
    /// The implementation with DataContract is for purpouse of the accessibility of properties
    /// http://msdn.microsoft.com/en-us/library/ms733127.aspx
    [DataContract]
    public abstract class BusinessEntity<TId> : IBusinessEntity
	{
		#region Constructors

		public BusinessEntity() { }
		public BusinessEntity(TId id)
		{
			ID = id;
		}

		#endregion Constructors

		#region Properties

		[DataMember]
        public virtual TId ID { get; protected set; }

		#endregion Properties

		#region Methods

		public override bool Equals(object obj)
        {
            var compareTo = obj as BusinessEntity<TId>;

            return (compareTo != null) &&
                   (HasSameNonDefaultIdAs(compareTo) ||
                    // Since the IDs aren't the same, either of them must be transient to 
                    // compare business value signatures
                    (((IsTransient()) || compareTo.IsTransient()) && HasSameBusinessSignatureAs(compareTo)));
        }

        /// <summary>
        /// Must be provided to properly compare two objects
        /// </summary>
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
		
    	#endregion Methods

		#region Implementation of IBusinessEntity

		/// <summary>
        /// Transient objects are not associated with an item already in storage.  For instance,
        /// is transient if its ID is 0.
        /// </summary>
        public virtual bool IsTransient()
        {
            return ID == null || ID.Equals(default(TId));
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serializers this instance.
        /// </summary>
        /// <typeparam name="T">Type of Entity to Serializer.</typeparam>
        /// <returns>String which represent this Entitie Serializable</returns>
        public virtual string Serialize()
        {
            string result;
            using(var memoryStream = new MemoryStream())
            {
                var xmlSerializer = new DataContractJsonSerializer(GetType());
                xmlSerializer.WriteObject(memoryStream,this);
                memoryStream.Position = 0;
                using (var streamReader = new StreamReader(memoryStream))
                {
                    result = streamReader.ReadToEnd();
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the stream serialize.
        /// </summary>
        /// <returns></returns>
        public virtual Stream GetStreamSerialize()
        {
            var memoryStream = new MemoryStream();
            var xmlSerializer = new DataContractJsonSerializer(GetType());
            xmlSerializer.WriteObject(memoryStream, this);
            return memoryStream;
        }

        /// <summary>
        /// Deserealizes the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static object Deserealize(Type type,Stream stream)
        {
            DataContractJsonSerializer xmlSerializer = new DataContractJsonSerializer(type);
            var result = xmlSerializer.ReadObject(stream);
            return result; 
        }

        /// <summary>
        /// Deserealizes the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static object Deserealize(string type,Stream stream)
        {
            var typeObject = Type.GetType(type);
            return Deserealize(typeObject, stream);

        }

        /// <summary>
        /// Deserealizes the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static object Deserealize(string type, string stream)
        {
            object result;
            var array = Encoding.ASCII.GetBytes(stream);
            using(var memoryStream = new MemoryStream(array))
            {
                result = Deserealize(type, memoryStream);
            }

            return result;
        }

        /// <summary>
        /// Deserealizes the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static object Deserealize(Type type,string stream)
        {
            object result;
            var array = Encoding.ASCII.GetBytes(stream);
            using (var memoryStream = new MemoryStream(array))
            {
                result = Deserealize(type, memoryStream);
            }

            return result;
        }

        #endregion

		#region Private Methods

		private bool HasSameBusinessSignatureAs(BusinessEntity<TId> compareTo)
		{
			return GetHashCode().Equals(compareTo.GetHashCode());
		}

		/// <summary>
		/// Returns true if self and the provided persistent object have the same ID values 
		/// and the IDs are not of the default ID value
		/// </summary>
		private bool HasSameNonDefaultIdAs(BusinessEntity<TId> compareTo)
		{
			return (ID != null && !ID.Equals(default(TId))) &&
				   (compareTo.ID != null && !compareTo.ID.Equals(default(TId))) && ID.Equals(compareTo.ID);
		}

		#endregion Private Methods
	}
}