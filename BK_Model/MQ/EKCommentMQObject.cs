using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Model.MQ
{
    [Serializable]
    public class EKCommentMQObject
    {
        public string uuid { get; set; }
        public string From { get; set; }
        public long To { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }

    [Serializable]
    public class PCommentMQObject
    {
        public string uuid { get; set; }
        public string From { get; set; }
        public long To { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
