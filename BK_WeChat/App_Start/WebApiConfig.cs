using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace BK_WeChat
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API 配置和服务

            // Web API 路由
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            config.Routes.MapHttpRoute(
                name: "ActionApi",
                routeTemplate: "api/{controller}/{act}/{id}",
                defaults: new { act = RouteParameter.Optional, id = RouteParameter.Optional }
            );
        }
    }
}
