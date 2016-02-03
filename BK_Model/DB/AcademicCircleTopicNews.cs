
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class AcademicCircleTopicNews
    {
        [Key]
        public long ID { get; set; }
        public Nullable<long> TopicID { get; set; }
        public string NewsInfo { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
