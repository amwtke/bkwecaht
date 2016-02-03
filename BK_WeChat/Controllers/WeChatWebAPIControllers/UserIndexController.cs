using BK.CommonLib.DB.Redis;
using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.Util;
using BK.Model.DB;
using BK.WeChat.Controllers.WeChatWebAPIControllerHelper;
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

namespace BK.WeChat.Controllers
{
    public class UserIndexController: ApiController
    {
        UserInfo userinfo;

        /// <summary>
        /// 学生用户名片页 页面初始化
        /// post api/UserIndex/Initialize
        /// </summary>
        /// <param name="postParameter">openid: uuid:被查看的人的uuid</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 账号不存在
        /// Success 成功
        /// </returns>
        [Route("api/UserIndex/Initialize")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitialize([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            Guid uuid = postParameter.uuid;
            if(string.IsNullOrEmpty(openid) && uuid == Guid.Empty)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                userinfo = await userRepository.GetUserInfoByUuid(uuid);
                if(userinfo == null)
                {
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.NotFound);
                }
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);

                VisitBetweenUser newVisitor = new VisitBetweenUser() { UserGuest_uuid = userUuid, UserHost_uuid = uuid, VisitTime = DateTime.Now };
                if(await userRepository.AddVisitBetweenUser(newVisitor))
                    await CommonLib.MQ.WeChatNoticeSendMQHelper.SendNoticeVisit(uuid, userUuid);

                userinfo.NumOfContacts = await userRepository.GetUserContactNumber(uuid);
                userinfo.NumOfVisitor = await userRepository.GetUserVisitorNumber(uuid);
                userinfo.NumOfFavorite = await userRepository.GetuserFavoriteNumber(uuid);
                userinfo.IsContact = await userRepository.IsUserContact(userUuid, uuid);
                userinfo.IsFavorite = (await userRepository.IsUserFavorite(userUuid, uuid)).ToString();
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, userinfo, HttpStatusCode.OK, customStatus.Success);
            }
        }
        /// <summary>
        /// 学生用户名片页 页面初始化 其他学术资料
        /// post api/UserIndex/InitializeUserIndexRecords
        /// </summary>
        /// <param name="postParameter">openid: uuid:对方uuid</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 账号不存在
        /// Success 成功
        /// userAwardsList 专长技能 userPatentList 核心课程 userEducationList 教育经历 userExperienceList 工作经历 userArticleNumber 学术论文 visitorsBeenToTopSix 最后一项
        /// </returns>
        [Route("api/UserIndex/InitializeUserIndexRecords")]
        [UserBehaviorFilter]
        [NameCardWebApiFilter]
        public async Task<HttpResponseMessage> PostInitializeMyRecords([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            Guid uuid = postParameter.uuid;
            if(string.IsNullOrEmpty(openid) && uuid == Guid.Empty)
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            using(UserRepository userRepository = new UserRepository())
            {
                List<UserSkill> userSkillList = await userRepository.GetUserRecords(new UserSkill() { AccountEmail_uuid = uuid });
                List<UserCourse> userCourseList = await userRepository.GetUserRecords(new UserCourse() { AccountEmail_uuid = uuid });
                List<UserEducation> userEducationList = await userRepository.GetUserRecords(new UserEducation() { AccountEmail_uuid = uuid });
                List<UserExperience> userExperienceList = await userRepository.GetUserRecords(new UserExperience() { AccountEmail_uuid = uuid });
                int userArticleNumber = await userRepository.GetUserRecordsNumber(new UserArticle() { AccountEmail_uuid = uuid });
                int visitorsBeenToNumber = await userRepository.GetVisitorBeenToNumber(uuid);
                List<UserInfo> visitorsBeenTo = await userRepository.GetVisitorBeenTo(uuid, 6);
                Dictionary<string, object> tempResult = new Dictionary<string, object>();
                tempResult.Add("userSkillList", userSkillList);
                tempResult.Add("userCourseList", userCourseList);
                tempResult.Add("userEducationList", userEducationList);
                tempResult.Add("userExperienceList", userExperienceList);
                tempResult.Add("userArticleNumber", userArticleNumber);
                tempResult.Add("visitorsBeenToNumber", visitorsBeenToNumber);
                tempResult.Add("visitorsBeenToTopSix", visitorsBeenTo);
                return WebApiHelper.HttpRMtoJson(tempResult, HttpStatusCode.OK, customStatus.Success);


            }
        }

        /// <summary>
        /// 学生用户名片页 页面初始化 用户好友列表
        /// post api/UserIndex/InitializeUserContact
        /// </summary>
        /// <param name="postParameter">openid: uuid: pageIndex: pageSize:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// </returns>
        [Route("api/UserIndex/InitializeUserContact")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeuserContact([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;
            Guid uuid = postParameter.uuid;
            if(string.IsNullOrEmpty(openid) && uuid == Guid.Empty)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            using(UserRepository userRepository = new UserRepository())
            {
                var uclist = await userRepository.GetUserContact(uuid, pageIndex, pageSize);
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, uclist, HttpStatusCode.OK, customStatus.Success);
            }
        }
        /// <summary>
        /// 添加好友
        /// post api/UserIndex/AddContact
        /// </summary>
        /// <param name="postParameter">openid:自己 uuid:对方 textMsg:附言</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// AccountExist 已存在
        /// Success 成功
        /// </returns>
        [Route("api/UserIndex/AddContact")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostAddContact([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            Guid uuid = postParameter.uuid;
            string textMsg = postParameter.textMsg;
            if(string.IsNullOrEmpty(openid) || uuid == Guid.Empty)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            using(UserRepository userRepository = new UserRepository())
            {
                Guid myuuid = await userRepository.GetUserUuidByOpenid(openid);
                UserContacts uc1 = new UserContacts() { AccountEmail_uuid = myuuid, ConAccount_uuid = uuid, RequestUser_uuid = myuuid, AddTime = DateTime.Now, Status = false };
                UserContacts uc2 = new UserContacts() { AccountEmail_uuid = uuid, ConAccount_uuid = myuuid, RequestUser_uuid = myuuid, AddTime = DateTime.Now, Status = false };
                var result = await userRepository.AddUserContact(uc1, uc2);
                if(result)
                {
                    if(string.IsNullOrEmpty(textMsg))
                        await CommonLib.MQ.WeChatNoticeSendMQHelper.SendNoticeAddContact(uuid, myuuid);
                    else
                        await CommonLib.MQ.WeChatNoticeSendMQHelper.SendNoticeAddContact(uuid, myuuid,textMsg);

                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.Success);
                }
                else
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.AccountExist);
            }
        }
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
        [Route("api/UserIndex/DeleteContact")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostDeleteContact([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            Guid uuid = postParameter.uuid;
            if(string.IsNullOrEmpty(openid) || uuid == Guid.Empty)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            using(UserRepository userRepository = new UserRepository())
            {
                Guid myuuid = await userRepository.GetUserUuidByOpenid(openid);
                var result = await userRepository.DeleteUserContact(uuid, myuuid);
                if(result)
                {
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.Success);
                }
                else
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.NotFound);
            }
        }


        /// <summary>
        /// 学生用户名片页 添加赞
        /// post api/UserIndex/AddFavorite
        /// </summary>
        /// <param name="postParameter">openid: uuid:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// </returns>
        [Route("api/UserIndex/AddFavorite")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostAddFavorite([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            Guid uuid = postParameter.uuid;
            if(string.IsNullOrEmpty(openid) || uuid == Guid.Empty)
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                Guid myuuid = await userRepository.GetUserUuidByOpenid(openid);
                user_favorite uf = new user_favorite() { user_account_uuid = myuuid, user_fav_account_uuid = uuid, add_time = DateTime.Now };
                var result = await userRepository.AddUserFavorite(uf);
                if(result)
                {
                    await CommonLib.MQ.WeChatNoticeSendMQHelper.SendNoticeFavorite(uuid, myuuid);
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Success);
                }
                else
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.AccountExist);
            }
        }

        /// <summary>
        /// 学生用户名片页 删除赞
        /// post api/UserIndex/DeleteFavorite
        /// </summary>
        /// <param name="postParameter">openid: uuid:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 未找到记录
        /// Success 成功
        /// </returns>
        [Route("api/UserIndex/DeleteFavorite")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostDeleteFavorite([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            Guid uuid = postParameter.uuid;
            if(string.IsNullOrEmpty(openid) || uuid == Guid.Empty)
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                Guid myuuid = await userRepository.GetUserUuidByOpenid(openid);
                user_favorite uf = new user_favorite() { user_account_uuid = myuuid, user_fav_account_uuid = uuid, add_time = DateTime.Now };
                var result = await userRepository.DeleteUserFavorite(uf);
                if(result)
                {
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Success);
                }
                else
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);
            }
        }


        /// <summary>
        /// 学生用户名片页 页面初始化 访客列表
        /// post api/UserIndex/InitializeUserVisitor
        /// </summary>
        /// <param name="postParameter">openid: uuid: pageIndex: pageSize:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// </returns>
        [Route("api/UserIndex/InitializeUserVisitor")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeUserVisitor([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            Guid uuid = postParameter.uuid;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;
            if(string.IsNullOrEmpty(openid) || pageSize == 0 || uuid == Guid.Empty)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                var uclist = await userRepository.GetUserVisitor(uuid, pageIndex, pageSize);
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, uclist, HttpStatusCode.OK, customStatus.Success);
            }
        }
        /// <summary>
        /// 学生用户名片页 页面初始化 访客访问过的列表
        /// post api/UserIndex/InitializeUserVisitorBeenTo
        /// </summary>
        /// <param name="postParameter">openid: uuid: pageIndex: pageSize:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// </returns>
        [Route("api/UserIndex/InitializeUserVisitorBeenTo")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeUserVisitorBeenTo([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            Guid uuid = postParameter.uuid;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;
            if(string.IsNullOrEmpty(openid) || pageSize == 0 || uuid == Guid.Empty)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                var uclist = await userRepository.GetVisitorBeenTo(uuid, pageIndex, pageSize);
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, uclist, HttpStatusCode.OK, customStatus.Success);
            }
        }

        /// <summary>
        /// 资料修改 页面初始化 根据对象、ID取单条数据
        /// post api/UserIndex/InitializeUserRecords
        /// </summary>
        /// <param name="postParameter">openid: id: UserXXXXX:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// message: UserXXXX的对象
        /// </returns>
        [Route("api/UserIndex/InitializeUserRecords")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeUserRecords([FromBody]UserRecordsParameter postParameter)
        {
            string openid = postParameter.openID;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;
            if(string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            dynamic input = null;
            foreach(System.Reflection.PropertyInfo pi in postParameter.GetType().GetProperties())
            {
                if(pi.PropertyType.BaseType.Equals(typeof(DBModelBase)) && pi.GetValue(postParameter) != null)
                {
                    input = Convert.ChangeType(pi.GetValue(postParameter), pi.PropertyType);
                    break;
                }
            }
            if(input == null)
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            else if(input.AccountEmail_uuid == Guid.Empty)
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            using(UserRepository userRepository = new UserRepository())
            {
                if(input.Id != 0)
                {
                    input = await userRepository.GetUserRecordsById(input);
                }
                else if(pageIndex != 0 && pageSize != 0)
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

        [Route("api/UserIndex/GetUserInfo")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostUserInfo([FromBody]UserRecordsParameter postParameter)
        {
            string openid = postParameter.openID;
            Guid uuid = postParameter.uuid;
            if (string.IsNullOrEmpty(openid) || uuid == Guid.Empty)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using (UserRepository userRepository = new UserRepository())
            {
                var userinfo = await userRepository.GetUserInfoByUuid(uuid);
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, userinfo, HttpStatusCode.OK, customStatus.Success);
            }
        }

    }
}
