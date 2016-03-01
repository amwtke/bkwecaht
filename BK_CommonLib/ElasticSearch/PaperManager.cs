using BK.CommonLib.DB.Redis;
using BK.CommonLib.Log;
using BK.CommonLib.Util;
using BK.Configuration;
using BK.Model.Configuration.ElasticSearch;
using BK.Model.Configuration.Redis;
using BK.Model.DB;
using BK.Model.Index;
using BK.Model.Redis.Objects.paper;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BK.CommonLib.ElasticSearch
{
    public static class PaperManager
    {
        static void LogError(Exception ex)
        {
            LogHelper.LogErrorAsync(typeof(PaperManager), ex);
        }
        static ElasticClient _client = null;
        static PaperESConfig _config = null;
        static PaperManager()
        {
            _client = ESHeper.GetClient<PaperESConfig>();
            if (_client == null)
            {
                var err = new Exception("_client没有正确初始化！");
                LogError(err);
                throw err;
            }

            _config = BK_ConfigurationManager.GetConfig<PaperESConfig>();
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
            if (!ESHeper.BeSureMapping<PapersIndex>(_client, _config.IndexName, new IndexSettings()
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

        public static async Task<bool> AddOrUpdateAsync(PapersIndex obj)
        {
            try
            {
                var result = await _client.SearchAsync<PapersIndex>(s => s.Query(q => q.QueryString(ss => ss.Query("Id:" + obj.Id))));
                PapersIndex l = obj;
                if (result.Total >= 1)
                {
                    string _id = result.Hits.First().Id;
                    var r = await _client.UpdateAsync<PapersIndex>((u) =>
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
                    var resoponse = await _client.IndexAsync<PapersIndex>(l, (i) => { i.Index(_config.IndexName); return i; });
                    return resoponse.Created;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return false;
        }

        public static async Task<bool> DeleteAsync(long id)
        {
            try
            {
                var result = await _client.SearchAsync<PapersIndex>(s => s.Query(q => q.QueryString(ss => ss.Query("Id:" + id))));
                if (result.Total >= 1)
                {
                    string _id = result.Hits.First().Id;
                    var r = await _client.DeleteAsync<PapersIndex>((u) =>
                    {
                        u.Id(_id);
                        u.Index(_config.IndexName);
                        return u;
                    });
                    return r.IsValid;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return false;
        }

        public static bool AddOrUpdate(PapersIndex obj)
        {
            try
            {
                var result = _client.Search<PapersIndex>(s => s.Query(q => q.QueryString(ss => ss.Query("Id:" + obj.Id))));
                PapersIndex l = obj;
                if (result.Total >= 1)
                {
                    string _id = result.Hits.First().Id;
                    var r = _client.Update<PapersIndex>((u) =>
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
                    var resoponse = _client.Index<PapersIndex>(l, (i) => { i.Index(_config.IndexName); return i; });
                    return resoponse.Created;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return false;
        }

        public static async Task<List<PapersIndex>> GetPapersAsync(List<Guid> users)
        {
            if (users != null && users.Count() > 0)
            {
                List<PapersIndex> ret = new List<PapersIndex>();
                foreach (var v in users)
                {
                    var tmp = await GetByUserUuid(v);
                    if (tmp != null && tmp.Count() > 0)
                        ret.AddRange(tmp);
                }
                return ret;
            }
            return null;
        }
        public static async Task<List<PapersIndex>> GetPapersAsync(long researchFieldId, int pageIndex, int pageSize)
        {
            try
            {
                var result = await _client.SearchAsync<PapersIndex>(s => s.Filter(f => f.Term("ResearchFieldId", researchFieldId))
                                                                            .SortDescending("PublishTime")
                                                                            .Skip(pageIndex * pageSize).Take(pageSize));

                if (result != null && result.Total > 0)
                {
                    List<PapersIndex> ret = new List<PapersIndex>();
                    foreach (var v in result.Documents)
                    {
                        ret.Add(v);
                    }
                    return ret;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return null;
        }

        public static async Task<PapersIndex> GetById(long id)
        {
            string ekid = id.ToString();
            try
            {
                var result = await _client.SearchAsync<PapersIndex>(s => s.Query(q => q.QueryString(ss => ss.Query("Id:" + ekid))));
                if (result != null && result.Total == 1)
                {
                    return result.Documents.First();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return null;
        }

        public static async Task<List<PapersIndex>> GetByUserUuid(Guid uuid)
        {
            string uuidStr = uuid.ToString();
            try
            {
                var result = await _client.SearchAsync<PapersIndex>(s => s.Query(q => q.QueryString(ss => ss.Query("AccountEmail_uuid:" + uuidStr))).SortDescending("PublishTime"));

                if (result != null && result.Total > 0)
                {
                    List<PapersIndex> ret = new List<PapersIndex>();
                    foreach (var v in result.Documents)
                    {
                        ret.Add(v);
                    }
                    return ret;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return null;
        }

        public static PapersIndex CopyFromDB(UserArticle obj)
        {
            PapersIndex index = null;
            if (obj != null && obj.Id > 0)
            {
                index = new PapersIndex();
                index.Id = obj.Id.ToString();
                index.AccountEmail_uuid = obj.AccountEmail_uuid.ToString();
                index.ArticlePath = obj.ArticlePath;
                index.Author = obj.Author;
                index.PostMagazine = obj.PostMagazine != null ? obj.PostMagazine : "";
                if (obj.PublishTime != null)
                    index.PublishTime = CommonHelper.ToUnixTime(obj.PublishTime.Value);
                index.Title = obj.Title;
                index.KeyWords = index.Author.ToLower() + " " + index.PostMagazine.ToLower() + " " + index.Title.ToLower();
                using (DB.Repositorys.UserRepository repo = new DB.Repositorys.UserRepository())
                {
                    index.ResearchFieldId = (repo.GetUserInfoByUuid(obj.AccountEmail_uuid))?.ResearchFieldId ?? 0;
                }

            }
            return index;
        }

        public static async Task<bool> SetZanPeopleAsync(long id, Guid user, DateTime time)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();
            double unixTime = CommonHelper.ToUnixTime(time);
            return await redis.SetScoreEveryKeyAsync<PaperRedis, PZanPeopleZsetAttribute>(id.ToString(), user.ToString(), unixTime);
        }

        public static async Task<bool> SetReadPeopleAsync(long id, Guid user, DateTime time)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();
            double unixTime = CommonHelper.ToUnixTime(time);
            return await redis.SetScoreEveryKeyAsync<PaperRedis, PReadPeopleZsetAttribute>(id.ToString(), user.ToString(), unixTime);
        }

        public static async Task<double> AddZanCountAsync(long id)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();

            return await redis.AddScoreAsync<PaperRedis, PZanCountZsetAttribute>(id.ToString(), 1);
        }

        public static async Task<bool> SetZanCountAsync(long id, double score)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();

            return await redis.SetScoreAsync<PaperRedis, PZanCountZsetAttribute>(id.ToString(), score);
        }

        public static bool SetZanCount(long id, double score)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();
            return redis.SetScore<PaperRedis, PZanCountZsetAttribute>(id.ToString(), score);
        }

        public static async Task<double> AddReadCountAsync(long id)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();

            return await redis.AddScoreAsync<PaperRedis, PReadCountZsetAttribute>(id.ToString(), 1);
        }

        public static async Task<bool> SetReadCountAsync(long id, double score)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();

            return await redis.SetScoreAsync<PaperRedis, PReadCountZsetAttribute>(id.ToString(), score);
        }

        public static bool SetReadCount(long id, double score)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();

            return redis.SetScore<PaperRedis, PReadCountZsetAttribute>(id.ToString(), score);
        }

        public static async Task<double> AddCommentCountAsync(long id)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();

            return await redis.AddScoreAsync<PaperRedis, PCommentCountZsetAttribute>(id.ToString(), 1);
        }

        public static async Task<double> GetCommentCountAsync(long id)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();

            return await redis.GetScoreAsync<PaperRedis, PCommentCountZsetAttribute>(id.ToString());
        }

        //public static async Task<List<PapersIndex>> Search(string queryStr, int pageIndex, int pageSize)
        //{
        //    try
        //    {
        //        int from = pageIndex * pageSize;
        //        int size = pageSize;
        //        List<PapersIndex> ret = new List<PapersIndex>();
        //        List<string> keywords = MakeKeyWordList(queryStr);
        //        if (keywords.Count >= 1)
        //        {
        //            QueryContainer container = new QueryContainer();
        //            List<QueryContainer> containerlist = new List<QueryContainer>();
        //            foreach (var s in keywords)
        //            {
        //                var title = new QueryContainer();
        //                //title = Query<PapersIndex>.Term("KeyWords", s);
        //                title = Query<PapersIndex>.QueryString(q => q.Query(s).DefaultOperator(Operator.And).OnFields(new string[] { "KeyWords" }).Analyzer("ik_smart"));
        //                containerlist.Add(title);// or  query = (query1 || query2) && query3;
        //            }
        //            container = containerlist[0];
        //            for (int i = 1; i < containerlist.Count; i++)
        //            {
        //                container = container || containerlist[i];
        //            }
        //            var result = await _client.SearchAsync<PapersIndex>(s => s.Index(_config.IndexName).Query(container).Skip(from).Take(size));
        //            ret = result.Documents.ToList();
        //            return ret;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogError(ex);
        //    }
        //    return new List<PapersIndex>();
        //}

        //private static List<string> MakeKeyWordList(string queryString)
        //{
        //    List<string> ret = new List<string>();
        //    //Regex regEnglish = new Regex("^[a-zA-Z]");
        //    //if(regEnglish.IsMatch(queryString))
        //    //{
        //    //    ret.Add(queryString);
        //    //    return ret;
        //    //}
        //    ret = queryString.Split(new char[] { ' ' }).ToList();
        //    return ret;
        //}


        //public static async Task<List<PapersIndex>> Search2(string queryStr, int pageIndex, int pageSize)
        //{
        //    try
        //    {
        //        string keyword = string.Format("*{0}*", queryStr);
        //        int from = pageIndex * pageSize;
        //        int size = pageSize;
        //        List<PapersIndex> ret = new List<PapersIndex>();
        //            var result = await _client.SearchAsync<PapersIndex>(s => s.Index(_config.IndexName).Query(q=>q.Term("KeyWords",queryStr)).AnalyzeWildcard(true).Analyzer("ik_smart").Skip(from).Take(size));
        //            ret = result.Documents.ToList();
        //            return ret;

        //    }
        //    catch (Exception ex)
        //    {
        //        LogError(ex);
        //    }
        //    return new List<PapersIndex>();
        //}

        public static async Task<List<PapersIndex>> Search3(string queryStr, int pageIndex, int pageSize)
        {
            try
            {
                string keywords = string.Format("{0}", queryStr.Trim());
                int from = pageIndex * pageSize;
                int size = pageSize;
                List<PapersIndex> ret = new List<PapersIndex>();
                var container = Query<PapersIndex>.QueryString(q => q.Query(keywords).DefaultOperator(Operator.And)
                //.OnFields(new string[] { "KeyWords" })
                .Analyzer("ik_smart"));

                var result = await _client.SearchAsync<PapersIndex>(s => s.Index(_config.IndexName).Query(container).Skip(from).Take(size));
                ret = result.Documents.ToList();
                return ret;

            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new List<PapersIndex>();
        }
    }
}
