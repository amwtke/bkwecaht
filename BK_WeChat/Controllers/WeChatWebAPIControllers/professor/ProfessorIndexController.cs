using BK.CommonLib.DB.Redis;
using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.Util;
using BK.Model.DB;
using BK.WeChat.Controllers.Base;
using BK.WeChat.Controllers.Filters;
using BK.WeChat.Controllers.WeChatWebAPIParameters;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace BK.WeChat.Controllers.WeChatWebAPIControllers.professor
{
    public class ProfessorIndexController : ApiController
    {
        UserInfo userinfo;

        /// <summary>
        /// 教授-访客名片初始化-头部
        /// post api/UserIndex/Initialize
        /// </summary>
        /// <param name="postParameter">openid: uuid:被查看的人的uuid</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 账号不存在
        /// Success 成功
        /// </returns>
        [Route("api/ProfessorIndex/Initialize")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitialize([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            Guid uuid = postParameter.uuid;
            if (string.IsNullOrEmpty(openid) && uuid == Guid.Empty)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using (UserRepository userRepository = new UserRepository())
            {
                userinfo = await userRepository.GetUserInfoByUuidAsync(uuid);
                if (userinfo == null)
                {
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.NotFound);
                }
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);

                VisitBetweenUser newVisitor = new VisitBetweenUser() { UserGuest_uuid = userUuid, UserHost_uuid = uuid, VisitTime = DateTime.Now };
                await userRepository.AddVisitBetweenUser(newVisitor);

                userinfo.NumOfContacts = await userRepository.GetUserContactNumber(uuid);
                userinfo.NumOfVisitor = await userRepository.GetUserVisitorNumber(uuid);
                userinfo.NumOfFavorite = await userRepository.GetuserFavoriteNumber(uuid);
                userinfo.IsContact = await userRepository.IsUserContact(userUuid, uuid);
                userinfo.IsFavorite = (await userRepository.IsUserFavorite(userUuid, uuid)).ToString();
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, userinfo, HttpStatusCode.OK, customStatus.Success);
            }
        }


        /// <summary>
        /// 教授访客名片页-页面初始化-body部分(带uuid,后台统计访客信息要用!!!)
        /// openid为自己的，uuid是访客的。
        /// post api/UserIndex/InitializeUserIndexRecords
        /// </summary>
        /// <param name="postParameter">openid: uuid:对方uuid</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 账号不存在
        /// Success 成功
        /// </returns>
        [Route("api/ProfessorIndex/InitializeRecords")]
        [UserBehaviorFilter]
        [NameCardWebApiFilter]
        public async Task<HttpResponseMessage> PostInitializeMyRecords([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            if (string.IsNullOrEmpty(openid) || Guid.Empty.Equals(postParameter.uuid))
            {
                return WebApiHelper.HttpRMtoJson("必须同时传入openid与uuid", HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            using (UserRepository userRepository = new UserRepository())
            {
                var userUuid = postParameter.uuid;
                if (userUuid == Guid.Empty)
                {
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.NotFound);
                }
                else
                {
                    UserInfo userinfo = await userRepository.GetUserInfoByUuidAsync(userUuid);
                    //研究兴趣
                    string Interests = userinfo.Interests;

                    //学术地位
                    List<UserAcademic> userAcademicList = await userRepository.GetUserRecords(new UserAcademic { AccountEmail_uuid = userUuid });
                    List<string> acadmeicList = RepositoryHelper.ConvertUserAcademicToString(userAcademicList);

                    //项目资助
                    List<UserAwards> userAwardList = await userRepository.GetUserRecords(new UserAwards() { AccountEmail_uuid = userUuid });
                    //教育经历
                    List<UserEducation> userEducationList = await userRepository.GetUserRecords(new UserEducation() { AccountEmail_uuid = userUuid });

                    //工作经历
                    List<UserExperience> userExperienceList = await userRepository.GetUserRecords(new UserExperience() { AccountEmail_uuid = userUuid });

                    //论文数
                    int userArticleNumber = await userRepository.GetUserRecordsNumber(new UserArticle() { AccountEmail_uuid = userUuid });

                    //专利数
                    int userPatendNumber = await userRepository.GetUserRecordsNumber(new UserPatent() { AccountEmail_uuid = userUuid });

                    //访问过我的人还访问过谁
                    int visitorsBeenToNumber = await userRepository.GetVisitorBeenToNumber(userUuid);
                    List<UserInfo> visitorsBeenTo = await userRepository.GetVisitorBeenTo(userUuid, 6);

                    Dictionary<string, object> tempResult = new Dictionary<string, object>();

                    tempResult.Add("Interests", Interests);
                    tempResult.Add("acadmeicList", acadmeicList);
                    tempResult.Add("userAwardList", userAwardList);
                    tempResult.Add("userEducationList", userEducationList);
                    tempResult.Add("userExperienceList", userExperienceList);
                    tempResult.Add("userArticleNumber", userArticleNumber);
                    tempResult.Add("userPatendNumber", userPatendNumber);
                    tempResult.Add("visitorsBeenToNumber", visitorsBeenToNumber);
                    tempResult.Add("visitorsBeenToTopSix", visitorsBeenTo);
                    return WebApiHelper.HttpRMtoJson(tempResult, HttpStatusCode.OK, customStatus.Success);
                }

            }
        }

        /// <summary>
        /// 名片页 页面初始化 用户好友列表
        /// post api/UserIndex/InitializeUserContact
        /// </summary>
        /// <param name="postParameter">openid: uuid: pageIndex: pageSize:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// </returns>
        [Route("api/ProfessorIndex/InitializeUserContact")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeuserContact([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;
            Guid uuid = postParameter.uuid;
            if (string.IsNullOrEmpty(openid) && uuid == Guid.Empty)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            using (UserRepository userRepository = new UserRepository())
            {
                var uclist = await userRepository.GetUserContact(uuid, pageIndex, pageSize);
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, uclist, HttpStatusCode.OK, customStatus.Success);
            }
        }
        /// <summary>
        /// 添加好友
        /// post api/UserIndex/AddContact
        /// </summary>
        /// <param name="postParameter">openid:自己 uuid:对方</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// AccountExist 已存在
        /// Success 成功
        /// </returns>
        [Route("api/ProfessorIndex/AddContact")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostAddContact([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            Guid uuid = postParameter.uuid;
            if (string.IsNullOrEmpty(openid) || uuid == Guid.Empty)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            using (UserRepository userRepository = new UserRepository())
            {
                Guid myuuid = await userRepository.GetUserUuidByOpenid(openid);
                UserContacts uc1 = new UserContacts() { AccountEmail_uuid = myuuid, ConAccount_uuid = uuid, RequestUser_uuid = myuuid, AddTime = DateTime.Now };
                UserContacts uc2 = new UserContacts() { AccountEmail_uuid = uuid, ConAccount_uuid = myuuid, RequestUser_uuid = myuuid, AddTime = DateTime.Now };
                var result = await userRepository.AddUserContact(uc1, uc2);
                if (result)
                {
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.Success);
                }
                else
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.AccountExist);
            }
        }

        //TODO:zset缓存
        /// <summary>
        /// 删除好友
        /// post api/UserIndex/DeleteContact
        /// </summary>
        /// <param name="postParameter">openid:自己 uuid:对方</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 不存在
        /// Success 成功
        /// </returns>
        [Route("api/ProfessorIndex/DeleteContact")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostDeleteContact([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            Guid uuid = postParameter.uuid;
            if (string.IsNullOrEmpty(openid) || uuid == Guid.Empty)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            using (UserRepository userRepository = new UserRepository())
            {
                Guid myuuid = await userRepository.GetUserUuidByOpenid(openid);
                var result = await userRepository.DeleteUserContact(uuid, myuuid);
                if (result)
                {
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.Success);
                }
                else
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.NotFound);
            }
        }


        /// <summary>
        /// 用户名片页 添加赞
        /// post api/UserIndex/AddFavorite
        /// </summary>
        /// <param name="postParameter">openid: uuid:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// </returns>
        [Route("api/ProfessorIndex/AddFavorite")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostAddFavorite([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            Guid uuid = postParameter.uuid;
            if (string.IsNullOrEmpty(openid) || uuid == Guid.Empty)
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using (UserRepository userRepository = new UserRepository())
            {
                Guid myuuid = await userRepository.GetUserUuidByOpenid(openid);
                user_favorite uf = new user_favorite() { user_account_uuid = myuuid, user_fav_account_uuid = uuid, add_time = DateTime.Now };
                var result = await userRepository.AddUserFavorite(uf);
                if (result)
                {
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Success);
                }
                else
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.AccountExist);
            }
        }

        //TODO:set缓存
        /// <summary>
        /// 用户名片页 删除赞
        /// post api/UserIndex/DeleteFavorite
        /// </summary>
        /// <param name="postParameter">openid: uuid:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 未找到记录
        /// Success 成功
        /// </returns>
        [Route("api/ProfessorIndex/DeleteFavorite")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostDeleteFavorite([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            Guid uuid = postParameter.uuid;
            if (string.IsNullOrEmpty(openid) || uuid == Guid.Empty)
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using (UserRepository userRepository = new UserRepository())
            {
                Guid myuuid = await userRepository.GetUserUuidByOpenid(openid);
                user_favorite uf = new user_favorite() { user_account_uuid = myuuid, user_fav_account_uuid = uuid, add_time = DateTime.Now };
                var result = await userRepository.DeleteUserFavorite(uf);
                if (result)
                {
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Success);
                }
                else
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);
            }
        }

        //TODO:要加入zset缓存。
        /// <summary>
        /// 用户名片页 页面初始化 访客列表.
        /// post api/UserIndex/InitializeUserVisitor
        /// </summary>
        /// <param name="postParameter">openid: uuid: pageIndex: pageSize:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// </returns>
        [Route("api/ProfessorIndex/InitializeUserVisitor")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeUserVisitor([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            Guid uuid = postParameter.uuid;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;
            if (string.IsNullOrEmpty(openid) || pageSize == 0 || uuid == Guid.Empty)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using (UserRepository userRepository = new UserRepository())
            {
                var uclist = await userRepository.GetUserVisitor(uuid, pageIndex, pageSize);
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, uclist, HttpStatusCode.OK, customStatus.Success);
            }
        }

        //TODO: zset缓存
        /// <summary>
        /// 用户名片页 页面初始化 访客访问过的列表
        /// post api/UserIndex/InitializeUserVisitorBeenTo
        /// </summary>
        /// <param name="postParameter">openid: uuid: pageIndex: pageSize:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// </returns>
        [Route("api/ProfessorIndex/InitializeUserVisitorBeenTo")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeUserVisitorBeenTo([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            Guid uuid = postParameter.uuid;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;
            if (string.IsNullOrEmpty(openid) || pageSize == 0 || uuid == Guid.Empty)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using (UserRepository userRepository = new UserRepository())
            {
                var uclist = await userRepository.GetVisitorBeenTo(uuid, pageIndex, pageSize);
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, uclist, HttpStatusCode.OK, customStatus.Success);
            }
        }
    }
}