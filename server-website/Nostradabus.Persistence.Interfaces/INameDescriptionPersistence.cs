using System.Collections.Generic;
using Nostradabus.Persistence.Interfaces.Common;

namespace Nostradabus.Persistence.Interfaces
{
    /// <summary>
    /// Interfaces of Persistence for Code Description Entities. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface INameDescriptionPersistence<T> : IPersistence<T>
    {
        /// <summary>
        /// Gets a Code Description Entity by Name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        T GetByName(string name);

        IList<T> GetAllowedStatus(IList<T> status);
    }
}
