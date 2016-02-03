using BK.Model.Configuration.Att;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Model.Configuration.ElasticSearch
{
    [BKConfig("ElasticSearch", "EKArticle")]
    public class EKESConfig : IESIndexInterface
    {
        [BKKey("IndexName")]
        public string IndexName { get; set; }

        [BKKey("NumberOfShards")]
        public string NumberOfShards { get; set; }

        [BKKey("NumberOfReplica")]
        public string NumberOfReplica { get; set; }
    }

    [BKConfig("ElasticSearch", "Papers")]
    public class PaperESConfig : IESIndexInterface
    {
        [BKKey("IndexName")]
        public string IndexName { get; set; }

        [BKKey("NumberOfShards")]
        public string NumberOfShards { get; set; }

        [BKKey("NumberOfReplica")]
        public string NumberOfReplica { get; set; }
    }
}
