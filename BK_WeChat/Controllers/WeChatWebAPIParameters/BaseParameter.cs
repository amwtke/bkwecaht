using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BK.WeChat.Controllers.WeChatWebAPIParameters
{
    public class BaseParameter
    {
        public string openID { get; set; }
        public string unionID { get; set; }
        public string jsonpCallback { get; set; }
        public int pageIndex { get; set; }
        public int pageSize { get; set; }
    }
}