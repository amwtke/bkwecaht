using BK.CommonLib.Log;
using BK.Configuration;
using BK.Model.Configuration;
using BK.Model.Index;
using BK.Model.MQ;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.CommonLib.ElasticSearch
{
    public static class LogESManager
    {
        static void LogError(Exception ex)
        {
            LogHelper.LogErrorAsync(typeof(PaperManager), ex);
        }
        static ElasticClient _client = null;
        static LogESConfig _config = null;
        static LogESManager()
        {
            _client = ESHeper.GetClient<LogESConfig>();
            if (_client == null)
            {
                var err = new Exception("_client没有正确初始化！");
                LogError(err);
                throw err;
            }

            _config = BK_ConfigurationManager.GetConfig<LogESConfig>();
            if (_config == null)
            {
                var err = new Exception("配置没有正确初始化！");
                LogError(err);
                throw err;
            }

            init();
        }

        static void init()
        {
            if (!ESHeper.BeSureMapping<LogEvent>(_client, _config.IndexName, new IndexSettings()
            {
                NumberOfReplicas = Convert.ToInt32(_config.NumberOfReplica),
                NumberOfShards = Convert.ToInt32(_config.NumberOfShards),
            }))
            {
                var err = new Exception("Mapping没有正确初始化！");
                LogError(err);
                throw err;
            }

            if (!ESHeper.BeSureMapping<BizIndex>(_client, _config.IndexName, new IndexSettings()
            {
                NumberOfReplicas = Convert.ToInt32(_config.NumberOfReplica),
                NumberOfShards = Convert.ToInt32(_config.NumberOfShards),
            }))
            {
                var err = new Exception("Mapping没有正确初始化！");
                LogError(err);
                throw err;
            }
        }

        public static async Task<bool> AddOrUpdateLogEventAsync(LogEvent obj)
        {
            try
            {
                var result = await _client.SearchAsync<LogEvent>(s => s.Query(q => q.QueryString(ss => ss.Query("Id:" + obj.Id))));
                LogEvent l = obj;
                if (result.Total >= 1)
                {
                    string _id = result.Hits.First().Id;
                    var r = await _client.UpdateAsync<LogEvent>((u) =>
                    {
                        u.Id(_id);
                        u.Doc(obj);
                        u.Index(_config.IndexName);
                        return u;
                    });
                    return r.IsValid;
                }
                else
                {
                    var resoponse = await _client.IndexAsync<LogEvent>(l, (i) => { i.Index(_config.IndexName); return i; });
                    return resoponse.Created;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return false;
        }

        public static bool AddOrUpdateLogEvent(LogEvent obj)
        {
            try
            {
                var result = _client.Search<LogEvent>(s => s.Query(q => q.QueryString(ss => ss.Query("Id:" + obj.Id))));
                LogEvent l = obj;
                if (result.Total >= 1)
                {
                    string _id = result.Hits.First().Id;
                    var r = _client.Update<LogEvent>((u) =>
                    {
                        u.Id(_id);
                        u.Doc(obj);
                        u.Index(_config.IndexName);
                        return u;
                    });
                    return r.IsValid;
                }
                else
                {
                    var resoponse = _client.Index<LogEvent>(l, (i) => { i.Index(_config.IndexName); return i; });
                    return resoponse.Created;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return false;
        }

        public static async Task<bool> AddOrUpdateBizAsync(BizIndex obj)
        {
            try
            {
                var result = await _client.SearchAsync<BizIndex>(s => s.Query(q => q.QueryString(ss => ss.Query("Id:" + obj.Id))));
                BizIndex l = obj;
                if (result.Total >= 1)
                {
                    string _id = result.Hits.First().Id;
                    var r = await _client.UpdateAsync<BizIndex>((u) =>
                    {
                        u.Id(_id);
                        u.Doc(obj);
                        u.Index(_config.IndexName);
                        return u;
                    });
                    return r.IsValid;
                }
                else
                {
                    var resoponse = await _client.IndexAsync<BizIndex>(l, (i) => { i.Index(_config.IndexName); return i; });
                    return resoponse.Created;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return false;
        }

        public static bool AddOrUpdateBiz(BizIndex obj)
        {
            try
            {
                var result = _client.Search<BizIndex>(s => s.Query(q => q.QueryString(ss => ss.Query("Id:" + obj.Id))));
                BizIndex l = obj;
                if (result.Total >= 1)
                {
                    string _id = result.Hits.First().Id;
                    var r = _client.Update<BizIndex>((u) =>
                    {
                        u.Id(_id);
                        u.Doc(obj);
                        u.Index(_config.IndexName);
                        return u;
                    });
                    return r.IsValid;
                }
                else
                {
                    var resoponse = _client.Index<BizIndex>(l, (i) => { i.Index(_config.IndexName); return i; });
                    return resoponse.Created;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return false;
        }

        public static LogEvent CopyFromLogEventMQ(LogEventMQ mq)
        {
            if (mq == null || string.IsNullOrEmpty(mq.Id) || Guid.Parse(mq.Id).Equals(Guid.Empty))
                return null;
            LogEvent ret = new LogEvent();
            foreach (System.Reflection.PropertyInfo pi in ret.GetType().GetProperties())
            {
                object value = mq.GetType().GetProperty(pi.Name).GetValue(mq);
                if(value!=null)
                    pi.SetValue(ret, value);
            }
            return ret;
        }

        public static BizIndex CopyFromBizMQ(BizMQ mq)
        {
            if (mq == null || string.IsNullOrEmpty(mq.Id) || Guid.Parse(mq.Id).Equals(Guid.Empty))
                return null;
            BizIndex ret = new BizIndex();
            foreach (System.Reflection.PropertyInfo pi in ret.GetType().GetProperties())
            {
                object value = mq.GetType().GetProperty(pi.Name).GetValue(mq);
                if(value!=null)
                    pi.SetValue(ret, value);
            }
            return ret;
        }
    }
}
