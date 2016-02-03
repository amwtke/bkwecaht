using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BK.WeChat.Controllers.WeChatWebAPIParameters
{
    public class ComplementParameter: BaseParameter
    {
        public string university { get; set; }
        public string faculty { get; set; }
        public int researchFieldId { get; set; }
        public short degree { get; set; }
        public short enrollment { get; set; }
        public DateTime? birthday { get; set; }

        public string province { get; set; }
        public string city { get; set; }

        public string hometownProvince { get; set; }
        public string hometownCity { get; set; }

        public string name { get; set; }
        public string Gender { get; set; }

        public string position { get; set; }
        public short isBusiness { get; set; }
    }
}