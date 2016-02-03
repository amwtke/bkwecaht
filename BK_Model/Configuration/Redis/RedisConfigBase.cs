using BK.Model.Configuration.Att;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Model.Configuration.Redis
{
    public class RedisConfigBase
    {
        [BKKey("MasterHostAndPort")]
        public string MasterHostAndPort { get; set; }

        [BKKey("SlaveHostsAndPorts")]
        public string SlaveHostsAndPorts { get; set; }

        [BKKey("Password")]
        public string Password { get; set; }

        [BKKey("StringSeperator")]
        public string StringSeperator { get; set; }
    }
}
