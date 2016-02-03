using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.ElasticSearch;
using Nest;
using System.Web;
using System.Reflection;
using BK.Model.Index;
using BK.Model.Configuration;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace BK.CommonLib.Log
{
    delegate void LogInfoHandler(Type t, object message);
    delegate void LogErrorHandler(Type t, Exception ex);
    delegate void LogBizHandler(BizObject bo);

    public static class LogHelper
    {
        private static bool isConfigured = false;

        private const string BIZ_INDEX_NAME = "biz";
        private const string LOG_INDEX_NAME = "log";
        private static IndexSettings _bizSettings = new IndexSettings()
        {
            NumberOfReplicas = 1,
            NumberOfShards = 2,
        };
        static LogHelper()
        {
            //System.Reflection.Assembly.LoadFrom("log4stash.dll");
            //System.Reflection.Assembly.LoadFrom("log4net.dll");
            //System.Reflection.Assembly.LoadFrom("Newtonsoft.Json.dll");
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            //foreach (Assembly a in loadedAssemblies)
            //{
            //    Console.WriteLine(a.ToString());
            //}

            if (!CheckMapping<BizObject>(BIZ_INDEX_NAME) )//|| !CheckMapping<LogEvent>(LOG_INDEX_NAME)) //{ }
                throw new Exception("索引获取或者更新失败！");
        }
        public static bool CheckMapping<T>(string indexName) where T : class
        {
            IGetMappingResponse mapping = ConnectionManager.RemoteClient.GetMapping<T>();
            if (mapping != null && (mapping.Mappings == null || mapping.Mappings.Count != 0)) return true;
            var response = ConnectionManager.RemoteClient.CreateIndex(
                c => c.Index(indexName).InitializeUsing(_bizSettings).AddMapping<T>(m => m.MapFromAttributes()));

            //var response = ConnectionManager.RemoteClient.Map<T>(x => x.Index(indexName).InitializeUsing(_bizSettings));
            return response.Acknowledged;
        }
        //private static bool prepBizObjectIndex()
        //{
        //    try
        //    {
        //        IGetMappingResponse mapping = ConnectionManager.RemoteClient.GetMapping<BizObject>();
        //        if (mapping != null && (mapping.Mappings == null || mapping.Mappings.Count != 0)) return true;
        //        //var response = ConnectionManager.RemoteClient.CreateIndex(
        //        //    c => c.Index(BIZ_INDEX_NAME).InitializeUsing(_bizSettings).AddMapping<BizObject>(m => m.MapFromAttributes()));

        //        var response = ConnectionManager.RemoteClient.Map<BizObject>(x => x.Index(BIZ_INDEX_NAME));
        //        return response.Acknowledged;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        //private static bool prepLogIndex()
        //{
        //    try
        //    {
        //        IGetMappingResponse mapping = ConnectionManager.RemoteClient.GetMapping<BK.Model.Index.LogEvent>();
        //        if (mapping != null && (mapping.Mappings == null || mapping.Mappings.Count != 0)) return true;

        //        var response = ConnectionManager.RemoteClient.Map<BK.Model.Index.LogEvent>(x => x.Index(LOG_INDEX_NAME));
        //        return response.Acknowledged;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        static void checkConfiguration()
        {
            string path = System.Configuration.ConfigurationManager.AppSettings["Log4netConfigPath"];
            if (!isConfigured)
            {
                try
                {
                    //log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(HttpContext.Current.Server.MapPath(@"~\log4net.config")));
                    log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(HttpContext.Current.Server.MapPath(path)));
                    isConfigured = true;
                }
                catch
                {
                    try
                    {
                        log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(System.Environment.CurrentDirectory + path));
                        isConfigured = true;
                    }
                    catch
                    {
                        log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(path));
                        isConfigured = true;
                    }
                }
            }
        }

        #region 同步

        public static void WriteError(Type t, Exception ex)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(t);
            log.Error(ex.ToString(), ex);
        }

        public static void WriteLogInfo(Type t, object message)
        {
            //Console.WriteLine("log info rprocess:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            log4net.ILog log = log4net.LogManager.GetLogger(t);
            log.Info(message);
        }

        public static void WriteBizLog(BizObject bo)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(typeof(BizObject));
            log.Info(bo);
        }
        #endregion

        #region 异步
        public static void LogInfoAsync(Type t, object message)
        {
            checkConfiguration();
            LogInfoHandler handler = new LogInfoHandler(WriteLogInfo);
            handler.BeginInvoke(t, message, LogInfoCallBack, handler);
        }

        public static void LogBizAsync(HttpRequest request, BizObject bo)
        {
            if (isRobot(request))
                return;
            if (isFakeLogin(request))
                bo.Message = "fake_login";
            checkConfiguration();
            LogBizHandler handler = new LogBizHandler(WriteBizLog);
            handler.BeginInvoke(bo, LogBizCallBack, handler);
        }

        public static void LogErrorAsync(Type t, Exception ex)
        {
            checkConfiguration();
            LogErrorHandler handler = new LogErrorHandler(WriteError);
            handler.BeginInvoke(t, ex, LogErrCallBack, handler);
        }
        #endregion

        #region callbacks
        private static void LogInfoCallBack(IAsyncResult ar)
        {
            //Console.WriteLine("log info call back:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            if (ar == null)
                throw new Exception("LogInfoCallBack fails,because ar is null!");

            LogInfoHandler handler = ar.AsyncState as LogInfoHandler;
            handler.EndInvoke(ar);
        }

        private static void LogBizCallBack(IAsyncResult ar)
        {
            //Console.WriteLine("log info call back:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            if (ar == null)
                throw new Exception("LogBizCallback fails,because ar is null!");

            LogBizHandler handler = ar.AsyncState as LogBizHandler;
            handler.EndInvoke(ar);
        }

        private static void LogErrCallBack(IAsyncResult ar)
        {
            //Console.WriteLine("log err call back:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            if (ar == null)
                throw new Exception("LogErrCallBack fails,because ar is null!");

            LogErrorHandler handler = ar.AsyncState as LogErrorHandler;
            handler.EndInvoke(ar);
        }
        #endregion

        private static Boolean isRobot(HttpRequest r)
        {
            if (r == null) return false;
            if (string.IsNullOrEmpty(r.UserAgent))
                return false;


            string strUserAgent = r.UserAgent;
            return strUserAgent.ToLower().Contains("baiduspider") || strUserAgent.ToLower().Contains("googlebot") ||
                   (strUserAgent.ToLower().Contains("sogou") && strUserAgent.ToLower().Contains("spider")) ||
                   strUserAgent.ToLower().Contains("yahoo! slurp") || strUserAgent.ToLower().Contains("iaskspider") ||
                   strUserAgent.ToLower().Contains("yodaobot") || strUserAgent.ToLower().Contains("yodaobot") ||
                   strUserAgent.ToLower().Contains("360spider") || strUserAgent.ToLower().Contains("haosouspider")
                   || strUserAgent.ToLower().Contains("bingbot") || strUserAgent.ToLower().Contains("AhrefsBot");
        }
        private static bool isFakeLogin(HttpRequest request)
        {
            if (request == null) return false;
            string qs = request.QueryString["isLog"];
            if (string.IsNullOrEmpty(qs))
                return false;
            return qs.Equals("false");
        }
    }

    public static class ConnectionManager
    {
        static ElasticClient _client;
        static Object _syncObject = new object();

        public static ElasticClient RemoteClient
        {
            get
            {
                if (_client == null)
                    init();
                return _client;
            }
        }


        static ConnectionManager()
        {
            init();
        }

        private static void

        init()
        {
            if (_client != null) return;
            lock (_syncObject)
            {
                if (_client != null) return;
                try
                {
                    string Address = BK.Configuration.BK_ConfigurationManager.GetConfig<LogConfig>().RemoteAddress;
                    //BK.CommonLib.Configuration.BK_ConfigurationManager.Log.RemoteAddress;
                    string Port = BK.Configuration.BK_ConfigurationManager.GetConfig<LogConfig>().RemotePort;
                    //BK.CommonLib.Configuration.BK_ConfigurationManager.GetConfig<BK.Model.Configuration.LogConfig>().RemotePort;
                    //BK.CommonLib.Configuration.BK_ConfigurationManager.Log.RemotePort;

                    var node = new Uri(@"http://" + Address + ":" + Port);

                    var settings = new ConnectionSettings(
                        node
                        );

                    _client = new ElasticClient(settings);
                }
                catch (Exception ex)
                {
                    LogHelper.LogErrorAsync(typeof(LogHelper), ex);
                }
            }
        }
    }
}
