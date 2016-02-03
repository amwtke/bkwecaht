using BK.CommonLib.DB.Redis;
using BK.CommonLib.DB.Redis.Objects;
using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.Log;
using BK.Model.Configuration.Redis;
using BK.Model.Redis.Objects.UserBehavior;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BK.WeChat.Controllers.WeChatWebAPIControllers.Find
{
    public static class FindHelper
    {
        static ConcurrentDictionary<string, double> _cacheScore = new ConcurrentDictionary<string, double>();
        static bool flag = true;

        public static void Close()
        {
            flag = false;
        }
        static FindHelper()
        {
            init();
        }

        static void init()
        {
            BK.CommonLib.Util.AsyncHelper.RunAsync(delegate () 
            {
                while(flag)
                {
                    LogHelper.LogInfoAsync(typeof(FindHelper), "开始同步uuid访问记录！");

                    var list = new RedisManager2<WeChatRedisConfig>().GetRangeByRank<NameCardRedis,NameCardCountZsetAttribute>("");

                    foreach(var v in list)
                    {
                        _cacheScore[v.Key] = v.Value;
                    }
                    //System.Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
                    LogHelper.LogInfoAsync(typeof(FindHelper), "结束同步uuid访问记录！");
                    Thread.Sleep(TimeSpan.FromMinutes(10));
                }
            }, null);
        }



        public static bool GetUnivDeptScore(string univName,string deptName,out double uscore,out double dscore)
        {
            using (SystemRepository repo = new SystemRepository())
            {
                var univs = repo.GetUnivIdByName(univName);
                var dept = repo.GetDepByName(univs, deptName);
                uscore = dscore = 0;
                if (univs == null && dept == null)
                {
                    return false;
                }
                if(univs!=null)                    
                    uscore = Convert.ToDouble(univs.UnivsID);
                if(dept!=null)
                    dscore = Convert.ToDouble(dept.ID);
                return true;
            }
        }

        public static async Task<Tuple<List<string>,List<string>,List<string>>> GetThreeSet(bool professor, double univScore, double depScore, double rfScore)
        {
            var _redis = new RedisManager2<WeChatRedisConfig>();

            if (professor)
            {
                var univRedis = await _redis.GetRangeByScoreWithoutScoreAsync<NameCardRedis, UnivProfessorZsetAttribute>("", 0, -1, StackExchange.Redis.Order.Descending, univScore, univScore);
                var depsRedis = await _redis.GetRangeByScoreWithoutScoreAsync<NameCardRedis, DeptProfessorZsetAttribute>("", 0, -1, StackExchange.Redis.Order.Descending, depScore, depScore);

                var rfRedis = await _redis.GetRangeByScoreWithoutScoreAsync<NameCardRedis, ResearchFieldProfessorZsetAttribute>("", 0, -1, StackExchange.Redis.Order.Descending, rfScore, rfScore);

                return Tuple.Create(univRedis, depsRedis, rfRedis);
            }
            else
            {
                var univRedis = await _redis.GetRangeByScoreWithoutScoreAsync<NameCardRedis, UnivStudentZsetAttribute>("", 0, -1, StackExchange.Redis.Order.Descending, univScore, univScore);
                var depsRedis = await _redis.GetRangeByScoreWithoutScoreAsync<NameCardRedis, DeptStudentZsetAttribute>("", 0, -1, StackExchange.Redis.Order.Descending, depScore, depScore);

                var rfRedis = await _redis.GetRangeByScoreWithoutScoreAsync<NameCardRedis, ResearchFieldStudentZsetAttribute>("", 0, -1, StackExchange.Redis.Order.Descending, rfScore, rfScore);
                return Tuple.Create(univRedis, depsRedis, rfRedis);
            }
        }

        public static List<string> SortByAccessCount(List<string> listuuid)
        {
            SortedDictionary<double, List<string>> _dic = new SortedDictionary<double, List<string>>();

            foreach(var s in listuuid)
            {
                double score = 0;
                if (_cacheScore.ContainsKey(s))
                    score = _cacheScore[s];

                if (!_dic.ContainsKey(score))
                    _dic.Add(score, new List<string>());
                _dic[score].Add(s);
            }
            List<string> ret = new List<string>();
            foreach(var p in _dic)
            {
                ret.AddRange(p.Value);
            }
            ret.Reverse();
            return ret;
        }

        /// <summary>
        /// index zero based
        /// </summary>
        /// <param name="univs"></param>
        /// <param name="deps"></param>
        /// <param name="rfs"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static async Task<List<string>> FindStudtenRule(string self,List<string> univs, List<string> deps, List<string> rfs, int pageNumber, int pageSize)
        {
            //补救部分为空的项
            if (univs == null)
                univs = new List<string>();
            if (deps == null)
                deps = new List<string>();
            if (rfs == null)
                rfs = new List<string>();

            //按照访问数先排序
            univs = SortByAccessCount(univs);
            deps = SortByAccessCount(deps);
            rfs = SortByAccessCount(rfs);

            //计算交集 rf > u >d
            var ud = univs.Intersect(deps);//应该就是d
            var second = ud.Union(deps);//同一个学校同一个学院的
            var first = deps.Intersect(rfs);//同一个学校同一个学院同一个专业
            var third = univs;//同一个学校的
            var fourth = rfs;//同一个专业的

            var result = first.Union(second).Union(third).Union(fourth).ToList();
            result.Remove(self);
            //计算分页结果
            List<string> retList = new List<string>();
            long from = pageNumber * pageSize;
            int start = (int)from;
            if(start<result.Count())
            {
                if(start+pageSize<=result.Count())
                    return result.Skip(start).Take(pageSize).ToList();
                else
                {
                    foreach(var s in result.ToList().Skip(start))
                    {
                        retList.Add(s);
                        pageSize--;
                    }

                    start = 0;
                }
            }
            else //如果start超出或者等于result数量
            {
                start = start - result.Count();
            }

            //用教授的点击排名补足不够的部分
            long to = start + pageSize - 1;
            var tmp = await new RedisManager2<WeChatRedisConfig>().GetRangeByRankAsync<NameCardRedis, NameCardSCountZsetAttribute>("", from: start, to: to);
            if (tmp != null && tmp.Length > 0)
            {
                //self = self.ToLower();
                //foreach (var s in tmp)
                //{
                //    if(!s.Key.Equals(self))
                //        retList.Add(s.Key);
                //}

                //不去虫
                foreach (var s in tmp)
                {
                    retList.Add(s.Key);
                }
            }

            return retList;
        }

        public static List<string> FindProfessorRule(Guid self,List<string> univs, List<string> deps, List<string> rfs, int pageNumber, int pageSize)
        {
            //补救部分为空的项
            if (univs == null)
                univs = new List<string>();
            if (deps == null)
                deps = new List<string>();
            if (rfs == null)
                rfs = new List<string>();
            //按照访问数先排序
            univs = SortByAccessCount(univs);
            deps = SortByAccessCount(deps);
            rfs = SortByAccessCount(rfs);
            //计算交集 rf > u >d
            var ud = univs.Intersect(deps);//应该就是d
            var third = ud.Union(deps);//同一个学校同一个学院的
            var first = deps.Intersect(rfs);//同一个学校同一个学院同一个专业
            var fourth = univs;//同一个学校的
            var second = rfs;//同一个专业的

            var result = first.Union(second);//.Union(third).Union(fourth);

            //计算分页结果
            List<string> retList = result.ToList(); retList.Remove(self.ToString().ToUpper());

            long from = pageNumber * pageSize;
            int start = (int)from;

            #region 不需要返回rf不相关的名片
            //if (start < result.Count())
            //{
            //    if (start + pageSize <= result.Count())
            //        return result.ToList().Skip(start).Take(pageSize).ToList();
            //    else
            //    {
            //        foreach (var s in result.ToList().Skip(start))
            //        {
            //            retList.Add(s);
            //            pageSize--;
            //        }
            //        start = 0;
            //    }
            //}
            //else
            //{
            //    start = start - result.Count();
            //}

            ////用教授的点击排名补足不够的部分
            //long to = start + pageSize - 1;
            //var tmp = await new RedisManager2<WeChatRedisConfig>().GetRangeByRankAsync<NameCardRedis, NameCardPCountZsetAttribute>("", from: start, to: to);
            //if (tmp != null & tmp.Length > 0)
            //{
            //    foreach (var s in tmp)
            //    {
            //        retList.Add(s.Key);
            //    }
            //}
            #endregion
            return retList.Skip(start).Take(pageSize).ToList();
        }

        public static async Task<List<string>> FindProfessorRuleForStudent(List<string> univs, List<string> deps, List<string> rfs, int pageNumber, int pageSize)
        {
            //补救部分为空的项
            if (univs == null)
                univs = new List<string>();
            if (deps == null)
                deps = new List<string>();
            if (rfs == null)
                rfs = new List<string>();
            //按照访问数先排序
            univs = SortByAccessCount(univs);
            deps = SortByAccessCount(deps);
            rfs = SortByAccessCount(rfs);
            //计算交集 rf > u >d
            var ud = univs.Intersect(deps);//应该就是d
            var third = ud.Union(deps);//同一个学校同一个学院的
            var first = deps.Intersect(rfs);//同一个学校同一个学院同一个专业
            var fourth = univs;//同一个学校的
            var second = rfs;//同一个专业的

            var result = first.Union(second).Union(third).Union(fourth);

            //计算分页结果
            List<string> retList = new List<string>();

            long from = pageNumber * pageSize;
            int start = (int)from;

            if (start < result.Count())
            {
                if (start + pageSize <= result.Count())
                    return result.ToList().Skip(start).Take(pageSize).ToList();
                else
                {
                    foreach (var s in result.ToList().Skip(start))
                    {
                        retList.Add(s);
                        pageSize--;
                    }
                    start = 0;
                }
            }
            else
            {
                start = start - result.Count();
            }

            //用教授的点击排名补足不够的部分
            long to = start + pageSize - 1;
            var tmp = await new RedisManager2<WeChatRedisConfig>().GetRangeByRankAsync<NameCardRedis, NameCardPCountZsetAttribute>("", from: start, to: to);
            if (tmp != null & tmp.Length > 0)
            {
                foreach (var s in tmp)
                {
                    retList.Add(s.Key);
                }
            }
            return retList;
        }
    }
}