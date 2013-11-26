using Nostradabus.BusinessEntities.Interfaces;

namespace Nostradabus.BusinessEntities.Common
{
    /// <summary>
    /// Interfaces base for Code Description Entities.
    /// </summary>
    public interface INameDescripton : IBusinessEntity 
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        string Description { get; set; }
    }
}
