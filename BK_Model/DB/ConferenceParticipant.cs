
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class ConferenceParticipant
    {
        [Key]
        public long ID { get; set; }
        public Nullable<long> ConferenceID { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
