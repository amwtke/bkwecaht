using BK.CommonLib.DB.Redis.Objects;
using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.Log;
using BK.WeChat.Controllers.WeChatWebAPIParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace BK.WeChat.Controllers.Base
{
    public class BaseFilter : ActionFilterAttribute
    {
        protected string getActionName(HttpActionContext context)
        {
            return context.ActionDescriptor.ActionName;
        }
        protected string getActionName(HttpActionExecutedContext context)
        {
            return context.ActionContext.ActionDescriptor.ActionName;
        }


        protected Dictionary<string,object> getActionArguments(HttpActionContext context)
        {
            return context.ActionArguments;
        }

        protected Dictionary<string, object> getActionArguments(HttpActionExecutedContext context)
        {
            return context.ActionContext.ActionArguments;
        }

        protected List<string> getActionArgumentsKeys(HttpActionContext context)
        {
            return getActionArguments(context).Keys.ToList();
        }
        protected List<string> getActionArgumentsKeys(HttpActionExecutedContext context)
        {
            return getActionArguments(context).Keys.ToList();
        }

        protected List<BaseParameter> getActionArgumentsValues(HttpActionContext context)
        {
            List<BaseParameter> _ret = new List<BaseParameter>();
            foreach (var v in getActionArguments(context).Values)
            {
                if(v is BaseParameter)
                {
                    _ret.Add(v as BaseParameter);
                }
            }
            return _ret;
        }

        protected async Task<Guid> getUUidByOpenIdAsync(string openId)
        {
            using (UserRepository r = new UserRepository())
            {
                return await r.GetUserUuidByOpenid(openId);
            }
        }

        protected string getOpenId(HttpActionContext context)
        {
            List<BaseParameter> _ret = new List<BaseParameter>();
            foreach (var v in getActionArguments(context).Values)
            {
                if (v is BaseParameter)
                {
                    _ret.Add(v as BaseParameter);
                }
            }
            string openid = _ret[0].openID;
            return openid;
        }

        protected List<BaseParameter> getActionArgumentsValues(HttpActionExecutedContext context)
        {
            List<BaseParameter> _ret = new List<BaseParameter>();
            foreach (var v in getActionArguments(context).Values)
            {
                if (v is BaseParameter)
                {
                    _ret.Add(v as BaseParameter);
                }
            }
            return _ret;
        }

        protected string getOpenId(HttpActionExecutedContext context)
        {
            List<BaseParameter> _ret = new List<BaseParameter>();
            foreach (var v in getActionArguments(context).Values)
            {
                if (v is BaseParameter)
                {
                    _ret.Add(v as BaseParameter);
                }
            }
            string openid = _ret[0].openID;
            return openid;
        }
    }

    public class UserBehaviorFilter : ActionFilterAttribute
    {
        static void Logger(Exception ex)
        {
            LogHelper.LogErrorAsync(typeof(UserBehaviorFilter), ex);
        }

        public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            try
            {
                foreach (object arg in actionContext.ActionArguments.Values)
                {
                    if (arg is BaseParameter)
                    {
                        var p = arg as BaseParameter;
                        if (!string.IsNullOrEmpty(p.openID))
                        {
                            UserLoginBehaviorOp.AddUpdateLastLoginTime(p.openID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Exception exx = new Exception(actionContext.ActionDescriptor.ActionName+"出错了！"+ex.ToString());
                Logger(exx);
            }

            return base.OnActionExecutingAsync(actionContext, cancellationToken);
        }
    }
}