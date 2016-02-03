
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class Message
    {
        [Key]
        public long ID { get; set; }
        public string Title { get; set; }
        public string MessageInfo { get; set; }
        public DateTime SendTime { get; set; }
        public int Status { get; set; }
        public int? MsgType { get; set; }
        public long? RelationID { get; set; }
        public string MsgNum { get; set; }
        public int? SendStatus { get; set; }
        public Guid SendUser_uuid { get; set; }
        public Guid Receiver_uuid { get; set; }
        public Guid RelationID_uuid { get; set; }
    }
}
