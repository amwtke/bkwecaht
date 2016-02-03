using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Model.MQ
{
    [Serializable]
    public class NoticeMQ
    {
        public long Id { get; set; }
        public Guid Receiver_Uuid { get; set; }
        public Guid Relation_Uuid { get; set; }
        public long Relation_Id { get; set; }
        public int status { get; set; }
        public object PayLoad { get; set; }
        public NoticeType MsgType { get; set; }
        /// <summary>
        /// Unixtime
        /// </summary>
        public DateTime TimeStamp { get; set; }
    }


    public enum NoticeType
    {
        /// <summary>
        /// 联系人请求
        /// </summary>
        Contact_Request = 100,
        /// <summary>
        /// 点赞加一
        /// </summary>
        Favorite_Add = 200,
        /// <summary>
        /// 访客踪迹
        /// </summary>
        Visitor_Add = 300,

    }
}
