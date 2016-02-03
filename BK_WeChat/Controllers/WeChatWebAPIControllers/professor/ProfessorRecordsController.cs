using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.Util;
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
using BK.Model.DB;

namespace BK.WeChat.Controllers.WeChatWebAPIControllers
{
    public class ProfessorRecordsController : ApiController
    {
        private UserInfo userinfo;

        /// <summary>
        /// 教授用户资料页 页面初始化。教授页面头部初始化
        /// post api/MyIndex/Initialize
        /// </summary>
        /// <param name="postParameter">openid:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 账号不存在
        /// Success 成功
        /// </returns>
        [Route("api/ProfessorRecord/Initialize")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitialize([FromBody]LoginParameter postParameter)
        {
            string openid = postParameter.openID;
            if (string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using (UserRepository userRepository = new UserRepository())
            {
                userinfo = await userRepository.GetUserInfoByOpenid(openid);
                if (userinfo == null)
                {
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.NotFound);
                }
                userinfo.NumOfContacts = await userRepository.GetUserContactNumber(userinfo.uuid);
                userinfo.NumOfVisitor = await userRepository.GetUserBeenToNumber(userinfo.uuid);
                userinfo.NumOfFavorite = await userRepository.GetuserFavoriteNumber(userinfo.uuid);
                using (SystemRepository systemRepository = new SystemRepository())
                {
                    userinfo.Hometown = await systemRepository.GetShortAddress(userinfo.HometownProvince, userinfo.HometownCity);
                }
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, userinfo, HttpStatusCode.OK, customStatus.Success);
            }
        }


        /// <summary>
        /// 教授初始化
        /// post api/MyRecords/InitProfessorRecords
        /// </summary>
        /// <param name="postParameter">openid:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 账号不存在
        /// Success 成功
        /// researchField研究领域  userAcadmics学术地位 项目资助 教育经历 工作经历 学术论文 专利 
        /// </returns>
        [Route("api/Professor/InitProfessorRecords")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeProfessorRecords([FromBody]BaseParameter postParameter)
        {
            string openid = postParameter.openID;
            if (string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            using (UserRepository userRepository = new UserRepository())
            {
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);
                if (userUuid == Guid.Empty)
                {
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.NotFound);
                }
                else
                {
                    Dictionary<string, object> tempResult = new Dictionary<string, object>();

                    UserInfo userinfo = await userRepository.GetUserInfoByOpenid(openid);
                    //研究兴趣
                    string researchField = userinfo.Interests;
                    tempResult.Add("yjly", researchField);


                    //学术地位
                    List<UserAcademic> userAcademicList = await userRepository.GetUserRecords(new UserAcademic { AccountEmail_uuid = userUuid });
                    List<string> acadmeicList =  RepositoryHelper.ConvertUserAcademicToString(userAcademicList);
                    tempResult.Add("xsdw", Tuple.Create("UserAcademic", acadmeicList));

                    //项目资助
                    List<UserAwards> userAwardList = await userRepository.GetUserRecords(new UserAwards() { AccountEmail_uuid = userUuid });

                    tempResult.Add("zzxm", Tuple.Create("UserAwards", userAwardList));

                    //教育经历
                    List<UserEducation> userEducationList = await userRepository.GetUserRecords(new UserEducation() { AccountEmail_uuid = userUuid });
                    tempResult.Add("jyjl", Tuple.Create("UserEducation", userEducationList));

                    //工作经历
                    List<UserExperience> userExperienceList = await userRepository.GetUserRecords(new UserExperience() { AccountEmail_uuid = userUuid });
                    tempResult.Add("gzjl", Tuple.Create("UserExperience", userExperienceList));

                    //论文数
                    int userArticleNumber = await userRepository.GetUserRecordsNumber(new UserArticle() { AccountEmail_uuid = userUuid });
                    tempResult.Add("lws", Tuple.Create("UserArticle", userArticleNumber));

                    //专利数
                    int userPatendNumber = await userRepository.GetUserRecordsNumber(new UserPatent() { AccountEmail_uuid = userUuid });
                    tempResult.Add("zls", Tuple.Create("UserPatent", userPatendNumber));

                    //访问过我的人还访问过谁
                    int visitorsBeenToNumber = await userRepository.GetVisitorBeenToNumber(userUuid);
                    List<UserInfo> visitorsBeenTo = await userRepository.GetVisitorBeenTo(userUuid, 6);

                    tempResult.Add("visitorsBeenToNumber", visitorsBeenToNumber);
                    tempResult.Add("visitorsBeenToTopSix", visitorsBeenTo);
                    return WebApiHelper.HttpRMtoJson(tempResult, HttpStatusCode.OK, customStatus.Success);
                }

            }
        }
        #region 8大项
        /// <summary>
        /// 返回8大项的通用方法,根据对象查出此对象的列表。必须传入对象的id与对象的名称。根据openid来查。
        /// </summary>
        /// <param name="postParameter">openid: id: UserXXXXX:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// message: UserXXXX的对象
        /// </returns>
        [Route("api/Professor/GetObjects")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostGetObjects([FromBody]UserRecordsParameter postParameter)
        {
            string openid = postParameter.openID;
            string typeid = postParameter.typeid;

            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;
            if (string.IsNullOrEmpty(openid) || string.IsNullOrEmpty(typeid) || RecordFactory.GetTypeById(typeid)== null)
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            
            using (UserRepository userRepository = new UserRepository())
            {
                //获取uuid
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);
                if(userUuid.Equals(Guid.Empty))
                    return WebApiHelper.HttpRMtoJson("Error:uuid is empty", HttpStatusCode.OK, customStatus.InvalidArguments);

                //获取对象
                Type t = RecordFactory.GetTypeById(typeid);
                if(t.GetInterface("IDBModelWithID")== null)
                {
                    return WebApiHelper.HttpRMtoJson("Error:类型错误："+"type:"+t.ToString()+ "。不是IDBModelWithID的接口！", HttpStatusCode.OK, customStatus.InvalidArguments);
                }
                dynamic input = t.Assembly.CreateInstance(t.FullName) as IDBModelWithID;
                input.AccountEmail_uuid = userUuid;

                if (input.Id != 0)
                {
                    input = await userRepository.GetUserRecordsById(input);
                }
                else if (pageIndex != 0 && pageSize != 0)
                {
                    input = await userRepository.GetUserRecords(input, pageIndex, pageSize);
                }
                else
                {
                    input = await userRepository.GetUserRecords(input);
                }

                return WebApiHelper.HttpRMtoJson(input, HttpStatusCode.OK, customStatus.Success);
            }
        }

