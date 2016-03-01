using BK.CommonLib.DB.Redis;
using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.ElasticSearch;
using BK.CommonLib.Util;
using BK.Model.Configuration.Redis;
using BK.Model.DB;
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

namespace BK.WeChat.Controllers.WeChatWebAPIControllers.Find
{
    public class FindPepoleController : ApiController
    {
        [Route("api/Find/Professors")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostFindProfessors([FromBody]BaseParameter postParameter)
        {
            string openid = postParameter.openID;
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
                    //return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, "学校，院系名称不正规！", HttpStatusCode.OK, customStatus.InvalidArguments);

                //拿到三个集合
                double rfScore = Convert.ToDouble(rf.Value);
                var set = await FindHelper.GetThreeSet(true, univScore, depScore, rfScore);

                var retUuid = FindHelper.FindProfessorRule(useruuid,set.Item1, set.Item2, set.Item3, postParameter.pageIndex, postParameter.pageSize);

                List<Tuple<UserInfo, string, bool>> ret = new List<Tuple<UserInfo, string, bool>>();
                if (retUuid != null && retUuid.Count > 0)
                {
                    using (UserRepository repo = new UserRepository())
                    {
                        List<string> eduList = await ProfessorManager.GetUserEducations(userinfo.uuid);
                        var list = await ProfessorManager.Search_UUid(retUuid);
                        foreach (var v in list)
                        {
                            Guid uuid = Guid.Parse(v.Id);
                            var tempUserinfo = await repo.GetUserInfoByUuidAsync(uuid);
                            string three = !string.IsNullOrEmpty(v.Diwei) ? v.Diwei.Trim() : "";
                            bool isXY = ProfessorManager.IsXiaoYou(eduList, v.Education);
                            ret.Add(Tuple.Create(tempUserinfo, three, isXY));
                        }
                    }
                }
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, ret, HttpStatusCode.OK, customStatus.Success);
            }
        }

        [Route("api/Find/Students")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostFindStudents([FromBody]BaseParameter postParameter)
        {
            string openid = postParameter.openID;
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
                if (!FindHelper.GetUnivDeptScore(xuexiaoname, yuanxiname, out univScore, out depScore))
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, "学校，院系名称不正规！", HttpStatusCode.OK, customStatus.InvalidArguments);

                //拿到三个集合
                double rfScore = Convert.ToDouble(rf.Value);
                var set = await FindHelper.GetThreeSet(false, univScore, depScore, rfScore);

                var retUuid = await FindHelper.FindStudtenRule(useruuid.ToString().ToUpper(), set.Item1, set.Item2, set.Item3, postParameter.pageIndex, postParameter.pageSize);
                
                List<UserInfo> ret = new List<UserInfo>();
                foreach (var s in retUuid)
                {
                    var v = await userRepository.GetUserInfoByUuidAsync(Guid.Parse(s));

                    if (v != null)
                        ret.Add(v);

                    //if (v.ResearchFieldId == 0)
                    //    v.SubResearchFieldId = 120513;//未知
                }

                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, ret, HttpStatusCode.OK, customStatus.Success);
            }
        }

        [Route("api/Find/StudentFindProfessors")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostFindProfessorsFroStudents([FromBody]BaseParameter postParameter)
        {
            string openid = postParameter.openID;
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
                //return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, "学校，院系名称不正规！", HttpStatusCode.OK, customStatus.InvalidArguments);

                //拿到三个集合
                double rfScore = Convert.ToDouble(rf.Value);
                var set = await FindHelper.GetThreeSet(true, univScore, depScore, rfScore);

                var retUuid = await FindHelper.FindProfessorRuleForStudent(set.Item1, set.Item2, set.Item3, postParameter.pageIndex, postParameter.pageSize);

                List<Tuple<UserInfo, string, bool>> ret = new List<Tuple<UserInfo, string, bool>>();
                if (retUuid != null && retUuid.Count > 0)
                {
                    using (UserRepository repo = new UserRepository())
                    {
                        List<string> eduList = await ProfessorManager.GetUserEducations(userinfo.uuid);
                        var list = await ProfessorManager.Search_UUid(retUuid);
                        foreach (var v in list)
                        {
                            Guid uuid = Guid.Parse(v.Id);
                            var tempUserinfo = await repo.GetUserInfoByUuidAsync(uuid);
                            string three = !string.IsNullOrEmpty(v.Diwei) ? v.Diwei.Trim() : "";
                            bool isXY = ProfessorManager.IsXiaoYou(eduList, v.Education);
                            ret.Add(Tuple.Create(tempUserinfo, three, isXY));
                        }
                    }
                }
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, ret, HttpStatusCode.OK, customStatus.Success);
            }
        }
    }
}