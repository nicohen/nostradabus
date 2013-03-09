using System;
using System.Runtime.Serialization;
using Nostradabus.BusinessEntities.Common;

namespace Nostradabus.BusinessEntities
{
    /// <summary>
    /// Represent a Holiday.
    /// </summary>
    [DataContract]
    public class HolidaySchedule : BusinessEntity<int>
    {
        [DataMember(Name = "date")]
        public virtual DateTime Date { get; set; }

        [DataMember(Name = "description")]
        public virtual string Description { get; set; }

        [DataMember(Name = "active")]
        public virtual bool Active { get; set; }
    }
}
