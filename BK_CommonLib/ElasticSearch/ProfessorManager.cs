using BK.CommonLib.DB.Redis;
using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.Log;
using BK.Configuration;
using BK.Model.Configuration.ElasticSearch;
using BK.Model.Configuration.Redis;
using BK.Model.DB;
using BK.Model.Index;
using BK.Model.Redis.Objects.UserBehavior;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.CommonLib.ElasticSearch
{
    public static class ProfessorManager
    {
        static void LogError(Exception ex)
        {
            LogHelper.LogErrorAsync(typeof(ProfessorManager), ex);
        }
        static ElasticClient _client = null;
        static ProfessorESConfig _config = null;
        static RedisManager2<WeChatRedisConfig> _redis = new RedisManager2<WeChatRedisConfig>();
        static ProfessorManager()
        {
            _client = ESHeper.GetClient<ProfessorESConfig>();
            if (_client == null)
            {
                var err = new Exception("_client没有正确初始化！");
                LogError(err);
                throw err;
            }

            _config = BK_ConfigurationManager.GetConfig<ProfessorESConfig>();
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
            if (!ESHeper.BeSureMapping<ProfessorIndex>(_client, _config.IndexName, new IndexSettings()
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
        public static ProfessorIndex GenObject(Guid uuid)
        {
            if (uuid.Equals(Guid.Empty))
                return null;
            ProfessorIndex index = new ProfessorIndex();
            index.Id = uuid.ToString();
            StringBuilder sb = new StringBuilder();
            using (UserRepository repo = new UserRepository())
            {
                var userinfo = repo.GetUserInfoByUuid_TB(uuid);
                if (userinfo.ResearchFieldId == null || userinfo.IsBusiness != 0)
                    return null;

                UserEducation userEducation = new UserEducation();
                userEducation.AccountEmail_uuid = uuid;
                var educationList = repo.GetUserRecords_TB<UserEducation>(userEducation);

                UserAcademic userAc = new UserAcademic();userAc.AccountEmail_uuid = uuid;
                var acadmicList = repo.GetUserRecords_TB<UserAcademic>(userAc);

                //research id
                index.ResearchId = userinfo.ResearchFieldId.Value;
                sb.Append(string.Format("{0} ",userinfo.ResearchFieldId.Value.ToString()));
                //姓名
                index.Name = userinfo.Name;
                sb.Append(string.Format("{0} ", userinfo.Name));
                //研究兴趣
                sb.Append(string.Format("{0} ", userinfo.Interests));
                index.Interests = userinfo.Interests;
                //单位
                sb.Append(string.Format("{0} ", userinfo.Unit));
                index.Danwei = userinfo.Unit;
                //地位
                if (acadmicList!=null && acadmicList.Count>0)
                {
                    List<string> tmp = new List<string>();
                    int diweiScore = 0;
                    foreach(var v in acadmicList)
                    {
                        if(!string.IsNullOrEmpty(v.Association))
                        {
                            if (!tmp.Contains("协会委员"))
                            {
                                tmp.Add("协会委员");
                                diweiScore += 1;
                            }
                        }

                        if (!string.IsNullOrEmpty(v.Fund))
                        {
                            if (!tmp.Contains("基金评审"))
                            {
                                tmp.Add("基金评审");
                                diweiScore += 4;
                            }
                        }

                        if (!string.IsNullOrEmpty(v.Magazine))
                        {
                            if (!tmp.Contains("杂志编委"))
                            {
                                tmp.Add("杂志编委");
                                diweiScore += 2;
                            }
                        }
                    }
                    StringBuilder diweisb = new StringBuilder();
                    foreach(string s in tmp)
                    {
                        sb.Append(string.Format("{0} ", s));
                        diweisb.Append(string.Format("{0} ", s));
                    }
                    index.Diwei = diweisb.ToString();
                    index.DiweiScore = diweiScore;
                }

                //校友
                StringBuilder xiaoyousb = new StringBuilder();
                if(educationList!=null && educationList.Count>0)
                {
                    foreach(var v in educationList)
                    {
                        if(!string.IsNullOrEmpty(v.School))
                        {
                            sb.Append(string.Format("{0} ", v.School));
                            xiaoyousb.Append(string.Format("{0} ", v.School));
                        }
                    }
                    index.Education = xiaoyousb.ToString();
                }
                //地点
                if(!string.IsNullOrEmpty(userinfo.Address))
                {
                    string adress = userinfo.Address.Replace(" ", "").Trim();
                    sb.Append(string.Format("{0} ", adress));
                    index.Address = adress;
                }

                //点击量
                double score = _redis.GetScore<NameCardRedis, NameCardPCountZsetAttribute>(userinfo.uuid.ToString());
                index.AccessCount = score;
            }
            index.KeyWords = sb.ToString();
            return index;
        }
        public static async Task<bool> AddOrUpdateAsync(ProfessorIndex obj)
        {
            try
            {
                var result = await _client.SearchAsync<ProfessorIndex>(s => s.Query(q => q.QueryString(ss => ss.Query("Id:" + obj.Id))));
                ProfessorIndex l = obj;
                if (result.Total >= 1)
                {
                    string _id = result.Hits.First().Id;
                    var r = await _client.UpdateAsync<ProfessorIndex>((u) =>
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
                    var resoponse = await _client.IndexAsync<ProfessorIndex>(l, (i) => { i.Index(_config.IndexName); return i; });
                    return resoponse.Created;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return false;
        }

        public static bool AddOrUpdate(ProfessorIndex obj)
        {
            try
            {
                var result = _client.Search<ProfessorIndex>(s => s.Query(q => q.QueryString(ss => ss.Query("Id:" + obj.Id))));
                ProfessorIndex l = obj;
                if (result.Total >= 1)
                {
                    string _id = result.Hits.First().Id;
                    var r = _client.Update<ProfessorIndex>((u) =>
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
                    var resoponse = _client.Index<ProfessorIndex>(l, (i) => { i.Index(_config.IndexName); return i; });
                    return resoponse.Created;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return false;
        }

        public static async Task<List<ProfessorIndex>> Search(Guid myUuuid,string queryStr, int pageIndex, int pageSize)
        {
            try
            {
                string keywords = string.Format("{0}", queryStr.Trim());
                keywords = await ESHeper.AnalyzeQueryString(_client, _config.IndexName, "ik_smart", keywords);
                int from = pageIndex * pageSize;
                int size = pageSize;
                List<ProfessorIndex> ret = new List<ProfessorIndex>();

                //主搜索
                var container = Query<ProfessorIndex>.QueryString(q => q.Query(keywords).DefaultOperator(Operator.And)
                .OnFields(new string[] { "Name", "Interests", "Diwei", "Danwei", "Address" })
                .Analyzer("ik_smart"));

                //container = container && Query<ProfessorIndex>.QueryString(q => q.Query(keywords).DefaultOperator(Operator.Or)
                //.OnFields(new string[] { "Interests" })
                //.Analyzer("ik_smart"));

                var myuuidContainer = Query<ProfessorIndex>.Term("Id", myUuuid.ToString());
                container = container  && (!myuuidContainer);

                //search
                var result = await _client.SearchAsync<ProfessorIndex>(s => s.Index(_config.IndexName).Query(container).SortDescending("DiweiScore")
                .SortDescending("AccessCount")
                .Skip(from).Take(size));
                ret = result.Documents.ToList();

                return ret;

            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new List<ProfessorIndex>();
        }

        public static async Task<List<ProfessorIndex>> Search_rf(long rfid, string queryStr, int pageIndex, int pageSize)
        {
            try
            {
                string keywords = queryStr; ;
                int from = pageIndex * pageSize;
                int size = pageSize;
                List<ProfessorIndex> ret = new List<ProfessorIndex>();
                var rfContainer = Query<ProfessorIndex>.Term("ResearchId", rfid.ToString());
                QueryContainer container = null;
                if (!string.IsNullOrEmpty(keywords))
                {
                    var kwContainer = Query<ProfessorIndex>.QueryString(q => q.Query(keywords).DefaultOperator(Operator.And).Analyzer("ik_smart"));
                    container = rfContainer && kwContainer;
                }
                else
                {
                    container = rfContainer;
                }

                var result = await _client.SearchAsync<ProfessorIndex>(s => s.Index(_config.IndexName).Query(container).Skip(from).Take(size).SortDescending("AccessCount"));
                ret = result.Documents.ToList();

                return ret;

            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new List<ProfessorIndex>();
        }

        public static async Task<List<ProfessorIndex>> Search_UUid(List<string>uuidList)
        {
            try
            { 
                var container = Query<ProfessorIndex>.Term("Id", uuidList[0].ToLower());
                for (int i = 1; i < uuidList.Count; i++)
                {
                    container = container || Query<ProfessorIndex>.Term("Id", uuidList[i].ToLower());
                }
                //search
                var result = await _client.SearchAsync<ProfessorIndex>(s => s.Index(_config.IndexName).Query(container).SortDescending("DiweiScore").SortDescending("AccessCount"));
                var ret = result.Documents.ToList();

                return ret;
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new List<ProfessorIndex>();
        }

        public static async Task<List<ProfessorIndex>> Search_rf3(Guid uuid, bool xiaoyou = false, string danwei = "", int diwei =0, string address = "", int pageIndex = 0, int pageSize = 10)
        {
            try
            {
                int from = pageIndex * pageSize;
                int size = pageSize;
                List<ProfessorIndex> ret = new List<ProfessorIndex>();
                UserInfo userinfo = null; List<UserEducation> eduList = null;
                using (UserRepository repo = new UserRepository())
                {
                    userinfo = await repo.GetUserInfoByUuid(uuid);
                    eduList = await repo.GetUserRecordsByUuid<UserEducation>(userinfo.uuid);
                }

                //researchfieldid
                if (userinfo.ResearchFieldId == null)
                    return new List<ProfessorIndex>();
                string rfid = userinfo.ResearchFieldId.ToString();

                var rfContainer = Query<ProfessorIndex>.Term("ResearchId", rfid.ToString());
                var myuuidContainer = Query<ProfessorIndex>.Term("Id", uuid.ToString());
                QueryContainer container = rfContainer && !myuuidContainer;

                //标签
                if (diwei != 0 || xiaoyou)
                {
                    QueryContainer xyContainer = null;
                    QueryContainer diweiContainer = null;
                    QueryContainer lableContainer = null;
                    //地位
                    if (diwei != 0)
                    {
                        diweiContainer = MakeDiweiContainer(diwei);
                    }

                    //校友
                    if (xiaoyou && eduList != null)
                    {
                        List<string> tmpList = new List<string>();
                        foreach (UserEducation v in eduList)
                        {
                            if (string.IsNullOrEmpty(v.School))
                                continue;

                            if (xyContainer == null)
                            {
                                xyContainer = Query<ProfessorIndex>.QueryString(q => q.Query(v.School).OnFields(new string[] { "Education" }).DefaultOperator(Operator.And).Analyzer("ik_smart"));
                                tmpList.Add(v.School);
                            }
                            else
                            {
                                if (!tmpList.Contains(v.School))
                                {
                                    xyContainer = xyContainer || (Query<ProfessorIndex>.QueryString(q => q.Query(v.School).OnFields(new string[] { "Education" }).DefaultOperator(Operator.Or).Analyzer("ik_smart")));
                                    tmpList.Add(v.School);
                                }
                            }
                        }
                    }
                    //统计
                    if (xyContainer != null && diweiContainer != null)
                        lableContainer = diweiContainer || xyContainer;
                    else
                    {
                        lableContainer = xyContainer != null ? xyContainer : diweiContainer;
                    }

                    //合并
                    if (lableContainer != null)
                        container = container && lableContainer;
                }
                //单位
                if (!string.IsNullOrEmpty(danwei))
                {
                    container = container && (Query<ProfessorIndex>.QueryString(q => q.Query(danwei).DefaultOperator(Operator.And).Analyzer("ik_smart")));
                }

                //address
                if (!string.IsNullOrEmpty(address))
                {
                    var addressContainer = Query<ProfessorIndex>.QueryString(q => q.Query(address).DefaultOperator(Operator.Or).Analyzer("ik_smart"));
                    container = container && addressContainer;
                }
                //search
                var result = await _client.SearchAsync<ProfessorIndex>(s => s.Index(_config.IndexName).Query(container).Skip(from).Take(size).SortDescending("DiweiScore").SortDescending("AccessCount"));
                ret = result.Documents.ToList();

                return ret;
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new List<ProfessorIndex>();
        }

        public static int GetDiweiScore(string diwei)
        {
            int score = 0;

            if(!string.IsNullOrEmpty(diwei))
            {
                diwei = diwei.Trim();
                List<string> tmp = diwei.Split(new char[] { ' ' }).ToList();
                foreach(var v in tmp)
                {
                    if (v.Trim().Equals("协会委员"))
                        score += 1;
                    if (v.Trim().Equals("基金评审"))
                        score += 4;
                    if (v.Trim().Equals("杂志编委"))
                        score += 2;
                }
            }
            return score;
        }

        private static QueryContainer MakeDiweiContainer(int diweiScore)
        {
            if (diweiScore > 0)
            {
                QueryContainer container = Query<ProfessorIndex>.Term("DiweiScore", diweiScore);
                for (int i = 1; i < 8; i++)
                {
                    if (i != diweiScore)
                    {
                        if((i<diweiScore && (i|diweiScore)==diweiScore)||(i > diweiScore && (i | diweiScore) == i))
                            container = container || Query<ProfessorIndex>.Term("DiweiScore", i);
                    }
                }
                
                return container; 
            }
            return null;
        }

        public static bool IsXiaoYou(List<string> myEducation,string eduStr)
        {
            if (string.IsNullOrEmpty(eduStr))
                return false;
            eduStr = eduStr.Trim().ToLower();
            foreach(var v in myEducation)
            {
                if (eduStr.Contains(v))
                    return true;
            }
            return false;
        }

        public static async Task<List<string>> GetUserEducations(Guid uuid)
        {
            List<UserEducation> eduList = null;
            List<string> ret = null;
            using (UserRepository repo = new UserRepository())
            {
                var userinfo = await repo.GetUserInfoByUuid(uuid);
                eduList = await repo.GetUserRecordsByUuid<UserEducation>(userinfo.uuid);
            }
            if(eduList!=null)
            {
                ret = new List<string>();
                foreach(var v in eduList)
                {
                    string school = v.School.Trim().ToLower();
                    if (!string.IsNullOrEmpty(school) && !ret.Contains(school))
                        ret.Add(school);
                }
            }

            return ret;
        }

        public static async Task<List<ProfessorIndex>> SearchEducation(string queryStr, Guid uuid,int pageIndex, int pageSize)
        {
            try
            {
                string keywords = string.Format("{0}", queryStr.Trim());
                int from = pageIndex * pageSize;
                int size = pageSize;
                List<ProfessorIndex> ret = new List<ProfessorIndex>();

                //教育经历
                var education = Query<ProfessorIndex>.QueryString(q => q.Query(keywords).DefaultOperator(Operator.Or)
                .OnFields(new string[] { "Education" })
                .Analyzer("ik_smart"));

                if(!uuid.Equals(Guid.Empty))
                {
                    var uuidContainer = Query<ProfessorIndex>.Term("Id", uuid.ToString());
                    education = education && uuidContainer;
                }

                var result = await _client.SearchAsync<ProfessorIndex>(s => s.Index(_config.IndexName).Query(education).SortDescending("DiweiScore")
                .SortDescending("AccessCount")
                .Skip(from).Take(size));
                ret = result.Documents.ToList();

                return ret;

            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new List<ProfessorIndex>();
        }
    }
}
