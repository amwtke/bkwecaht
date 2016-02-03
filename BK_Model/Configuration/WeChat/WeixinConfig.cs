using BK.Model.Configuration.Att;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Model.Configuration
{
    [BKConfig("WeChat", "Default")]
    public class WeixinConfig : IConfigModel
    {
        [BKKey("WeixinToken")]
        public string WeixinToken { get; set; }

        [BKKey("WeixinEncodingAESKey")]
        public string WeixinEncodingAESKey { get; set; }

        [BKKey("WeixinAppId")]
        public string WeixinAppId { get; set; }

        [BKKey("WeixinAppSecret")]
        public string WeixinAppSecret { get; set; }

        public void init()
        {
        }
    }
}
