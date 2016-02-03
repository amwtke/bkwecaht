using BK.Model.DB;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BK.Model.Configuration.Att;
using BK.Model.Configuration;
using System.Configuration;
using BK.Model.Configuration.MQ;
using System.Reflection;
using BK.Model.Configuration.Redis;
using BK.Model.Configuration.ElasticSearch;
using BK.Model.Configuration.User;
using BK.Model.Configuration.PaaS;

namespace BK.Configuration.DB
{
    public class ConfigDBContext: DbContext
    {
        public ConfigDBContext() : base("name=BKConfigContext")
        {
            this.Configuration.LazyLoadingEnabled = false;
            Database.SetInitializer<ConfigDBContext>(new BKConfigInit<ConfigDBContext>());
        }

        public ConfigDBContext(string conString) : base(conString)
        {
            this.Configuration.LazyLoadingEnabled = false;
            Database.SetInitializer<ConfigDBContext>(new BKConfigInit<ConfigDBContext>());
        }

        public DbSet<BKConfigItem> BKConfigs { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }

    public class BKConfigInit<T>: CreateDatabaseIfNotExists<ConfigDBContext>
    {
        protected override void Seed(ConfigDBContext context)
        {
            base.Seed(context);
            string AppDomain = ConfigurationManager.AppSettings["ConfigDomain"];

            WeixinConfig weixinobject = new WeixinConfig();
            weixinobject.WeixinAppId = ConfigurationManager.AppSettings["WeixinAppId"];
            weixinobject.WeixinAppSecret = ConfigurationManager.AppSettings["WeixinAppSecret"];
            weixinobject.WeixinEncodingAESKey = ConfigurationManager.AppSettings["WeixinEncodingAESKey"];
            weixinobject.WeixinToken = ConfigurationManager.AppSettings["WeixinToken"];


            LogConfig log = new LogConfig();
            log.TraceResponse = "false";
            log.RemotePort = "9200";
            log.RemoteAddress = "10.46.74.122";

            #region weixin
            BKConfigAttribute wx_mf = typeof(WeixinConfig).GetCustomAttributes(false)[0] as BKConfigAttribute;

            List<BKConfigItem> weixinItems = new List<BKConfigItem>();
            weixinItems.Add(new BKConfigItem { Domain = AppDomain, Module = wx_mf.Module, Function = wx_mf.Function, Key = "WeixinAppId", Value = weixinobject.WeixinAppId, TimeStamp = DateTime.Now });

            weixinItems.Add(new BKConfigItem { Domain = AppDomain, Module = wx_mf.Module, Function = wx_mf.Function, Key = "WeixinAppSecret", Value = weixinobject.WeixinAppSecret, TimeStamp = DateTime.Now });

            weixinItems.Add(new BKConfigItem { Domain = AppDomain, Module = wx_mf.Module, Function = wx_mf.Function, Key = "WeixinEncodingAESKey", Value = weixinobject.WeixinEncodingAESKey, TimeStamp = DateTime.Now });

            weixinItems.Add(new BKConfigItem { Domain = AppDomain, Module = wx_mf.Module, Function = wx_mf.Function, Key = "WeixinToken", Value = weixinobject.WeixinToken, TimeStamp = DateTime.Now });
            weixinItems.ForEach(a => context.BKConfigs.Add(a));
            #endregion


            #region log
            BKConfigAttribute log_mf = typeof(LogConfig).GetCustomAttributes(false)[0] as BKConfigAttribute;
            List<BKConfigItem> logItems = new List<BKConfigItem>();

            logItems.Add(new BKConfigItem { Domain = AppDomain, Module = log_mf.Module, Function = log_mf.Function, Key = "TraceResponse", Value = log.TraceResponse, TimeStamp = DateTime.Now });

            logItems.Add(new BKConfigItem { Domain = AppDomain, Module = log_mf.Module, Function = log_mf.Function, Key = "RemotePort", Value = log.RemotePort, TimeStamp = DateTime.Now });

            logItems.Add(new BKConfigItem { Domain = AppDomain, Module = log_mf.Module, Function = log_mf.Function, Key = "RemoteAddress", Value = log.RemoteAddress, TimeStamp = DateTime.Now });
            logItems.ForEach(b => context.BKConfigs.Add(b));



            #endregion


            #region MQ

            WeChatMessageMQConfig wcMQ = new WeChatMessageMQConfig();
            wcMQ.ExchangeName = "WeChatMessage_Exchange";
            wcMQ.QueueName = "WeChatMessage_queue";
            wcMQ.HostName = "10.46.74.122";
            wcMQ.Port = "5672";
            wcMQ.Password = "123456";
            wcMQ.UserName = "xj";
            wcMQ.SpermThreshold = "5000";
            wcMQ.NumberOfC = "1";
            DBinitialHelper.MakeList(wcMQ, AppDomain).ForEach(c => context.BKConfigs.Add(c));

            EmailMQConfig emailMQ = new EmailMQConfig();
            emailMQ.ExchangeName = "Email_Exchange";
            emailMQ.QueueName = "Email_queue";
            emailMQ.HostName = "10.46.74.122";
            emailMQ.Port = "5672";
            emailMQ.Password = "123456";
            emailMQ.UserName = "xj";
            emailMQ.SpermThreshold = "5000";
            emailMQ.NumberOfC = "1";
            DBinitialHelper.MakeList(emailMQ, AppDomain).ForEach(c => context.BKConfigs.Add(c));

            SMMQConfig smmq = new SMMQConfig();
            smmq.ExchangeName = "sm_Exchange";
            smmq.QueueName = "sm_queue";
            smmq.HostName = "10.46.74.122";
            smmq.Port = "5672";
            smmq.Password = "123456";
            smmq.UserName = "xj";
            smmq.SpermThreshold = "5000";
            smmq.NumberOfC = "1";
            DBinitialHelper.MakeList(smmq, AppDomain).ForEach(c => context.BKConfigs.Add(c));

            LogMQConfig logmq = new LogMQConfig();
            logmq.ExchangeName = "log_Exchange";
            logmq.QueueName = "log_queue";
            logmq.HostName = "10.46.74.122";
            logmq.Port = "5672";
            logmq.Password = "123456";
            logmq.UserName = "xj";
            logmq.SpermThreshold = "5000";
            logmq.NumberOfC = "1";
            DBinitialHelper.MakeList(logmq, AppDomain).ForEach(c => context.BKConfigs.Add(c));


            TestMQ testmq = new TestMQ();
            testmq.ExchangeName = "xj";
            testmq.QueueName = "task_queue";
            testmq.HostName = "10.46.74.122";
            testmq.Port = "5672";
            testmq.Password = "123456";
            testmq.UserName = "xj";
            testmq.SpermThreshold = "5000";
            testmq.NumberOfC = "3";
            DBinitialHelper.MakeList(testmq, AppDomain).ForEach(c => context.BKConfigs.Add(c));

            WeChatRedisConfig wcRedisconfig = new WeChatRedisConfig();
            wcRedisconfig.MasterHostAndPort = "10.46.74.122:6379";
            wcRedisconfig.Password = "123456";
            wcRedisconfig.SlaveHostsAndPorts = "10.46.74.122:6380,";
            wcRedisconfig.StringSeperator = ",";
            DBinitialHelper.MakeList(wcRedisconfig, AppDomain).ForEach(c => context.BKConfigs.Add(c));

            NoticeMQConfig noticeMQ = new NoticeMQConfig();
            noticeMQ.ExchangeName = "Notice_Exchange";
            noticeMQ.QueueName = "Notice_queue";
            noticeMQ.HostName = "10.46.74.122";
            noticeMQ.Port = "5672";
            noticeMQ.Password = "123456";
            noticeMQ.UserName = "xj";
            noticeMQ.SpermThreshold = "5000";
            noticeMQ.NumberOfC = "1";
            DBinitialHelper.MakeList(noticeMQ, AppDomain).ForEach(c => context.BKConfigs.Add(c));
            #endregion

            #region index

            LocationConfig location = new LocationConfig();
            location.IndexName = "userlocation";
            location.NumberOfReplica = "1";
            location.NumberOfShards = "2";
            location.RemoteAddress = "10.46.74.122";
            location.RemotePort = "9200";
            DBinitialHelper.MakeList(location, AppDomain).ForEach(c => context.BKConfigs.Add(c));

            ComplexLocationConfig complexLocation = new ComplexLocationConfig();
            location.IndexName = "complexlocation";
            location.NumberOfReplica = "1";
            location.NumberOfShards = "2";
            location.RemoteAddress = "10.46.74.122";
            location.RemotePort = "9200";
            DBinitialHelper.MakeList(complexLocation, AppDomain).ForEach(c => context.BKConfigs.Add(c));
            #endregion

            #region userbehavior

            UserBehaviorConfig userBehaviorConfig = new UserBehaviorConfig();
            userBehaviorConfig.LoginTimeSpanMin = "10";
            userBehaviorConfig.GetMessageCount = "10";
            DBinitialHelper.MakeList(userBehaviorConfig, AppDomain).ForEach(c => context.BKConfigs.Add(c));

            #endregion

            #region PaaS

            QiniuConfig qiniu = new QiniuConfig();
            qiniu.ACCESS_KEY = "r3_0PzYByEpjj1nFIFkmo1wIqHGuRzdgckeBplji";
            qiniu.SECRET_KEY = "YKWYVUm-DqE4xFWfgLbWWke5hvsbEe5G-iw-QJfL";
            qiniu.ImgBUCKET = "eksns-img";
            qiniu.ImgDOMAIN = "img.51science.cn";
            qiniu.AttBUCKET = "eksns-att";
            qiniu.AttDOMAIN = "7xme9o.dl1.z0.glb.clouddn.com";
            qiniu.HDPBUCKET = "bk-headpic";
            qiniu.HDPDOMAIN = "headpic.51science.cn";
            qiniu.EKABUCKET = "bk-ektodaypic";
            qiniu.EKADOMAIN = "wechatektodaypic.51science.cn";
            DBinitialHelper.MakeList(qiniu, AppDomain).ForEach(c => context.BKConfigs.Add(c));
            #endregion
            context.SaveChanges();
        }
    }

    internal static class DBinitialHelper
    {
        public static List<BKConfigItem> MakeList(Object o, string domain)
        {
            if(o == null || o.GetType().GetCustomAttributes(typeof(BKConfigAttribute), false).Length != 1)
                return null;

            List<BKConfigItem> _list = new List<BKConfigItem>();

            BKConfigAttribute bkatt = o.GetType().GetCustomAttributes(typeof(BKConfigAttribute), false)[0] as BKConfigAttribute;
            string modulName = bkatt.Module;
            string funcName = bkatt.Function;

            o.GetType().GetProperties().ToList().ForEach(delegate (PropertyInfo property) {
                BKKeyAttribute key = property.GetCustomAttribute(typeof(BKKeyAttribute)) as BKKeyAttribute;
                string keyStr = key.Key;
                if(o.GetType().GetProperty(keyStr) == null)
                {
                    return;
                }
                string value = o.GetType().GetProperty(keyStr).GetValue(o).ToString();
                _list.Add(new BKConfigItem { Domain = domain, Module = modulName, Function = funcName, Key = keyStr, Value = value, TimeStamp = DateTime.Now });
            });
            return _list;
        }
    }
}
