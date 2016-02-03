using BK.Model.Configuration.Att;
using BK.Model.Configuration.ElasticSearch;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Model.Configuration
{
    [BKConfig("Log","Default")]
    public class LogConfig : IESIndexInterface
    {
        [BKKey("TraceResponse")]
        public string TraceResponse { get; set; }
        //[BKKey("RemotePort")]
        //public string RemotePort { get; set; }
        //[BKKey("RemoteAddress")]
        //public string RemoteAddress { get; set; }
    }

    [BKConfig("ElasticSearch", "Log")]
    public class LogESConfig : IESIndexInterface
    {
        [BKKey("IndexName")]
        public string IndexName { get; set; }

        [BKKey("NumberOfShards")]
        public string NumberOfShards { get; set; }

        [BKKey("NumberOfReplica")]
        public string NumberOfReplica { get; set; }
    }
}
