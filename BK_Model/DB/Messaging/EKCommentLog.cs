using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Model.DB.Messaging
{
    [Table("ek_commentlog")]
    public class EKCommentLog
    {
        /// <summary>
        /// 消息的UUId
        /// </summary>
        [Key]
        [Column("uuid")]
        public Guid Uuid { get; set; }
        public Guid from { get; set; }
        public long to { get; set; }
        public string content { get; set; }
        /// <summary>
        /// Unixtime
        /// </summary>
        public DateTime timestamp { get; set; }
    }

    [Table("paper_commentlog")]
    public class PaperCommentLog
    {
        /// <summary>
        /// 消息的UUId
        /// </summary>
        [Key]
        [Column("uuid")]
        public Guid Uuid { get; set; }
        public Guid from { get; set; }
        public long to { get; set; }
        public string content { get; set; }
        /// <summary>
        /// Unixtime
        /// </summary>
        public DateTime timestamp { get; set; }
    }
}
