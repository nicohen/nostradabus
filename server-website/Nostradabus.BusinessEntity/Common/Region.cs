using System.Runtime.Serialization;

namespace Nostradabus.BusinessEntities.Common
{
    /// <summary>
    /// Represent a Region.
    /// </summary>
    [DataContract]
    public class Region : NameDescription<int>
    {
        public override string ToString()
        {
            return Name;
        }
    }
}