        /// <summary>
        /// 返回8大项的通用方法,根据对象查出此对象的列表。必须传入对象的id与对象的名称。
        /// </summary>
        /// <param name="postParameter">openid: id: UserXXXXX:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// message: UserXXXX的对象
        /// </returns>
        [Route("api/Professor/GetObjectsByUuid")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostGetObjectsByUUid([FromBody]UserRecordsParameter postParameter)
        {
            Guid uuid = postParameter.uuid;
            string typeid = postParameter.typeid;

            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;
            if (uuid.Equals(Guid.Empty) || string.IsNullOrEmpty(typeid) || RecordFactory.GetTypeById(typeid) == null)
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using (UserRepository userRepository = new UserRepository())
            {
                //获取uuid
                var userUuid = uuid;
                if (userUuid.Equals(Guid.Empty))
                    return WebApiHelper.HttpRMtoJson("Error:uuid is empty", HttpStatusCode.OK, customStatus.InvalidArguments);

                //获取对象
                Type t = RecordFactory.GetTypeById(typeid);
                if (t.GetInterface("IDBModelWithID") == null)
                {
                    return WebApiHelper.HttpRMtoJson("Error:类型错误：" + "type:" + t.ToString() + "。不是IDBModelWithID的接口！", HttpStatusCode.OK, customStatus.InvalidArguments);
                }
                dynamic input = t.Assembly.CreateInstance(t.FullName) as IDBModelWithID;
                input.AccountEmail_uuid = userUuid;

                if (input.Id != 0)
                {
                    input = await userRepository.GetUserRecordsById(input);
                }
                else if (pageIndex != 0 && pageSize != 0)
                {
                    input = await userRepository.GetUserRecords(input, pageIndex, pageSize);
                }
                else
                {
                    input = await userRepository.GetUserRecords(input);
                }

                return WebApiHelper.HttpRMtoJson(input, HttpStatusCode.OK, customStatus.Success);
            }
        }

        /// <summary>
        /// 修改8大项
        /// </summary>
        /// <param name="postParameter">openid: [UserXXXXX:]</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 账号不存在
        /// Forbidden禁止修改
        /// Success 成功
        /// </returns>
        [Route("api/Professor/SaveOrUpdateRecords")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostSubmitUserRecords([FromBody]UserRecordsParameter postParameter)
        {
            string openid = postParameter.openID;
            if (string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            using (UserRepository userRepository = new UserRepository())
            {
                dynamic input = null;
                Guid uuid = await userRepository.GetUserUuidByOpenid(openid);
                if (uuid == Guid.Empty)
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);
                foreach (System.Reflection.PropertyInfo pi in postParameter.GetType().GetProperties())
                {
                    if (pi.PropertyType.BaseType.Equals(typeof(DBModelBase)) && pi.GetValue(postParameter) != null)
                    {
                        input = Convert.ChangeType(pi.GetValue(postParameter), pi.PropertyType);
                        break;
                    }
                }
                if (input == null)
                {
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
                }

                if (uuid != input.AccountEmail_uuid)
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Forbidden);

                if (await userRepository.SaveUserRecordsById(input))
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Success);
                else
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Fail);

            }


        }
        /// <summary>
        /// 删除8大项
        /// </summary>
        /// <param name="postParameter">openid: UserXXXXX:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// </returns>
        [Route("api/Professor/DeleteRecordsById")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostDeleteUserRecordsById([FromBody]UserRecordsParameter postParameter)
        {
            string openid = postParameter.openID;
            if (string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using (UserRepository userRepository = new UserRepository())
            {
                dynamic input = null;
                Guid uuid = await userRepository.GetUserUuidByOpenid(openid);
                if (uuid == Guid.Empty)
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);

                foreach (System.Reflection.PropertyInfo pi in postParameter.GetType().GetProperties())
                {
                    if (pi.PropertyType.BaseType.Equals(typeof(DBModelBase)) && pi.GetValue(postParameter) != null)
                    {
                        input = Convert.ChangeType(pi.GetValue(postParameter), pi.PropertyType);
                        break;
                    }
                }
                if (input == null)
                {
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
                }
                if (uuid != input.AccountEmail_uuid)
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Forbidden);

                if (await userRepository.DeleteUserRecordsById(input))
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Success);
                else
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Fail);
            }
        }
        #endregion

        [Route("api/Professor/UpdateInterestsField")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostUpdateResearchField([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            
            if (string.IsNullOrEmpty(openid) || string.IsNullOrEmpty(postParameter.textMsg))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            using (UserRepository userRepository = new UserRepository())
            {
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);
                if (userUuid == Guid.Empty)
                {
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.NotFound);
                }
                else
                {
                    UserInfo userinfo = await userRepository.GetUserInfoByOpenid(openid);

                    //更新研究兴趣
                    userinfo.Interests = postParameter.textMsg;
                    bool flag = await userRepository.SaveUserInfo(userinfo);
                    if(flag)
                        return WebApiHelper.HttpRMtoJson(flag, HttpStatusCode.OK, customStatus.Success);
                    else
                        return WebApiHelper.HttpRMtoJson("没有保存成功", HttpStatusCode.OK, customStatus.Fail);
                }
            }
        }

        [Route("api/Professor/GetInterestsField")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostGetInterests([FromBody]BaseParameter postParameter)
        {
            string openid = postParameter.openID;
            if (string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using (UserRepository userRepository = new UserRepository())
            {
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);
                if (userUuid == Guid.Empty)
                {
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.NotFound);
                }
                else
                {
                    UserInfo userinfo = await userRepository.GetUserInfoByOpenid(openid);
                    //研究兴趣
                    string researchField = userinfo.Interests;
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, researchField, HttpStatusCode.OK, customStatus.Success);
                }
            }
        }
    }
}