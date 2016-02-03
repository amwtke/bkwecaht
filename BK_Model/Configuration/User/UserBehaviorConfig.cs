using BK.Model.Configuration.Att;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Model.Configuration.User
{
    [BKConfig("User", "Behavior")]
    public class UserBehaviorConfig
    {
        [BKKey("LoginTimeSpanMin")]
        public string LoginTimeSpanMin { get; set; }

        [BKKey("GetMessageCount")]
        public string GetMessageCount { get; set; }
    }
}
