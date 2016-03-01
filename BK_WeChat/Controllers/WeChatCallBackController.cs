using BK.CommonLib.DB.Redis;
using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.Log;
using BK.CommonLib.Weixin.User;
using BK.Model.Configuration;
using BK.WeChat.BizHelper;
using BK.WeChat.Controllers.Base;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BK.WeChat.Controllers
{
    [RoutePrefix("wxcallback")]
    [Route("{action=index}")]
    public class WeChatCallBackController : MVCNeedWeixinCallBackBaseController
    {
        // GET: WeChatCallBack
        public async System.Threading.Tasks.Task<ActionResult> Index(string code, string state)
        {
            return await LoginCallBack(code, state, @"F2E/wechat/xuesheng_findds.html", @"F2E/wechat/dynamic_list.html");
        }
    }
}
