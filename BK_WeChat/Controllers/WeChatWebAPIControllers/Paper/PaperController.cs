using BK.CommonLib.DB.Redis;
using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.ElasticSearch;
using BK.CommonLib.MQ;
using BK.CommonLib.Util;
using BK.Model.Configuration.Redis;
using BK.Model.DB;
using BK.Model.Index;
using BK.Model.Redis.Objects.paper;
using BK.WeChat.Controllers.Base;
using BK.WeChat.Controllers.WeChatWebAPIControllers.Find;
using BK.WeChat.Controllers.WeChatWebAPIParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace BK.WeChat.Controllers.WeChatWebAPIControllers.Paper
{
    public class PaperController : ApiController
    {
        [Route("api/Paper/AddComment")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostEkCommentAdd([FromBody]EKCommentParameter postParameter)
        {
            string openid = postParameter.openID;
            long ekid = postParameter.Id;
            string content = postParameter.Content;
            if (string.IsNullOrEmpty(openid) || ekid == 0 || string.IsNullOrEmpty(content))
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using (UserRepository userR = new UserRepository())
            {
                var userinfo = await userR.GetUserInfoByOpenid(openid);
                if (PaperCommentMQOp.SendMessage(userinfo.uuid.ToString(), ekid, content))
                {
                    await PaperManager.AddCommentCountAsync(ekid);//评论数增加
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, "OK", HttpStatusCode.OK, customStatus.Success);
                }

                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, "Failed", HttpStatusCode.OK, customStatus.Fail);
            }
        }

        [Route("api/Paper/GetComments")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostPaperCommentGet([FromBody]EKCommentParameter postParameter)
        {
            string openid = postParameter.openID;
            long ekid = postParameter.Id;
            int secNo = postParameter.SectionNo;
            if (string.IsNullOrEmpty(openid) || ekid == 0)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            List<string> ret = new List<string>();
            ret = await PaperCommentMQOp.GetMessage(ekid, secNo);
            return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, ret, HttpStatusCode.OK, customStatus.Success);
        }

        //获取自己/与别人的的papaer列表。如果自己则传入openid，如果别人则还要传别人的uuid。
        [Route("api/Paper/GetMyPapers")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostGetMyPapers([FromBody]EKCommentParameter postParameter)
        {
            string openid = postParameter.openID;
            if (string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            Guid user = postParameter.uuid;
            if(user.Equals(Guid.Empty))//查自己
            {
                using (UserRepository repo = new UserRepository())
                {
                    var userinfo = await repo.GetUserInfoByOpenid(openid);
                    user = userinfo.uuid;
                }
            }

            var ret = await PaperManager.GetByUserUuid(user);
            if (ret != null && ret.Count > 0)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, ret, HttpStatusCode.OK, customStatus.Success);
            }
            else
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, new List<PapersIndex>(), HttpStatusCode.OK, customStatus.Success);
            }
        }

        [Route("api/Paper/AddPaper")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostAddPaper([FromBody]EKCommentParameter postParameter)
        {
            string openid = postParameter.openID;
            UserArticle userArticle = postParameter.userArticle;
            if (string.IsNullOrEmpty(openid) || userArticle == null)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            bool ret;
            using (UserRepository repo = new UserRepository())
            {
                ret = await repo.AddPaperAsync(userArticle);
                if (ret)
                {
                    var list = await repo.GetPaperByObjectAsync(userArticle);
                    if (list.Count() == 1)
                        userArticle = list[0];
                    else
                    {
                        return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, "错误，出现重复的paper记录。id：" + list[0].Id.ToString(), HttpStatusCode.OK, customStatus.Fail);
                    }
                }
                else
                {
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, "错误，添加paper出现重复的paper记录。", HttpStatusCode.OK, customStatus.Fail);
                }
            }
            PapersIndex p = PaperManager.CopyFromDB(userArticle);
            ret = await PaperManager.AddOrUpdateAsync(p);

            return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, ret, HttpStatusCode.OK, customStatus.Success);
        }

        [Route("api/Paper/UpdatePaper")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostUpdatePaper([FromBody]EKCommentParameter postParameter)
        {
            string openid = postParameter.openID;
            UserArticle userArticle = postParameter.userArticle;
            if (string.IsNullOrEmpty(openid) || userArticle == null ||userArticle.Id<=0)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            bool ret;
            using (UserRepository repo = new UserRepository())
            {
                ret = await repo.UpdatePaperAsync(userArticle);
            }
            PapersIndex p = PaperManager.CopyFromDB(userArticle);
            ret = await PaperManager.AddOrUpdateAsync(p);

            return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, ret, HttpStatusCode.OK, customStatus.Success);
        }

        [Route("api/Paper/GetPaper")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostGetArticleById([FromBody]EKCommentParameter postParameter)
        {
            string openid = postParameter.openID;
            long ekid = postParameter.Id;
            if (string.IsNullOrEmpty(openid) || ekid == 0)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            Guid user;
            using (UserRepository repo = new UserRepository())
            {
                var userinfo = await repo.GetUserInfoByOpenid(openid);
                user = userinfo.uuid;
            }
            var ret = await PaperManager.GetById(ekid);
            if(ret!=null)
            {
                await PaperManager.AddReadCountAsync(ekid);
                await PaperManager.SetReadPeopleAsync(ekid, user, DateTime.Now);
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, ret, HttpStatusCode.OK, customStatus.Success);
            }
            else
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, "此论文不在！", HttpStatusCode.OK, customStatus.Fail);
            }
        }

        /// <summary>
        /// 一次翻pagesize个人的论文。
        /// </summary>
        /// <param name="postParameter"></param>
        /// <returns></returns>
        [Route("api/Paper/GetPapers")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostGetArticles([FromBody]EKCommentParameter postParameter)
        {
            string openid = postParameter.openID;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;
            if (string.IsNullOrEmpty(openid) || pageSize == 0)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            #region 先获取此人相关的教授信息。应该跟“找教授”页面一致
            if (string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            //获取用户uuid 学校与院系与专业信息。
            Guid useruuid; string xuexiaoname, yuanxiname; long? rf;
            using (UserRepository userRepository = new UserRepository())
            {
                var userinfo = await userRepository.GetUserInfoByOpenid(openid);
                useruuid = userinfo.uuid;
                xuexiaoname = userinfo.Unit;
                yuanxiname = userinfo.Faculty;
                rf = userinfo.ResearchFieldId;

                if (string.IsNullOrEmpty(xuexiaoname) || string.IsNullOrEmpty(yuanxiname))
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, "学校，院系有个为空！", HttpStatusCode.OK, customStatus.InvalidArguments);

                //获取score
                double univScore, depScore;
                FindHelper.GetUnivDeptScore(xuexiaoname, yuanxiname, out univScore, out depScore);

                //拿到三个集合
                double rfScore = Convert.ToDouble(rf.Value);
                var set = await FindHelper.GetThreeSet(true, univScore, depScore, rfScore);

                var retUuid = FindHelper.FindProfessorRule(useruuid,set.Item1, set.Item2, set.Item3, postParameter.pageIndex, postParameter.pageSize);

                List<Guid> ret = new List<Guid>();
                foreach (var s in retUuid)
                {
                    ret.Add(Guid.Parse(s));
                }
                #endregion
                var list = await PaperManager.GetPapersAsync(ret);

                List<Tuple<PapersIndex, string, string, bool,string>> retList = new List<Tuple<PapersIndex, string, string, bool,string>>();
                if (list!=null && list.Count()>0)
                {
                    using (UserRepository repo = new UserRepository())
                    {
                        foreach (var paperindex in list)
                        {
                            //获取userinfo
                            var uinfo = await repo.GetUserInfoByUuid(Guid.Parse(paperindex.AccountEmail_uuid));
                            //获取是否赞
                            var redis = new RedisManager2<WeChatRedisConfig>();
                            double score = await redis.GetScoreEveryKeyAsync<PaperRedis, PZanPeopleZsetAttribute>(paperindex.Id, useruuid.ToString());
                            bool iszaned = score > 0;
                            double zanShu = await redis.GetScoreAsync<PaperRedis, PZanCountZsetAttribute>(paperindex.Id);
                            double comShu = await redis.GetScoreAsync<PaperRedis, PCommentCountZsetAttribute>(paperindex.Id);
                            retList.Add(Tuple.Create(paperindex, uinfo.Name, uinfo.Photo, iszaned,zanShu.ToString()+"-"+comShu.ToString()));
                        }
                    }
                }
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, retList, HttpStatusCode.OK, customStatus.Success);
            }
        }

        [Route("api/Paper/AddZan")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostAddZan([FromBody]EKCommentParameter postParameter)
        {
            string openid = postParameter.openID;
            long ekid = postParameter.Id;
            if (string.IsNullOrEmpty(openid) || ekid == 0)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            Guid user;
            using (UserRepository repo = new UserRepository())
            {
                var userinfo = await repo.GetUserInfoByOpenid(openid);
                user = userinfo.uuid;
            }
            var ret = await PaperManager.AddZanCountAsync(ekid);
            //更新赞的人
            await PaperManager.SetZanPeopleAsync(ekid, user, DateTime.Now);
            return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, ret, HttpStatusCode.OK, customStatus.Success);
        }

        [Route("api/Paper/IsZaned")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostIsZaned([FromBody]EKCommentParameter postParameter)
        {
            string openid = postParameter.openID;
            long ekid = postParameter.Id;
            if (string.IsNullOrEmpty(openid) || ekid == 0)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using (UserRepository userR = new UserRepository())
            {
                UserInfo user = await userR.GetUserInfoByOpenid(openid);
                var redis = new RedisManager2<WeChatRedisConfig>();
                double score = await redis.GetScoreEveryKeyAsync<PaperRedis, PZanPeopleZsetAttribute>(ekid.ToString(), user.uuid.ToString());
                if (score == 0)
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, false, HttpStatusCode.OK, customStatus.Success);
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, true, HttpStatusCode.OK, customStatus.Success);
            }
        }

        [Route("api/Paper/GetZanUsers")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostGetZanUsers([FromBody]EKCommentParameter postParameter)
        {
            string openid = postParameter.openID;
            long ekid = postParameter.Id;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;
            int fromNo = pageIndex * pageSize;
            int toNo = fromNo + pageSize;

            if (string.IsNullOrEmpty(openid) || ekid == 0)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using (UserRepository userR = new UserRepository())
            {
                UserInfo user = await userR.GetUserInfoByOpenid(openid);
                var redis = new RedisManager2<WeChatRedisConfig>();

                var zans = await redis.GetRangeByRankAsync<PaperRedis, PZanPeopleZsetAttribute>(ekid.ToString(), from: fromNo, to: toNo);

                List<UserInfo> ret = new List<UserInfo>();
                if (zans != null && zans.Count() > 0)
                {
                    foreach (var v in zans)
                    {
                        UserInfo tmp = await userR.GetUserInfoByUuid(Guid.Parse(v.Key));
                        ret.Add(tmp);
                    }
                }

                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, ret, HttpStatusCode.OK, customStatus.Success);
            }
        }

        [Route("api/Paper/GetReadUsers")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostGetReadUsers([FromBody]EKCommentParameter postParameter)
        {
            string openid = postParameter.openID;
            long ekid = postParameter.Id;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;
            int fromNo = pageIndex * pageSize;
            int toNo = fromNo + pageSize;

            if (string.IsNullOrEmpty(openid) || ekid == 0)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using (UserRepository userR = new UserRepository())
            {
                UserInfo user = await userR.GetUserInfoByOpenid(openid);
                var redis = new RedisManager2<WeChatRedisConfig>();

                var zans = await redis.GetRangeByRankAsync<PaperRedis, PReadPeopleZsetAttribute>(ekid.ToString(), from: fromNo, to: toNo);

                List<UserInfo> ret = new List<UserInfo>();
                if (zans != null && zans.Count() > 0)
                {
                    foreach (var v in zans)
                    {
                        UserInfo tmp = await userR.GetUserInfoByUuid(Guid.Parse(v.Key));
                        ret.Add(tmp);
                    }
                }

                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, ret, HttpStatusCode.OK, customStatus.Success);
            }
        }

        [Route("api/Paper/GetAllCount")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostGetAllCount([FromBody]EKCommentParameter postParameter)
        {
            string openid = postParameter.openID;
            long ekid = postParameter.Id;
            if (string.IsNullOrEmpty(openid) || ekid == 0)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            var redis = new RedisManager2<WeChatRedisConfig>();
            double zanCount = await redis.GetScoreAsync<PaperRedis, PZanCountZsetAttribute>(ekid.ToString());
            double readCount = await redis.GetScoreAsync<PaperRedis, PReadCountZsetAttribute>(ekid.ToString());
            double commentCount = await redis.GetScoreAsync<PaperRedis, PCommentCountZsetAttribute>(ekid.ToString());

            Tuple<double, double, double> ret = Tuple.Create(zanCount, readCount, commentCount);
            return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, ret, HttpStatusCode.OK, customStatus.Success);

        }

        [Route("api/Paper/Delete")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostDelete([FromBody]EKCommentParameter postParameter)
        {
            string openid = postParameter.openID;
            long ekid = postParameter.Id;
            if (string.IsNullOrEmpty(openid) || ekid == 0)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            bool flag = false;
            using (UserRepository repo = new UserRepository())
            {
                flag = await repo.DeletePaperAsync(ekid);
                if (flag)
                    flag = await PaperManager.DeleteAsync(ekid);
            }

            return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, flag, HttpStatusCode.OK, customStatus.Success);

        }

        [Route("api/Paper/Search")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostSearch([FromBody]EKCommentParameter postParameter)
        {
            string openid = postParameter.openID;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;
            string queryStr = postParameter.Content;

            if (string.IsNullOrEmpty(openid) || pageSize == 0 || string.IsNullOrEmpty(queryStr))
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            var ret = await PaperManager.Search3(queryStr, pageIndex, pageSize);
            List<Tuple<PapersIndex, string, string, bool, string>> retList = new List<Tuple<PapersIndex, string, string, bool, string>>();
            if (ret != null && ret.Count() > 0)
            {
                using (UserRepository repo = new UserRepository())
                {
                    var myUUID = (await repo.GetUserInfoByOpenid(openid)).uuid;
                    foreach (var paperindex in ret)
                    {
                        //获取userinfo
                        var uinfo = await repo.GetUserInfoByUuid(Guid.Parse(paperindex.AccountEmail_uuid));
                        //获取是否赞
                        var redis = new RedisManager2<WeChatRedisConfig>();
                        double score = await redis.GetScoreEveryKeyAsync<PaperRedis, PZanPeopleZsetAttribute>(paperindex.Id, myUUID.ToString());
                        bool iszaned = score > 0;
                        double zanShu = await redis.GetScoreAsync<PaperRedis, PZanCountZsetAttribute>(paperindex.Id);
                        double comShu = await redis.GetScoreAsync<PaperRedis, PCommentCountZsetAttribute>(paperindex.Id);
                        retList.Add(Tuple.Create(paperindex, uinfo.Name, uinfo.Photo, iszaned, zanShu.ToString() + "-" + comShu.ToString()));
                    }
                }
            }
            return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, retList, HttpStatusCode.OK, customStatus.Success);
        }
    }
}