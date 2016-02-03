using BK.CommonLib.DB.Redis;
using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.Util;
using BK.Model.DB;
using BK.WeChat.Controllers.WeChatWebAPIControllerHelper;
using BK.WeChat.Controllers.Base;
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
    public class MyRecordsController : ApiController
    {
        UserInfo userinfo;

        /// <summary>
        /// 学生用户资料页 页面初始化
        /// post api/MyIndex/Initialize
        /// </summary>
        /// <param name="postParameter">openid:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 账号不存在
        /// Success 成功
        /// </returns>
        [Route("api/MyRecords/Initialize")]
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
                userinfo.NumOfVisitor = await userRepository.GetUserVisitorNumber(userinfo.uuid);
                userinfo.NumOfFavorite = await userRepository.GetuserFavoriteNumber(userinfo.uuid);
                using(SystemRepository systemRepository = new SystemRepository())
                {
                    userinfo.Hometown = await systemRepository.GetShortAddress(userinfo.HometownProvince,userinfo.HometownCity);
                }
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, userinfo, HttpStatusCode.OK, customStatus.Success);
            }


        }

        /// <summary>
        /// 学生用户资料页 页面初始化 我的资料
        /// post api/MyRecords/InitializeMyRecords
        /// </summary>
        /// <param name="postParameter">openid:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 账号不存在
        /// Success 成功
        /// userAwardsList 专长技能 userPatentList 核心课程 userEducationList 教育经历 userExperienceList 工作经历 userArticleNumber 学术论文 visitorsBeenToTopSix 最后一项
        /// </returns>
        [Route("api/MyRecords/InitializeMyRecords")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeMyRecords([FromBody]LoginParameter postParameter)
        {
            string openid = postParameter.openID;
            if(string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            using(UserRepository userRepository = new UserRepository())
            {
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);
                if(userUuid == Guid.Empty)
                {
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.NotFound);
                }
                else
                {
                    List<UserSkill> userSkillList = await userRepository.GetUserRecords(new UserSkill() { AccountEmail_uuid = userUuid });
                    List<UserCourse> userCourseList = await userRepository.GetUserRecords(new UserCourse() { AccountEmail_uuid = userUuid });
                    List<UserEducation> userEducationList = await userRepository.GetUserRecords(new UserEducation() { AccountEmail_uuid = userUuid });
                    List<UserExperience> userExperienceList = await userRepository.GetUserRecords(new UserExperience() { AccountEmail_uuid = userUuid });
                    int userArticleNumber = await userRepository.GetUserRecordsNumber(new UserArticle() { AccountEmail_uuid = userUuid });
                    int visitorsBeenToNumber = await userRepository.GetVisitorBeenToNumber(userUuid);
                    List<UserInfo> visitorsBeenTo = await userRepository.GetVisitorBeenTo(userUuid, 6);
                    Dictionary<string,object> tempResult = new Dictionary<string, object>();
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
        }

        /// <summary>
        /// 学生用户资料页 页面初始化 访客
        /// post api/MyRecords/InitializeMyVisitor
        /// </summary>
        /// <param name="postParameter">openid: pageIndex: pageSize:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// </returns>
        [Route("api/MyRecords/InitializeMyVisitor")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeMyVisitor([FromBody]LoginParameter postParameter)
        {
            string openid = postParameter.openID;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;
            if(string.IsNullOrEmpty(openid) || pageSize == 0)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);
                var uclist = await userRepository.GetUserVisitor(userUuid, pageIndex, pageSize);
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, uclist, HttpStatusCode.OK, customStatus.Success);
            }
        }
        /// <summary>
        /// 学生用户资料页 页面初始化 赞我的人
        /// post api/MyRecords/InitializeMyFavorite
        /// </summary>
        /// <param name="postParameter">openid: pageIndex: pageSize:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// </returns>
        [Route("api/MyRecords/InitializeMyFavorite")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeMyFavorite([FromBody]LoginParameter postParameter)
        {
            string openid = postParameter.openID;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;
            if(string.IsNullOrEmpty(openid) || pageSize == 0)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);
                var uclist = await userRepository.GetuserFavorite(userUuid, pageIndex, pageSize);
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, uclist, HttpStatusCode.OK, customStatus.Success);
            }
        }
        /// <summary>
        /// 学生用户资料页 页面初始化 访客访问过的
        /// post api/MyRecords/InitializeVisitorBeenTo
        /// </summary>
        /// <param name="postParameter">openid: pageIndex: pageSize:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// </returns>
        [Route("api/MyRecords/InitializeVisitorBeenTo")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeVisitorBeenTo([FromBody]LoginParameter postParameter)
        {
            string openid = postParameter.openID;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;
            if(string.IsNullOrEmpty(openid) || pageSize == 0)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                Guid userUuid = await userRepository.GetUserUuidByOpenid(openid);
                var uclist = await userRepository.GetVisitorBeenTo(userUuid, pageIndex, pageSize);
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, uclist, HttpStatusCode.OK, customStatus.Success);
            }
        }

        /// <summary>
        /// 资料修改 页面初始化 个人简介
        /// post api/MyRecords/InitializeUserIntroduction
        /// </summary>
        /// <param name="postParameter">openid:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 账号不存在
        /// Success 成功
        /// </returns>
        [Route("api/MyRecords/InitializeUserIntroduction")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeUserIntroduction([FromBody]BaseParameter postParameter)
        {
            string openid = postParameter.openID;
            if (string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using (UserRepository userRepository = new UserRepository())
            {
                var uclist = await userRepository.GetUserInfoByOpenid(openid);
                if (uclist == null)
                {
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);
                }
                return WebApiHelper.HttpRMtoJson(uclist.UserIntroduction, HttpStatusCode.OK, customStatus.Success);
            }


        }
        /// <summary>
        /// 资料修改 页面提交 个人简介
        /// post api/MyRecords/SubmitUserIntroduction
        /// </summary>
        /// <param name="postParameter">openid: name:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 账号不存在
        /// Success 成功
        /// </returns>
        [Route("api/MyRecords/SubmitUserIntroduction")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostSubmitUserIntroduction([FromBody]RegisterParameter postParameter)
        {
            string openid = postParameter.openID;
            string name = postParameter.name;
            if(string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            using(UserRepository userRepository = new UserRepository())
            {
                var uclist = await userRepository.GetUserInfoByOpenid(openid);
                if(uclist == null)
                {
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);
                }
                uclist.UserIntroduction = name;
                if(await userRepository.SaveUserInfo(uclist))
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Success);
                else
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Fail);
            }


        }

        #region 通用增删改查
        /// <summary>
        /// 资料修改 页面初始化 根据对象、ID取单条数据
        /// post api/MyRecords/InitializeUserRecordsById
        /// </summary>
        /// <param name="postParameter">openid: id: UserXXXXX:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// message: UserXXXX的对象
        /// </returns>
        [Route("api/MyRecords/InitializeUserRecordsById")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeUserRecordsById([FromBody]UserRecordsParameter postParameter)
        {
            string openid = postParameter.openID;
            if(string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                dynamic input=null;
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
                input = await userRepository.GetUserRecordsById(input);

                return WebApiHelper.HttpRMtoJson(input, HttpStatusCode.OK, customStatus.Success);
            }


        }
        /// <summary>
        /// 资料修改 页面提交 各种用户资料
        /// post api/MyRecords/SubmitUserRecords
        /// </summary>
        /// <param name="postParameter">openid: [UserXXXXX:]</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 账号不存在
        /// Forbidden禁止修改
        /// Success 成功
        /// </returns>
        [Route("api/MyRecords/SubmitUserRecords")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostSubmitUserRecords([FromBody]UserRecordsParameter postParameter)
        {
            string openid = postParameter.openID;
            if(string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            using(UserRepository userRepository = new UserRepository())
            {
                dynamic input =null;
                Guid uuid = await userRepository.GetUserUuidByOpenid(openid);
                if(uuid == Guid.Empty)
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);
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

                if(uuid != input.AccountEmail_uuid)
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Forbidden);

                if(await userRepository.SaveUserRecordsById(input))
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Success);
                else
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Fail);
                
            }


        }
        /// <summary>
        /// 资料修改 删除记录 根据对象ID
        /// post api/MyRecords/DeleteUserRecordsById
        /// </summary>
        /// <param name="postParameter">openid: UserXXXXX:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// </returns>
        [Route("api/MyRecords/DeleteUserRecordsById")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostDeleteUserRecordsById([FromBody]UserRecordsParameter postParameter)
        {
            string openid = postParameter.openID;
            if(string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                dynamic input = null;
                Guid uuid = await userRepository.GetUserUuidByOpenid(openid);
                if(uuid == Guid.Empty)
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);

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
                if(uuid != input.AccountEmail_uuid)
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Forbidden);

                if(await userRepository.DeleteUserRecordsById(input))
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Success);
                else
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Fail);
            }
        }
        #endregion

        #region 8项资料的各种取数据
        /// <summary>
        /// 资料修改 页面初始化 特长
        /// post api/MyRecords/InitializeUserSkill
        /// </summary>
        /// <param name="postParameter">openid: [pageIndex: pageSize:]</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// message: UserSkill的列表
        /// </returns>
        [Route("api/MyRecords/InitializeUserSkill")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeUserSkill([FromBody]UserRecordsParameter postParameter)
        {
            string openid = postParameter.openID;
            long id = postParameter.id;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;

            if(string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                dynamic resultList;
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);
                if(id != 0)
                {
                    resultList = null;
                    //resultList = await userRepository.GetUserRecordsById<UserSkill>(id);

                }
                else if(pageIndex!=0&& pageSize != 0)
                {
                    resultList = await userRepository.GetUserRecords( new UserSkill() { AccountEmail_uuid= userUuid } , pageIndex, pageSize);
                }
                else
                {
                    resultList = await userRepository.GetUserRecords(new UserSkill() { AccountEmail_uuid = userUuid });
                }
                return WebApiHelper.HttpRMtoJson(resultList, HttpStatusCode.OK, customStatus.Success);
            }


        }
        /// <summary>
        /// 资料修改 页面初始化 课程
        /// post api/MyRecords/InitializeUserCourse
        /// </summary>
        /// <param name="postParameter">openid: [pageIndex: pageSize:]</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// message: UserCourse的列表
        /// </returns>
        [Route("api/MyRecords/InitializeUserCourse")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeUserCourse([FromBody]UserRecordsParameter postParameter)
        {
            string openid = postParameter.openID;
            long id = postParameter.id;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;

            if(string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                dynamic resultList;
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);
                if(id != 0)
                {
                    resultList = null;
                    //resultList = await userRepository.GetUserRecordsById<UserCourse>(id);
                }
                else if(pageIndex != 0 && pageSize != 0)
                {
                    resultList = await userRepository.GetUserRecords(new UserCourse() { AccountEmail_uuid = userUuid }, pageIndex, pageSize);
                }
                else
                {
                    resultList = await userRepository.GetUserRecords(new UserCourse() { AccountEmail_uuid = userUuid });
                }
                return WebApiHelper.HttpRMtoJson(resultList, HttpStatusCode.OK, customStatus.Success);
            }


        }
        /// <summary>
        /// 资料修改 页面初始化 学术地位
        /// post api/MyRecords/InitializeUserAcademic
        /// </summary>
        /// <param name="postParameter">openid: [pageIndex: pageSize:]</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// message: UserAcademic的列表
        /// </returns>
        [Route("api/MyRecords/InitializeUserAcademic")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeUserAcademic([FromBody]UserRecordsParameter postParameter)
        {
            string openid = postParameter.openID;
            long id = postParameter.id;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;

            if(string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                dynamic resultList;
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);
                if(id != 0)
                {
                    resultList = null;
                    //resultList = await userRepository.GetUserRecordsById<UserAcademic>(id);
                }
                else if(pageIndex != 0 && pageSize != 0)
                {
                    resultList = await userRepository.GetUserRecords(new UserAcademic() { AccountEmail_uuid = userUuid }, pageIndex, pageSize);
                }
                else
                {
                    resultList = await userRepository.GetUserRecords(new UserAcademic() { AccountEmail_uuid = userUuid });
                }
                return WebApiHelper.HttpRMtoJson(resultList, HttpStatusCode.OK, customStatus.Success);
            }


        }
        /// <summary>
        /// 资料修改 页面初始化 文章
        /// post api/MyRecords/InitializeUserArticle 
        /// </summary>
        /// <param name="postParameter">openid: [pageIndex: pageSize:]</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// message: UserArticle 的列表
        /// </returns>
        [Route("api/MyRecords/InitializeUserArticle")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeUserArticle([FromBody]UserRecordsParameter postParameter)
        {
            string openid = postParameter.openID;
            long id = postParameter.id;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;

            if(string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                dynamic resultList;
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);
                if(id != 0)
                {
                    resultList = null;
                    //resultList = await userRepository.GetUserRecordsById<UserArticle>(id);
                }
                else if(pageIndex != 0 && pageSize != 0)
                {
                    resultList = await userRepository.GetUserRecords(new UserArticle() { AccountEmail_uuid = userUuid }, pageIndex, pageSize);
                }
                else
                {
                    resultList = await userRepository.GetUserRecords(new UserArticle() { AccountEmail_uuid = userUuid });
                }
                return WebApiHelper.HttpRMtoJson(resultList, HttpStatusCode.OK, customStatus.Success);
            }


        }
        /// <summary>
        /// 资料修改 页面初始化 自助奖励
        /// post api/MyRecords/InitializeUserAwards
        /// </summary>
        /// <param name="postParameter">openid: [pageIndex: pageSize:]</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// message: UserAwards的列表
        /// </returns>
        [Route("api/MyRecords/InitializeUserAwards")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeUserAwards([FromBody]UserRecordsParameter postParameter)
        {
            string openid = postParameter.openID;
            long id = postParameter.id;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;

            if(string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                dynamic resultList;
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);
                if(id != 0)
                {
                    resultList = null;
                    //resultList = await userRepository.GetUserRecordsById<UserAwards>(id);
                }
                else if(pageIndex != 0 && pageSize != 0)
                {
                    resultList = await userRepository.GetUserRecords(new UserAwards() { AccountEmail_uuid = userUuid }, pageIndex, pageSize);
                }
                else
                {
                    resultList = await userRepository.GetUserRecords(new UserAwards() { AccountEmail_uuid = userUuid });
                }
                return WebApiHelper.HttpRMtoJson(resultList, HttpStatusCode.OK, customStatus.Success);
            }


        }
        /// <summary>
        /// 资料修改 页面初始化 教育经历
        /// post api/MyRecords/InitializeUserEducation
        /// </summary>
        /// <param name="postParameter">openid: [pageIndex: pageSize:]</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// message: UserEducation的列表
        /// </returns>
        [Route("api/MyRecords/InitializeUserEducation")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeUserEducation([FromBody]UserRecordsParameter postParameter)
        {
            string openid = postParameter.openID;
            long id = postParameter.id;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;

            if(string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                dynamic resultList;
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);
                if(id != 0)
                {
                    resultList = null;
                    //resultList = await userRepository.GetUserRecordsById<UserEducation>(id);
                }
                else if(pageIndex != 0 && pageSize != 0)
                {
                    resultList = await userRepository.GetUserRecords(new UserEducation() { AccountEmail_uuid = userUuid }, pageIndex, pageSize);
                }
                else
                {
                    resultList = await userRepository.GetUserRecords(new UserEducation() { AccountEmail_uuid = userUuid });
                }
                return WebApiHelper.HttpRMtoJson(resultList, HttpStatusCode.OK, customStatus.Success);
            }


        }
        /// <summary>
        /// 资料修改 页面初始化 工作经历
        /// post api/MyRecords/InitializeUserExperience
        /// </summary>
        /// <param name="postParameter">openid: [pageIndex: pageSize:]</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// message: UserExperience的列表
        /// </returns>
        [Route("api/MyRecords/InitializeUserExperience")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeUserExperience([FromBody]UserRecordsParameter postParameter)
        {
            string openid = postParameter.openID;
            long id = postParameter.id;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;

            if(string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                dynamic resultList;
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);
                if(id != 0)
                {
                    resultList = null;
                    //resultList = await userRepository.GetUserRecordsById<UserExperience>(id);
                }
                else if(pageIndex != 0 && pageSize != 0)
                {
                    resultList = await userRepository.GetUserRecords(new UserExperience() { AccountEmail_uuid = userUuid }, pageIndex, pageSize);
                }
                else
                {
                    resultList = await userRepository.GetUserRecords(new UserExperience() { AccountEmail_uuid = userUuid });
                }
                return WebApiHelper.HttpRMtoJson(resultList, HttpStatusCode.OK, customStatus.Success);
            }


        }
        /// <summary>
        /// 资料修改 页面初始化 专利
        /// post api/MyRecords/InitializeUserPatent
        /// </summary>
        /// <param name="postParameter">openid: [pageIndex: pageSize:]</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// message: UserPatent的列表
        /// </returns>
        [Route("api/MyRecords/InitializeUserPatent")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeUserPatent([FromBody]UserRecordsParameter postParameter)
        {
            string openid = postParameter.openID;
            long id = postParameter.id;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;

            if(string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                dynamic resultList;
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);
                if(id != 0)
                {
                    resultList = null;
                    //resultList = await userRepository.GetUserRecordsById<UserPatent>(id);
                }
                else if(pageIndex != 0 && pageSize != 0)
                {
                    resultList = await userRepository.GetUserRecords(new UserPatent() { AccountEmail_uuid = userUuid }, pageIndex, pageSize);
                }
                else
                {
                    resultList = await userRepository.GetUserRecords(new UserPatent() { AccountEmail_uuid = userUuid });
                }
                return WebApiHelper.HttpRMtoJson(resultList, HttpStatusCode.OK, customStatus.Success);
            }


        }
        #endregion
    }
}
