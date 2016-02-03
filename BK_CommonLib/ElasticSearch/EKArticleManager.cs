using BK.CommonLib.DB.Redis;
using BK.CommonLib.Log;
using BK.CommonLib.Util;
using BK.Configuration;
using BK.Model.Configuration.ElasticSearch;
using BK.Model.Configuration.Redis;
using BK.Model.DB;
using BK.Model.Index;
using BK.Model.Redis.Objects.EK;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.CommonLib.ElasticSearch
{
    public static class EKArticleManager
    {
        static void LogError(Exception ex)
        {
            LogHelper.LogErrorAsync(typeof(EKArticleManager), ex);
        }
        static ElasticClient _client = null;
        static EKESConfig _config = null;//BK_ConfigurationManager.GetConfig<LocationConfig>();
        static EKArticleManager()
        {
            _client = ESHeper.GetClient<EKESConfig>();
            if (_client == null)
            {
                var err = new Exception("_client没有正确初始化！");
                LogError(err);
                throw err;
            }

            _config = BK_ConfigurationManager.GetConfig<EKESConfig>();
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
            if (!ESHeper.BeSureMapping<EKIndex>(_client, _config.IndexName, new IndexSettings()
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

        public static async Task<bool> AddOrUpdateAsync(EKIndex obj)
        {
            try
            {
                var result = await _client.SearchAsync<EKIndex>(s => s.Query(q => q.QueryString(ss => ss.Query("Id:" + obj.Id))));
                EKIndex l = obj;
                if (result.Total >= 1)
                {
                    string _id = result.Hits.First().Id;
                    var r = await _client.UpdateAsync<EKIndex>((u) =>
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
                    var resoponse = await _client.IndexAsync<EKIndex>(l, (i) => { i.Index(_config.IndexName); return i; });
                    return resoponse.Created;
                }
            }
            catch(Exception ex)
            {
                LogError(ex);
            }
            return false;
        }

        public static bool AddOrUpdate(EKIndex obj)
        {
            try
            {
                var result = _client.Search<EKIndex>(s => s.Query(q => q.QueryString(ss => ss.Query("Id:" + obj.Id))));
                EKIndex l = obj;
                if (result.Total >= 1)
                {
                    string _id = result.Hits.First().Id;
                    var r = _client.Update<EKIndex>((u) =>
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
                    var resoponse = _client.Index<EKIndex>(l, (i) => { i.Index(_config.IndexName); return i; });
                    return resoponse.Created;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return false;
        }

        public static async Task<List<EKIndex>> GetArticlesAsync(int pageIndex, int pagesize)
        {
            int from = pageIndex * pagesize;
            var result = await _client.SearchAsync<EKIndex>(s => s.SortDescending("PublicDate").Skip(from).Size(pagesize));
            if(result!=null && result.Total>0)
            {
                List<EKIndex> list = new List<EKIndex>();
                foreach(EKIndex index in result.Documents)
                {
                    index.BodyText = "";
                    list.Add(index);
                }
                return list;
            }
            return null;
        }

        public static async Task<EKIndex> GetById(long id)
        {
            string ekid = id.ToString();
            try
            {
                var result = await _client.SearchAsync<EKIndex>(s => s.Query(q => q.QueryString(ss => ss.Query("Id:" + ekid))));
                if (result != null && result.Total == 1)
                {
                    return result.Documents.First();
                }
            }
            catch(Exception ex)
            {
                LogError(ex);
            }
            
            return null;
        }

        public static EKIndex CopyFromDB(EKToday obj)
        {
            EKIndex index=null;
            if(obj!=null && obj.ID>=0)
            {
                index = new EKIndex();

                index.Abstract = obj.Abstract;
                index.AccountEmail = obj.AccountEmail;
                index.AccountEmail_uuid = obj.AccountEmail_uuid.ToString();
                index.ArticleType = obj.ArticleType;
                index.BodyText = obj.BodyText;
                index.HeadPic = obj.HeadPic;
                index.HitPoint = obj.HitPoint != null ? obj.HitPoint.Value : 0;
                index.Id = obj.ID.ToString();
                index.IsExotic = obj.IsExotic != null ? obj.IsExotic.Value : false;
                index.IsPublic = obj.IsPublic != null ? obj.IsPublic.Value : false;
                if (obj.IsTop != null)
                    index.IsTop = CommonHelper.ToUnixTime(obj.IsTop.Value);
                index.Keywords = obj.Keywords;
                if (obj.PublicDate != null)
                    index.PublicDate = CommonHelper.ToUnixTime(obj.PublicDate.Value);
                index.ReadPoint = obj.ReadPoint != null ? obj.ReadPoint.Value : 0;
                index.Title = obj.Title;
            }
            return index;
        }


        public static async Task<double> AddZanAsync(long id)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();

            return await redis.AddScoreAsync<EKTodayRedis, EKZanCountZsetAttribute>(id.ToString(), 1);
        }

        public static async Task<bool> SetZanPeopleAsync(long id,Guid user,DateTime time)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();
            double unixTime = CommonHelper.ToUnixTime(time);
            return await redis.SetScoreEveryKeyAsync<EKTodayRedis, EKZanPepleZsetAttribute>(id.ToString(), user.ToString(), unixTime);
        }

        public static async Task<bool> SetReadPeopleAsync(long id, Guid user, DateTime time)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();
            double unixTime = CommonHelper.ToUnixTime(time);
            return await redis.SetScoreEveryKeyAsync<EKTodayRedis, EKReadPepleZsetAttribute>(id.ToString(), user.ToString(), unixTime);
        }

        public static async Task<bool> SetZanAsync(long id,double score)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();

            return await redis.SetScoreAsync<EKTodayRedis, EKZanCountZsetAttribute>(id.ToString(), score);
        }

        public static bool SetZan(long id, double score)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();
            return  redis.SetScore<EKTodayRedis, EKZanCountZsetAttribute>(id.ToString(), score);
        }

        public static async Task<double> AddReadCountAsync(long id)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();

            return await redis.AddScoreAsync<EKTodayRedis, EKReadCountZsetAttribute>(id.ToString(), 1);
        }

        public static async Task<bool> SetReadCountAsync(long id,double score)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();

            return await redis.SetScoreAsync<EKTodayRedis, EKReadCountZsetAttribute>(id.ToString(), score);
        }

        public static bool SetReadCount(long id, double score)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();

            return redis.SetScore<EKTodayRedis, EKReadCountZsetAttribute>(id.ToString(), score);
        }

        public static async Task<double> AddCommentCountAsync(long id)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();

            return await redis.AddScoreAsync<EKTodayRedis, EKCommentCountZsetAttribute>(id.ToString(), 1);
        }

        public static async Task<double> GetCommentCountAsync(long id)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();

            return await redis.GetScoreAsync<EKTodayRedis, EKCommentCountZsetAttribute>(id.ToString());
        }


        private static async Task<List<EKIndex>> _Search(string queryStr,int index,int size)
        {
            try
            {
                string keyword = String.Format("{0}", queryStr);
                var result = await _client.SearchAsync<EKIndex>(s => s.Index(_config.IndexName).Query(q => q.QueryString(qs => qs.Query(keyword).DefaultOperator(Operator.And).DefaultField("Title"))).SortDescending("PublicDate").Skip(index).Take(size));

                if (result != null && result.Documents.Count() > 0)
                    return result.Documents.ToList();
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return null;
        }

        public static async Task<List<EKIndex>> Search(string queryStr, int pageIndex, int pageSize)
        {
            try
            {
                int from = pageIndex * pageSize;
                int size = pageSize;
                List<EKIndex> ret = new List<EKIndex>();

                string[] keywords = queryStr.Split(new char[] { ' ' });
                if(keywords.Length>=1)
                {
                    QueryContainer container = new QueryContainer();
                    List<QueryContainer> containerlist = new List<QueryContainer>();
                    foreach(var s in keywords)
                    {
                        var title = new QueryContainer();
                        title = Query<EKIndex>.QueryString(q => q.Query(s).DefaultOperator(Operator.Or).OnFields(new string[] { "Title", "BodyText" }));
                        containerlist.Add(title);// or  query = (query1 || query2) && query3;
                    }
                    container = containerlist[0];
                    for(int i=1;i<containerlist.Count;i++)
                    {
                        container = container || containerlist[i];
                    }
                    var result = await _client.SearchAsync<EKIndex>(s => s.Index(_config.IndexName).Query(container).Skip(from).Take(size));
                    ret = result.Documents.ToList();
                    foreach(var r in ret)
                    {
                        r.BodyText = "";
                    }
                    return ret;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new List<EKIndex>();
        }

    }
}
