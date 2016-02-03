using BK.CommonLib.DB.Redis;
using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.ElasticSearch;
using BK.CommonLib.Log;
using BK.CommonLib.Util;
using BK.Model.Configuration.Redis;
using BK.Model.DB;
using BK.Model.Index;
using BK.Model.Redis.Objects.EK;
using BK.Model.Redis.Objects.UserBehavior;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace BK.DataService
{
    public partial class Service1 : ServiceBase
    {
        static RedisManager2<WeChatRedisConfig> _redis;
        static bool CloseService = false;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            LogHelper.LogInfoAsync(typeof(Service1), "BK.Data服务开始启动...");
            init();
            LogHelper.LogInfoAsync(typeof(Service1), "BK.Data服务完成！");
        }

        private void init()
        {
            _redis = new RedisManager2<WeChatRedisConfig>();
            //启动线程
            //BK.data 同步学校，专业与学员信息到zset
            AsyncHelper.RunAsync(delegate ()
            {
                while (!CloseService)
                {

                    try
                    {
                        LogHelper.LogInfoAsync(typeof(Service1), "BK.Data开始整理数据!");

                        ProcessData();
                        LogHelper.LogInfoAsync(typeof(Service1), "BK.Data结束整理数据!");
                        System.Threading.Thread.Sleep(TimeSpan.FromHours(2));
                    }
                    catch(Exception ex)
                    {
                        LogHelper.LogErrorAsync(typeof(Service1), ex);
                    }
                }
            }, null);
            System.Threading.Thread.Sleep(10);

            //EK 同步EK的文章，阅读数与赞数。
            AsyncHelper.RunAsync(delegate ()
            {
                while (!CloseService)
                {

                    try
                    {
                        LogHelper.LogInfoAsync(typeof(Service1), "BK.EK开始整理数据!");

                        ProcessEK();

                        LogHelper.LogInfoAsync(typeof(Service1), "BK.EK结束整理数据!");
                        System.Threading.Thread.Sleep(TimeSpan.FromHours(2));
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogErrorAsync(typeof(Service1), ex);
                    }
                }
            }, null);
            System.Threading.Thread.Sleep(10);

            //EK 赞的人与 读者列表同步
            AsyncHelper.RunAsync(delegate ()
            {
                while (!CloseService)
                {

                    try
                    {
                        LogHelper.LogInfoAsync(typeof(Service1), "BK.ZanerAndReader开始整理数据!");

                        ProcessEKZanerAndReader();

                        LogHelper.LogInfoAsync(typeof(Service1), "BK.ZanerAndReader结束整理数据!");
                        System.Threading.Thread.Sleep(TimeSpan.FromHours(2));
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogErrorAsync(typeof(Service1), ex);
                    }
                }
            }, null);
            System.Threading.Thread.Sleep(10);

            //Parer 同步
            AsyncHelper.RunAsync(delegate ()
            {
                while (!CloseService)
                {

                    try
                    {
                        LogHelper.LogInfoAsync(typeof(Service1), "BK.Paper开始整理数据!");

                        ProcessPaper();

                        LogHelper.LogInfoAsync(typeof(Service1), "BK.Paper结束整理数据!");
                        System.Threading.Thread.Sleep(TimeSpan.FromHours(2));
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogErrorAsync(typeof(Service1), ex);
                    }
                }
            }, null);
            System.Threading.Thread.Sleep(10);

            //同步教授信息
            AsyncHelper.RunAsync(delegate ()
            {
                while (!CloseService)
                {

                    try
                    {
                        LogHelper.LogInfoAsync(typeof(Service1), "BK.Professor开始整理数据!");

                        ProcessProfessors();

                        LogHelper.LogInfoAsync(typeof(Service1), "BKBK.Professor结束整理数据!");
                        System.Threading.Thread.Sleep(TimeSpan.FromHours(2));
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogErrorAsync(typeof(Service1), ex);
                    }
                }
            }, null);
            System.Threading.Thread.Sleep(10);
        }

        protected override void OnStop()
        {
            try
            {
                CloseService = true;
                _redis.Close();
            }
            catch (Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(Service1), ex);
            }
            LogHelper.LogInfoAsync(typeof(Service1), "BK.Data服务结束！");
        }

        private void ProcessData()
        {
            int count = 0;
            using (UserRepository userRepo = new UserRepository())
            {
                using (SystemRepository repo = new SystemRepository())
                {
                    foreach (var u in userRepo.GetAllUserInfo())
                    {
                        Univs univs=null;
                        if (!string.IsNullOrEmpty(u.Unit))
                        {
                            string xuexiao = u.Unit;
                            univs = repo.GetUnivIdByName(xuexiao);
                            if(univs!=null)
                            {
                                try
                                {
                                    //更新uuid到学校的zset
                                    double score = Convert.ToDouble(univs.UnivsID);
                                    _redis.SetScore<NameCardRedis, UnivZsetAttribute>(u.uuid.ToString().ToUpper(), score);

                                    //更新到教授
                                    if(u.IsBusiness==0)
                                        _redis.SetScore<NameCardRedis, UnivProfessorZsetAttribute>(u.uuid.ToString().ToUpper(), score);
                                    //更新到学生
                                    if (u.IsBusiness == 2)
                                        _redis.SetScore<NameCardRedis, UnivStudentZsetAttribute>(u.uuid.ToString().ToUpper(), score);

                                }
                                catch(Exception ex)
                                {
                                    LogHelper.LogErrorAsync(typeof(Service1), ex);
                                }
                            }
                            else
                            {
                                //如果没有找到则什么都不做
                            }
                        }

                        //学院zset
                        if (!string.IsNullOrEmpty(u.Faculty))
                        {
                            string xueyuan = u.Faculty;
                            var dept =repo.GetDepByName(univs,xueyuan);
                            if (dept != null)
                            {
                                try
                                {
                                    double score = Convert.ToDouble(dept.ID);
                                    //更新整体
                                    _redis.SetScore<NameCardRedis, DeptZsetAttribute>(u.uuid.ToString().ToUpper(), score);

                                    //更新教授
                                    if (u.IsBusiness == 0)
                                        _redis.SetScore<NameCardRedis, DeptProfessorZsetAttribute>(u.uuid.ToString().ToUpper(), score);
                                    //更新学生
                                    if (u.IsBusiness == 2)
                                        _redis.SetScore<NameCardRedis, DeptStudentZsetAttribute>(u.uuid.ToString().ToUpper(), score);
                                }
                                catch(Exception ex)
                                {
                                    LogHelper.LogErrorAsync(typeof(Service1), ex);
                                }
                            }
                        }

                        //专业与研究领域zset
                        if (u.ResearchFieldId!=null)
                        {
                            try
                            {
                                double score = Convert.ToDouble(u.ResearchFieldId);

                                //更新整体
                                _redis.SetScore<NameCardRedis, ResearchFieldZsetAttribute>(u.uuid.ToString().ToUpper(), score);
                                //更新教授
                                if (u.IsBusiness == 0)
                                    _redis.SetScore<NameCardRedis, ResearchFieldProfessorZsetAttribute>(u.uuid.ToString().ToUpper(), score);
                                //更新学生
                                if (u.IsBusiness == 2)
                                    _redis.SetScore<NameCardRedis, ResearchFieldStudentZsetAttribute>(u.uuid.ToString().ToUpper(), score);
                            }
                            catch (Exception ex)
                            {
                                LogHelper.LogErrorAsync(typeof(Service1), ex);
                            }
                        }
                        //LogHelper.LogInfoAsync(typeof(Service1), "第"+(++count)+"条！");
                    }//userinfo loop
                }
            }
        }

        private void ProcessEK()
        {
            var redis = new RedisManager2<WeChatRedisConfig>();
            //同步articl 数据
            using (UserRepository r = new UserRepository())
            {
                Dictionary<long, int> updateReadDic = new Dictionary<long, int>();
                Dictionary<long, int> updateZanDic = new Dictionary<long, int>();
                foreach (var v in r.GetAllEKToday())
                {
                    var index = EKArticleManager.CopyFromDB(v);
                    EKArticleManager.AddOrUpdate(index);

                    //更新read count
                    double readScore = redis.GetScore<EKTodayRedis, EKReadCountZsetAttribute>(v.ID.ToString());
                    int rrCount = Convert.ToInt32(readScore);
                    if(rrCount < v.ReadPoint)
                    {
                        EKArticleManager.SetReadCount(v.ID, v.ReadPoint.Value);
                    }
                    else
                    {
                        //r.SetReadCount_TB(v.ID, rrCount);
                        updateReadDic.Add(v.ID, rrCount);
                    }

                    //更新zanshu
                    double zanscore = redis.GetScore<EKTodayRedis, EKZanCountZsetAttribute>(v.ID.ToString());
                    int zrCount = Convert.ToInt32(zanscore);
                    if (zrCount < v.HitPoint)
                    {
                        EKArticleManager.SetZan(v.ID, v.HitPoint.Value);
                    }
                    else
                    {
                        //r.SetZanCount(v.ID, zrCount);
                        updateZanDic.Add(v.ID, zrCount);
                    }
                }

                //update read
                foreach(var pair in updateReadDic)
                {
                    r.SetReadCount_TB(pair.Key, pair.Value);
                }

                //update zan
                foreach(var pair in updateZanDic)
                {
                    r.SetZanCount(pair.Key, pair.Value);
                }
            }
        }

        private void ProcessEKZanerAndReader()
        {
            var redis = new RedisManager2<WeChatRedisConfig>();
            //
            using (UserRepository r = new UserRepository())
            {
                foreach (var v in r.GetAllEKToday())
                {
                    long id = v.ID;
                    #region zan
                    using (UserRepository rzan = new UserRepository())
                    {
                        var listDBZaner = rzan.GetEKZaners(id);
                        var listRedisZaner = redis.GetRangeByRank<EKTodayRedis, EKZanPepleZsetAttribute>(id.ToString());

                        if (listDBZaner == null) listDBZaner = new List<article_praise>();
                        if (listRedisZaner == null) listRedisZaner = new KeyValuePair<string, double>[] { };
                        //LogHelper.LogInfoAsync(typeof(Service1), "zan:" + id.ToString() +" "  +listDBZaner.Count()+" " +listRedisZaner.Count());

                        //DB -> reids
                        if (listDBZaner.Count() > 0 || listRedisZaner.Count() > 0)
                        {
                            if (listDBZaner.Count() > listRedisZaner.Count())
                            {
                                foreach (var p in listDBZaner)
                                {
                                    DateTime t = p.praisetime != null ? p.praisetime.Value : DateTime.Now;
                                    double score = BK.CommonLib.Util.CommonHelper.ToUnixTime(t);
                                    
                                    redis.SetScoreEveryKey<EKTodayRedis, EKZanPepleZsetAttribute>(id.ToString(), p.useraccount_uuid.ToString(), score);

                                    //LogHelper.LogInfoAsync(typeof(Service1), "zan:" + id.ToString()+" uuid:"+p.useraccount_uuid.ToString()+" score:"+score.ToString());
                                }
                            }
                            else
                            {
                                foreach (var p in listRedisZaner)
                                {
                                    Guid uuid = Guid.Parse(p.Key);
                                    double score = p.Value;
                                    rzan.AddEKZaner(id, uuid, score);

                                    //LogHelper.LogInfoAsync(typeof(Service1), "-->zan:" + id.ToString() + " uuid:" + p.Key + " score:" + score.ToString());
                                }
                            }
                        }
                    }


                    #endregion

                    #region reader
                    using (UserRepository rreader = new UserRepository())
                    {
                        var listDBreader = rreader.GetEKReaders(id);
                        var listRedisreader = redis.GetRangeByRank<EKTodayRedis, EKReadPepleZsetAttribute>(id.ToString());

                        if (listDBreader == null) listDBreader = new List<article_reader>();
                        if (listRedisreader == null) listRedisreader = new KeyValuePair<string, double>[] { };

                        //DB -> reids
                        if (listDBreader.Count() > 0 || listRedisreader.Count() > 0)
                        {
                            if (listDBreader.Count() > listRedisreader.Count())
                            {
                                foreach (var p in listDBreader)
                                {
                                    DateTime t = p.readtime != null ? p.readtime.Value : DateTime.Now;
                                    double score = BK.CommonLib.Util.CommonHelper.ToUnixTime(t);

                                    redis.SetScoreEveryKey<EKTodayRedis, EKReadPepleZsetAttribute>(id.ToString(), p.useraccount_uuid.ToString(), score);
                                }
                            }
                            else
                            {
                                foreach (var p in listRedisreader)
                                {
                                    Guid uuid = Guid.Parse(p.Key);
                                    double score = p.Value;
                                    rreader.AddEKReader(id, uuid, score);
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
        }

        private void ProcessPaper()
        {
            using (UserRepository repo = new UserRepository())
            {
                string addedStatus = ((int)PaperStatus.Added).ToString();
                foreach (var paper in repo.GetAllPapers())
                {
                    try
                    {
                        if (paper.Status.Equals(addedStatus))
                        {
                            PapersIndex tmp = PaperManager.CopyFromDB(paper);
                            PaperManager.AddOrUpdate(tmp);
                        }
                    }
                    catch(Exception ex)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(ex.ToString());
                        sb.AppendLine("id:" + paper.Id);
                        sb.AppendLine(string.Format("title:{0}", paper.Title));
                        sb.AppendLine(string.Format("status:{0}", paper.Status));
                        sb.AppendLine(string.Format("author:{0}", paper.Author));
                        sb.AppendLine(string.Format("uuid:{0}", paper.AccountEmail_uuid));
                        sb.AppendLine(string.Format("postmagazine:{0}", paper.PostMagazine));
                        LogHelper.LogErrorAsync(typeof(Service1), ex);
                        LogHelper.LogInfoAsync(typeof(Service1), sb.ToString());
                    }
                }
            }
        }

        private void ProcessProfessors()
        {
            using (UserRepository repo = new UserRepository())
            {
                foreach(var u in repo.GetAllUserInfo())
                {
                    try
                    {
                        ProfessorIndex index = ProfessorManager.GenObject(u.uuid);
                        if(index!=null)
                            ProfessorManager.AddOrUpdate(index);
                    }
                    catch(Exception ex)
                    {
                        LogHelper.LogErrorAsync(typeof(Service1), ex);
                    }
                }
            }
        }
    }
}
