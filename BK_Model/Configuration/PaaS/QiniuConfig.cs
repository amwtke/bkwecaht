using BK.Model.Configuration.Att;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Model.Configuration.PaaS
{
    [BKConfig("PaaS", "Qiniu")]
    public class QiniuConfig : IConfigModel
    {
        [BKKey("ACCESS_KEY")]
        public string ACCESS_KEY { get; set; }

        [BKKey("SECRET_KEY")]
        public string SECRET_KEY { get; set; }

        [BKKey("HDPBUCKET")]
        public string HDPBUCKET { get; set; }

        [BKKey("HDPDOMAIN")]
        public string HDPDOMAIN { get; set; }

        [BKKey("ImgBUCKET")]
        public string ImgBUCKET { get; set; }

        [BKKey("ImgDOMAIN")]
        public string ImgDOMAIN { get; set; }

        [BKKey("AttBUCKET")]
        public string AttBUCKET { get; set; }

        [BKKey("AttDOMAIN")]
        public string AttDOMAIN { get; set; }

        [BKKey("EKABUCKET")]
        public string EKABUCKET { get; set; }

        [BKKey("EKADOMAIN")]
        public string EKADOMAIN { get; set; }

        public void init()
        {
        }
    }
}
