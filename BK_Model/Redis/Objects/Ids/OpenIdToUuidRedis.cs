using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Model.Redis.Objects.Ids
{
    [RedisDBNumber("0")]
    [RedisHash("openid.useruuid.hash")]
    public class OpenIdToUserUUIDHash
    {
        [RedisKey]
        public string OpenId { get; set; }

        [RedisHashEntry("UserUuid")]
        public string UserUuid { get; set; }
    }
}
