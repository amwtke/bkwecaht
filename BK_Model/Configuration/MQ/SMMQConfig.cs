using BK.Model.Configuration.Att;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Model.Configuration.MQ
{
    [BKConfig("MQ", "SM")]
    public class SMMQConfig:IConfigModel,IMQConfig
    {
        [BKKey("HostName")]
        public string HostName { get; set; }

        [BKKey("Port")]
        public string Port { get; set; }

        [BKKey("UserName")]
        public string UserName { get; set; }

        [BKKey("Password")]
        public string Password { get; set; }

        [BKKey("ExchangeName")]
        public string ExchangeName { get; set; }

        [BKKey("QueueName")]
        public string QueueName { get; set; }

        [BKKey("SpermThreshold")]
        public string SpermThreshold { get; set; }
        [BKKey("NumberOfC")]
        public string NumberOfC { get; set; }

        public void init()
        {

        }
    }
}
