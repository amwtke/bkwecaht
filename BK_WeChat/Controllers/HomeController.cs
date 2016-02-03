﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP;
using System.Web.Configuration;
using BK_WeChat.CommonService.CustomMessageHandler;
using Senparc.Weixin.MP.MvcExtension;
using BK.CommonLib.Log;
using BK.Model.Configuration;
using StackExchange.Redis;

namespace BK_WeChat.Controllers
{
    public class HomeController : Controller
    {
        static BK.Model.Configuration.WeixinConfig wexinConfig = BK.Configuration.BK_ConfigurationManager.GetConfig<WeixinConfig>();
            //BK.CommonLib.Configuration.BK_ConfigurationManager.GetConfig<BK.Model.Configuration.WeixinConfig>();
        public static readonly string Token = wexinConfig.WeixinToken;
            //WebConfigurationManager.AppSettings["WeixinToken"];//与微信公众账号后台的Token设置保持一致，区分大小写。
        public static readonly string EncodingAESKey = wexinConfig.WeixinEncodingAESKey;            //WebConfigurationManager.AppSettings["WeixinEncodingAESKey"];//与微信公众账号后台的EncodingAESKey设置保持一致，区分大小写。
        public static readonly string AppId = wexinConfig.WeixinAppId;
        //WebConfigurationManager.AppSettings["WeixinAppId"];//与微信公众账号后台的AppId设置保持一致，区分大小写。

        readonly Func<string> _getRandomFileName = () => DateTime.Now.Ticks + Guid.NewGuid().ToString("n").Substring(0, 6);

        public HomeController()
        {

        }

        /// <summary>
        /// 微信后台验证地址（使用Get），微信后台的“接口配置信息”的Url填写如：http://wechat.51science.cn/
        /// </summary>
        [HttpGet]
        [ActionName("Index")]
        public ActionResult Get(PostModel postModel, string echostr)
        {
            if (CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, Token))
            {
                LogHelper.LogInfoAsync(typeof(HomeController), "微信成功对接！SUCCEED!");
                return Content(echostr); //返回随机字符串则表示验证通过
            }
            else
            {
                LogHelper.LogInfoAsync(typeof(HomeController), "failed:肖劲");
                return Content("failed:" + postModel.Signature + "," + CheckSignature.GetSignature(postModel.Timestamp, postModel.Nonce, Token) + "。" +
                    "如果你在浏览器中看到这句话，说明此地址可以被作为微信公众账号后台的Url，请注意保持Token一致。");
            }
        }

        /// <summary>
        /// 用户发送消息后，微信平台自动Post一个请求到这里，并等待响应XML。
        /// PS：此方法为简化方法，效果与OldPost一致。
        /// v0.8之后的版本可以结合Senparc.Weixin.MP.MvcExtension扩展包，使用WeixinResult，见MiniPost方法。
        /// </summary>
        [HttpPost]
        [ActionName("Index")]
        public ActionResult Post(PostModel postModel)
        {
            if (!CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, Token))
            {
                //LogManager.LogHelper.LogInfoAsync(typeof(HomeController), "failed:" + postModel.Signature + "," + CheckSignature.GetSignature(postModel.Timestamp, postModel.Nonce, Token) + "。" +
                //"如果你在浏览器中看到这句话，说明此地址可以被作为微信公众账号后台的Url，请注意保持Token一致。");
                return Content("参数错误！");
            }

            postModel.Token = Token;
            postModel.EncodingAESKey = EncodingAESKey;//根据自己后台的设置保持一致
            postModel.AppId = AppId;//根据自己后台的设置保持一致

            //v4.2.2之后的版本，可以设置每个人上下文消息储存的最大数量，防止内存占用过多，如果该参数小于等于0，则不限制
            var maxRecordCount = 10;

            //var logPath = Server.MapPath(string.Format("~/App_Data/MP/{0}/", DateTime.Now.ToString("yyyy-MM-dd")));
            //if (!Directory.Exists(logPath))
            //{
            //    Directory.CreateDirectory(logPath);
            //}

