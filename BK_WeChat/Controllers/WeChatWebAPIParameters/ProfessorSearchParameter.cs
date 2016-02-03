using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BK.WeChat.Controllers.WeChatWebAPIParameters
{
    public class ProfessorSearchParameter : BaseParameter
    {
        public long rfid { get; set; }
        public bool? xiaoyou { get; set; }
        public string danwei { get; set; }
        public string labels { get; set; }

        public string address { get; set; }
    }
}