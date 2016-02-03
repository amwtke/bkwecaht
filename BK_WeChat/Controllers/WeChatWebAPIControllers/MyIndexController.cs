using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.Util;
using BK.Model.DB;
using BK.WeChat.Controllers.Base;
using BK.WeChat.Controllers.WeChatWebAPIParameters;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace BK.WeChat.Controllers
{
    public class MyIndexController : ApiController
    {
        UserInfo userinfo; 

        /// <summary>
        /// 学生用户主页 页面初始化
        /// post api/MyIndex/Initialize
        /// </summary>
        /// <param name="loginParameter">openid:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 账号不存在
        /// Success 成功
        /// </returns>
        [Route("api/MyIndex/Initialize")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitialize([FromBody]LoginParameter loginParameter)
        {
            string openid = loginParameter.openID;
            if (string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(loginParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using (UserRepository userRepository = new UserRepository())
            {
                userinfo = await userRepository.GetUserInfoByOpenid(openid);
                if (userinfo == null)
                {
                    return WebApiHelper.HttpRMtoJson(loginParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.NotFound);
                }
                userinfo.NumOfContacts = await userRepository.GetUserContactNumber(userinfo.uuid);
                userinfo.NumOfBeenTo = await userRepository.GetUserBeenToNumber(userinfo.uuid);
                userinfo.NumOfFavorite = await userRepository.GetuserFavoriteNumber(userinfo.uuid);
                
                return WebApiHelper.HttpRMtoJson(userinfo, HttpStatusCode.OK, customStatus.Success);
            }


        }


        /// <summary>
        /// 学生用户主页 页面初始化 我的好友
        /// post api/MyIndex/InitializeMyContact
        /// </summary>
        /// <param name="postParameter">openid: pageIndex: pageSize:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// </returns>
        [Route("api/MyIndex/InitializeMyContact")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeMyContact([FromBody]LoginParameter postParameter)
        {
            string openid = postParameter.openID;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;
            if (string.IsNullOrEmpty(openid) || pageSize == 0)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            using (UserRepository userRepository = new UserRepository())
            {
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);
                if(userUuid == Guid.Empty)
                {
                    return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.NotFound);
                }
                var uclist = await userRepository.GetUserContact(userUuid, pageIndex, pageSize);
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, uclist, HttpStatusCode.OK, customStatus.Success);
            }
        }

        /// <summary>
        /// 学生用户主页 页面初始化 我访问过
        /// post api/MyIndex/InitializeMyBeenTo
        /// </summary>
        /// <param name="postParameter">openid: pageIndex: pageSize:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// </returns>
        [Route("api/MyIndex/InitializeMyBeenTo")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitializeMyBeenTo([FromBody]LoginParameter postParameter)
        {
            string openid = postParameter.openID;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;
            if (string.IsNullOrEmpty(openid) || pageSize == 0)
            {
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using (UserRepository userRepository = new UserRepository())
            {
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);
                var uclist = await userRepository.GetUserBeenTo(userUuid, pageIndex, pageSize);
                return WebApiHelper.HttpRMtoJson(postParameter.jsonpCallback, uclist, HttpStatusCode.OK, customStatus.Success);
            }
        }
    }
}
