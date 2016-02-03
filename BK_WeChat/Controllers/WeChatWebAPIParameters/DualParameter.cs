using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BK.WeChat.Controllers.WeChatWebAPIParameters
{
    public class DualParameter: BaseParameter
    {
        public Guid uuid { get; set; }
        public string textMsg { get; set; }
        public long itemId { get; set; }
    }
}