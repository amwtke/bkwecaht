using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.ElasticSearch;
using BK.CommonLib.Util;
using BK.Model.DB;
using BK.Model.Index;
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

namespace BK.WeChat.Controllers.WeChatWebAPIControllers.professor
{
    public class ProfessorSearchController : ApiController
    {
        [Route("api/Professor/Searchrf")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostSearchrf([FromBody]ProfessorSearchParameter postParameter)
        {
            string openid = postParameter.openID;
            int diwei = string.IsNullOrEmpty(postParameter.labels) ? 0 : ProfessorManager.GetDiweiScore(postParameter.labels);
            bool xiaoyou = postParameter.xiaoyou != null ? postParameter.xiaoyou.Value : false;
            string danwei = postParameter.danwei;

            int index = postParameter.pageIndex;
            int size = postParameter.pageSize;

            if (string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            Guid Myuuid;
            using (UserRepository repo = new UserRepository())
            {
                Myuuid = (await repo.GetUserInfoByOpenid(openid)).uuid;
            }


            var list = await ProfessorManager.Search_rf3(Myuuid, xiaoyou, danwei, diwei, postParameter.address, postParameter.pageIndex, postParameter.pageSize);

            List<Tuple<UserInfo, string, bool>> ret = new List<Tuple<UserInfo, string, bool>>();
            if (list != null && list.Count > 0)
            {
                using (UserRepository repo = new UserRepository())
                {
                    List<string> eduList = await ProfessorManager.GetUserEducations(Myuuid);
                    foreach (var v in list)
                    {
                        Guid uuid = Guid.Parse(v.Id);
                        var userinfo = await repo.GetUserInfoByUuid(uuid);
                        string three = !string.IsNullOrEmpty(v.Diwei) ? v.Diwei.Trim() : "";
                        bool isXY = ProfessorManager.IsXiaoYou(eduList, v.Education);
                        ret.Add(Tuple.Create(userinfo, three, isXY));
                    }
                }
            }
            return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, ret, HttpStatusCode.OK, customStatus.Success);
        }

        [Route("api/Professor/Search")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostSearch([FromBody]EKCommentParameter postParameter)
        {
            string openid = postParameter.openID;
            string queryStr = postParameter.Content;
            int index = postParameter.pageIndex;
            int size = postParameter.pageSize;

            if (string.IsNullOrEmpty(openid) || string.IsNullOrEmpty(queryStr))
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            UserInfo myUserInfo = null;
            using (UserRepository repo = new UserRepository())
            {
                myUserInfo = await repo.GetUserInfoByOpenid(openid);
            }
                List<Tuple<UserInfo, string, bool>> ret = new List<Tuple<UserInfo, string, bool>>();
            var list = await ProfessorManager.Search(myUserInfo.uuid,queryStr, index, size);
            if (list != null && list.Count > 0)
            {
                
                using (UserRepository repo = new UserRepository())
                {
                    List<string> eduList = await ProfessorManager.GetUserEducations(myUserInfo.uuid);
                    foreach (var v in list)
                    {
                        Guid uuid = Guid.Parse(v.Id);
                        var userinfo = await repo.GetUserInfoByUuid(uuid);

                        string three = !string.IsNullOrEmpty(v.Diwei) ? v.Diwei.Trim() : "";
                        bool isXY = ProfessorManager.IsXiaoYou(eduList, v.Education);
                        ret.Add(Tuple.Create(userinfo, three, isXY));
                    }
                }
            }
            return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, ret, HttpStatusCode.OK, customStatus.Success);
        }
    }
}