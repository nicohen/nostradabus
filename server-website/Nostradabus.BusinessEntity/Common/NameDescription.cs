using System.Runtime.Serialization;

namespace Nostradabus.BusinessEntities.Common
{
    /// <summary>
    /// Base for Name-Description Entities.
    /// </summary>
    /// <typeparam name="TID">The type of the ID.</typeparam>
    [DataContract]
    abstract public class NameDescription<TID> : BusinessEntity<TID>, INameDescripton
    {
		public NameDescription() {}
		public NameDescription(TID id) : base(id) {}

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember(Name="name")]
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        [DataMember(Name = "description")]
        public virtual string Description { get; set; }

        public override string ToString()
        {
            return Description;
        }
    }
}
