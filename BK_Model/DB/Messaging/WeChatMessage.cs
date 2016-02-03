using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Model.DB.Messaging
{
    [Table("ChatLog")]
    public class WeChatMessageMSSQL
    {
        /// <summary>
        /// 消息的UUId
        /// </summary>
        [Key]
        [Column("uuid")]
        public Guid Uuid { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Message { get; set; }
        public string SessionId { get; set; }
        /// <summary>
        /// Unixtime
        /// </summary>
        public DateTime TimeStamp { get; set; }
    }
}
