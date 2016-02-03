using BK.Model.Configuration.Att;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Model.Configuration.ElasticSearch
{
    public class IESIndexInterface
    {
        [BKKey("RemotePort")]
        public string RemotePort { get; set; }

        [BKKey("RemoteAddress")]
        public string RemoteAddress { get; set; }
    }
}
