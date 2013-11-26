using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Nostradabus.Website.Models
{
    /// <summary>
    /// Implement Serialization and Deserealization for ViewModel.
    /// </summary>
    [DataContract]
    public abstract class SerializeViewModel
    {
        /// <summary>
        /// Serializers this instance.
        /// </summary>
        /// <returns>String which represent this Entitie Serializable</returns>
        public virtual string Serialize()
        {
            string result;
            using (var memoryStream = new MemoryStream())
            {
                var xmlSerializer = new DataContractJsonSerializer(GetType()); 
                xmlSerializer.WriteObject(memoryStream, this);
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
        public static object Deserealize(Type type, Stream stream)
        {
            var xmlSerializer = new DataContractJsonSerializer(type);
            var result = xmlSerializer.ReadObject(stream);
            return result;
        }

        /// <summary>
        /// Deserealizes the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static object Deserealize(string type, Stream stream)
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
            using (var memoryStream = new MemoryStream(array))
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
        public static object Deserealize(Type type, string stream)
        {
            object result;
            var array = Encoding.ASCII.GetBytes(stream);
            using (var memoryStream = new MemoryStream(array))
            {
                result = Deserealize(type, memoryStream);
            }

            return result;
        }
    }
}