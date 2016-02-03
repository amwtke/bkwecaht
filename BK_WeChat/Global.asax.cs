using BK.CommonLib.DB.Redis;
using BK.CommonLib.MQ;
using BK.Model.Configuration.Redis;
using BK.WeChat.Controllers.WeChatWebAPIControllers.Find;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BK_WeChat
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_End()
        {
            RedisManager.Close();
            RedisManager2<WeChatRedisConfig>.CloseAll();
            MQManager.CloseAll();
            FindHelper.Close();
        }
    }
}
