using System.Runtime.Serialization;

namespace Nostradabus.BusinessEntities.Common
{
	[DataContract]
	public class Language : BusinessEntity<int>
	{
		public Language() : base() { }
		public Language(int id) : base(id) { }

        #region Properties

		[DataMember(Name = "code")]
		public virtual string Code { get; set; }

        [DataMember(Name = "name")]
        public virtual string Name { get; set; }
		
        #endregion

        #region Methods

        public override string ToString()
        {
			return Name;
        }

        #endregion
	}
}
