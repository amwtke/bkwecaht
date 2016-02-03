
using BK.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BK.WeChat.Controllers.WeChatWebAPIParameters
{
    public class EKCommentParameter : BaseParameter
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public int SectionNo { get; set; }
        public Guid uuid { get; set; }//别人的uuid
        public UserArticle userArticle { get; set; }
    }
}