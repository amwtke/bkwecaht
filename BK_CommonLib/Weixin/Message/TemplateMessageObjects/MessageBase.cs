using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.CommonLib.Weixin.Message
{
    public class MessageBase
    {
        public MessageBase(string v, string c)
        {
            value = v; color = c;
        }
        public string value { get; set; }
        public string color { get; set; }
    }
    public class TemplateBase
    {
        public TemplateBase(MessageBase f, MessageBase r)
        {
            first = f;
            remark = r;
        }
        public MessageBase first { get; set; }
        public MessageBase remark { get; set; }
    }
}
