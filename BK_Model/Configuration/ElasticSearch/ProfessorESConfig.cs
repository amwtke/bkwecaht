using BK.Model.Configuration.Att;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Model.Configuration.ElasticSearch
{
    [BKConfig("ElasticSearch", "Professor")]
    public class ProfessorESConfig : IESIndexInterface
    {
        [BKKey("IndexName")]
        public string IndexName { get; set; }

        [BKKey("NumberOfShards")]
        public string NumberOfShards { get; set; }

        [BKKey("NumberOfReplica")]
        public string NumberOfReplica { get; set; }
    }
}
