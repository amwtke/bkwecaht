using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BK.WeChat.Controllers.WeChatWebAPIParameters
{
    public class LocationParameter: BaseParameter
    {
        public int? Radius { get; set; }
        public int? IsBusiness { get; set; }
        public int? Gender { get; set; }
        public int? ResearchFieldId { get; set; }
    }
}