            //自定义MessageHandler，对微信请求的详细判断操作都在这里面。
            var messageHandler = new CustomMessageHandler(Request.InputStream, postModel, maxRecordCount);
            

            try
            {
                //测试时可开启此记录，帮助跟踪数据，使用前请确保App_Data文件夹存在，且有读写权限。
                //messageHandler.RequestDocument.Save(Path.Combine(logPath, string.Format("{0}_Request_{1}.txt", _getRandomFileName(), messageHandler.RequestMessage.FromUserName)));
                if (messageHandler.UsingEcryptMessage)
                {
                    //TODO: messageHandler.EcryptRequestDocument.Save(Path.Combine(logPath, string.Format("{0}_Request_Ecrypt_{1}.txt", _getRandomFileName(), messageHandler.RequestMessage.FromUserName)));
                }

                /* 如果需要添加消息去重功能，只需打开OmitRepeatedMessage功能，SDK会自动处理。
                 * 收到重复消息通常是因为微信服务器没有及时收到响应，会持续发送2-5条不等的相同内容的RequestMessage*/
                messageHandler.OmitRepeatedMessage = true;


                //执行微信处理过程
                messageHandler.Execute();

                //测试时可开启，帮助跟踪数据

                //if (messageHandler.ResponseDocument == null)
                //{
                //    throw new Exception(messageHandler.RequestDocument.ToString());
                //}

                if (messageHandler.ResponseDocument != null)
                {
                    //TODO: messageHandler.ResponseDocument.Save(Path.Combine(logPath, string.Format("{0}_Response_{1}.txt", _getRandomFileName(), messageHandler.RequestMessage.FromUserName)));
                }

                if (messageHandler.UsingEcryptMessage)
                {
                    //记录加密后的响应信息
                    //TODO: messageHandler.FinalResponseDocument.Save(Path.Combine(logPath, string.Format("{0}_Response_Final_{1}.txt", _getRandomFileName(), messageHandler.RequestMessage.FromUserName)));
                }

                //return Content(messageHandler.ResponseDocument.ToString());//v0.7-
                return new FixWeixinBugWeixinResult(messageHandler);//为了解决官方微信5.0软件换行bug暂时添加的方法，平时用下面一个方法即可
                //return new WeixinResult(messageHandler);//v0.8+
            }
            catch (Exception ex)
            {
                //TODO: 
                //using (TextWriter tw = new StreamWriter(Server.MapPath("~/App_Data/Error_" + _getRandomFileName() + ".txt")))
                //{
                //    tw.WriteLine("ExecptionMessage:" + ex.Message);
                //    tw.WriteLine(ex.Source);
                //    tw.WriteLine(ex.StackTrace);
                //    //tw.WriteLine("InnerExecptionMessage:" + ex.InnerException.Message);

                //    if (messageHandler.ResponseDocument != null)
                //    {
                //        tw.WriteLine(messageHandler.ResponseDocument.ToString());
                //    }

                //    if (ex.InnerException != null)
                //    {
                //        tw.WriteLine("========= InnerException =========");
                //        tw.WriteLine(ex.InnerException.Message);
                //        tw.WriteLine(ex.InnerException.Source);
                //        tw.WriteLine(ex.InnerException.StackTrace);
                //    }

                //    tw.Flush();
                //    tw.Close();
            }
            return Content("");
        }

        //[HttpGet]
        [Route("{hash}/{key}/{value}")]
        public async System.Threading.Tasks.Task<ActionResult> Redis(string hash, string key, string value)
        {
            var db = BK.CommonLib.DB.Redis.RedisManager.GetRedisDB();
            bool flag = await db.HashSetAsync(hash, key, value);
            return Content("ok");
        }

        [Route("{hash}/{key}")]
        public async System.Threading.Tasks.Task<ActionResult> Redis(string hash, string key)
        {
            var db = BK.CommonLib.DB.Redis.RedisManager.GetRedisDB();
            var value = await db.HashGetAsync(hash, key);
            LogHelper.LogInfoAsync(typeof(HomeController), "redis:value=" + value);
            return Content(value.ToString());
        }
    }
}