using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BK.WeChat.Controllers.WeChatWebAPIParameters
{
    public class LoginParameter: BaseParameter
    {
        public string account { get; set; }
        public string password { get; set; }
    }
}