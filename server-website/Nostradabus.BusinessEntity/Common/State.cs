using System.Runtime.Serialization;

namespace Nostradabus.BusinessEntities.Common
{
    /// <summary>
    /// Represent a State.
    /// </summary>
    [DataContract]
    public class State : BusinessEntity<int>
    {
		public State() : base() { }
		public State(int id) : base(id) { }

        #region Properties

        [DataMember(Name = "code")]
        public virtual string Code { get; set; }

        [DataMember(Name = "name")]
        public virtual string Name { get; set; }

        [DataMember(Name = "region")]
        public virtual Region Region { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return Code;
        }

        #endregion
    }
}
