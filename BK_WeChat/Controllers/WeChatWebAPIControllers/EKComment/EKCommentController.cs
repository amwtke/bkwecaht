using BK.CommonLib.DB.Redis;
using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.ElasticSearch;
using BK.CommonLib.MQ;
using BK.CommonLib.Util;
using BK.Model.Configuration.Redis;
using BK.Model.DB;
using BK.Model.Redis.Objects.EK;
using BK.WeChat.Controllers.Base;
using BK.WeChat.Controllers.WeChatWebAPIParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace BK.WeChat.Controllers.EKComment
{
    public class EKCommentController : ApiController
    {
        [Route("api/EK/AddComment")]
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
                if (EKCommentMessageMQOp.SendMessage(userinfo.uuid.ToString(), ekid, content))
                {
                    await EKArticleManager.AddCommentCountAsync(ekid);//评论数增加
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, "OK", HttpStatusCode.OK, customStatus.Success);
                }

                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, "Failed", HttpStatusCode.OK, customStatus.Fail);
            }
        }

        [Route("api/EK/GetComments")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostEkCommentGet([FromBody]EKCommentParameter postParameter)
        {
            string openid = postParameter.openID;
            long ekid = postParameter.Id;
            int secNo = postParameter.SectionNo;
            if (string.IsNullOrEmpty(openid) || ekid == 0)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            List<string> ret = new List<string>();
            ret = await EKCommentMessageMQOp.GetMessage(ekid, secNo);
            return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, ret, HttpStatusCode.OK, customStatus.Success);
        }
        [Route("api/EK/GetCommentCount")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostGetCommentCount([FromBody]EKCommentParameter postParameter)
        {
            string openid = postParameter.openID;
            long ekid = postParameter.Id;

            if (string.IsNullOrEmpty(openid) || ekid == 0)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            double count = await EKArticleManager.GetCommentCountAsync(ekid);//评论数增加
            return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, count, HttpStatusCode.OK, customStatus.Success);
        }

        [Route("api/EK/GetArticle")]
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
            var ret = await EKArticleManager.GetById(ekid);
            //将文章插图的地址转义
            ret.BodyText = WebApiHelper.GetEscapedBodyText(ret);
            await EKArticleManager.AddReadCountAsync(ekid);
            await EKArticleManager.SetReadPeopleAsync(ekid, user, DateTime.Now);
            return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, ret, HttpStatusCode.OK, customStatus.Success);
        }

        [Route("api/EK/GetArticles")]
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

            var ret = await EKArticleManager.GetArticlesAsync(pageIndex, pageSize);
            //将文章标题图添加云存储的域名
            foreach(var r in ret)
                r.HeadPic = WebApiHelper.GetQiniuEKArticleHeadPic(r);
            return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, ret, HttpStatusCode.OK, customStatus.Success);
        }

        [Route("api/EK/Search")]
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

            var ret = await EKArticleManager.Search(queryStr, pageIndex, pageSize);
            //将文章标题图添加云存储的域名
            foreach (var r in ret)
                r.HeadPic = WebApiHelper.GetQiniuEKArticleHeadPic(r);
            return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, ret, HttpStatusCode.OK, customStatus.Success);
        }

        [Route("api/EK/AddZan")]
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
            var ret = await EKArticleManager.AddZanAsync(ekid);
            //更新赞的人
            await EKArticleManager.SetZanPeopleAsync(ekid, user, DateTime.Now);
            return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, ret, HttpStatusCode.OK, customStatus.Success);
        }

        [Route("api/EK/IsZaned")]
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
                double score = await redis.GetScoreEveryKeyAsync<EKTodayRedis, EKZanPepleZsetAttribute>(ekid.ToString(), user.uuid.ToString());
                if(score==0)
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, false, HttpStatusCode.OK, customStatus.Success);
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, true, HttpStatusCode.OK, customStatus.Success);
            }
        }

        [Route("api/EK/GetZanUsers")]
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

                var zans = await redis.GetRangeByRankAsync<EKTodayRedis, EKZanPepleZsetAttribute>(ekid.ToString(), from: fromNo, to: toNo);

                List<UserInfo> ret = new List<UserInfo>();
                if (zans != null && zans.Count() > 0)
                {
                    foreach(var v in zans)
                    {
                        UserInfo tmp = await userR.GetUserInfoByUuid(Guid.Parse(v.Key));
                        ret.Add(tmp);
                    }
                }
               
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, ret, HttpStatusCode.OK, customStatus.Success);
            }
        }

        [Route("api/EK/GetReadUsers")]
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

                var zans = await redis.GetRangeByRankAsync<EKTodayRedis, EKReadPepleZsetAttribute>(ekid.ToString(), from: fromNo, to: toNo);

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

        [Route("api/EK/GetAllCount")]
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
            double zanCount = await redis.GetScoreAsync<EKTodayRedis, EKZanCountZsetAttribute>(ekid.ToString());
            double readCount = await redis.GetScoreAsync<EKTodayRedis, EKReadCountZsetAttribute>(ekid.ToString());
            double commentCount = await redis.GetScoreAsync<EKTodayRedis, EKCommentCountZsetAttribute>(ekid.ToString());

            Tuple<double, double, double> ret = Tuple.Create(zanCount, readCount, commentCount);
            return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, ret, HttpStatusCode.OK, customStatus.Success);

        }

    }
}