
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class LeaveAMessage
    {
        [Key]
        public long ID { get; set; }
        public string AccountEmail { get; set; }
        public string UserName { get; set; }
        public string BodyText { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
    }
}
