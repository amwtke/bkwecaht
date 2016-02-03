using BK.CommonLib.Log;
using BK.WeChat.Controllers.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;
using System.Threading;
using System.Threading.Tasks;
using BK.CommonLib.DB.Redis.Objects;
using BK.WeChat.Controllers.WeChatWebAPIParameters;
using BK.CommonLib.DB.Repositorys;
using BK.Model.DB;
using BK.CommonLib.DB.Redis;
using BK.Model.Configuration.Redis;
using BK.Model.Redis.Objects.UserBehavior;

namespace BK.WeChat.Controllers.Filters
{
    public class NameCardWebApiFilter : BaseFilter
    {
        static void Logger(Exception ex)
        {
            LogHelper.LogErrorAsync(typeof(NameCardWebApiFilter), ex);
        }

        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            //加入访问次数
            foreach (var v in getActionArgumentsValues(actionExecutedContext))
            {
                if (v is DualParameter)
                {
                    var u = v as DualParameter;
                    NameCardAccessCountOP.AddScore(u.uuid.ToString(), 1);
                    using (UserRepository repo = new UserRepository())
                    {
                        UserInfo user = repo.GetUserInfoByUuid_TB(u.uuid);
                        if(user!=null)
                        {
                            if(user.IsBusiness!= null)
                            {
                                if (user.IsBusiness == 0)
                                    new RedisManager2<WeChatRedisConfig>().AddScoreAsync<NameCardRedis, NameCardPCountZsetAttribute>(u.uuid.ToString(), 1);
                                else if(user.IsBusiness==2)
                                    new RedisManager2<WeChatRedisConfig>().AddScoreAsync<NameCardRedis, NameCardSCountZsetAttribute>(u.uuid.ToString(), 1);
                            }

                        }
                    }
                    break;
                }
            }
            return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }
    }
}