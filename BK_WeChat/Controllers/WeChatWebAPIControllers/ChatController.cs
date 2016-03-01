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
using BK.Model.Index;
using BK.CommonLib.ElasticSearch;
using BK.CommonLib.MQ;
using BK.CommonLib.DB.Redis.Objects;

namespace BK.WeChat.Controllers
{
    public class ChatController: ApiController
    {
        /// <summary>
        /// 消息中心 页面初始化
        /// post api/ChatCenter/Initialize
        /// </summary>
        /// <param name="postParameter">openid: radius:距离 pageIndex: pageSize:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 未发现位置信息，请开启位置服务
        /// Success 成功
        /// </returns>
        [Route("api/ChatCenter/Initialize")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostChatCenterInitialize([FromBody]BaseParameter postParameter)
        {
            string openid = postParameter.openID;
            if(string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            using(UserRepository userRepository = new UserRepository())
            {
                var uuid = await userRepository.GetUserUuidByOpenid(openid);
                var sessions = await MessageRedisOp.GetSessionsTimeStampByUuid(uuid.ToString().ToUpper(), Order.Descending, 0, -1);
                List<Tuple<double, UserInfo, string>> ChatCenterList = new List<Tuple<double, UserInfo, string>>();
                foreach(var s in sessions)
                {
                    double unreadNum = await MessageRedisOp.GetUnreadScore(uuid.ToString().ToUpper(), s.Key);
                    List<string> uuidPair = await MessageRedisOp.GetUUidsBySessionId(s.Key);
                    string userUuid = uuidPair[0] == uuid.ToString().ToUpper() ? uuidPair[1] : uuidPair[0];

                    Guid userGUID;
                    if(!Guid.TryParse(userUuid, out userGUID) || userGUID.Equals(Guid.Empty))
                        continue;
                    UserInfo toUser = await userRepository.GetUserInfoByUuidAsync(userGUID);

                    var latestMessage = await WeChatReceiveHelper.GetFirstMessagesFromRedis(uuid.ToString().ToUpper(), userUuid.ToUpper());
                    ChatCenterList.Add(Tuple.Create(unreadNum, toUser, latestMessage));
                }
                return WebApiHelper.HttpRMtoJson(ChatCenterList, HttpStatusCode.OK, customStatus.Success);
            }
        }


        /// <summary>
        /// 聊天单页 页面初始化
        /// post api/Chat/Initialize
        /// </summary>
        /// <param name="postParameter">openid: uuid:另一人的 pageIndex:一页10条 页码从0开始</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 未发现对象
        /// Success 成功
        /// </returns>
        [Route("api/Chat/Initialize")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostChatInitialize([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            Guid uuid = postParameter.uuid;
            int pageIndex = postParameter.pageIndex;
            if(string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                var user = await userRepository.GetUserInfoByOpenid(openid);
                var touser = await userRepository.GetUserInfoByUuidAsync(uuid);
                if(user == null || touser == null)
                {
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);
                }
                var chatList = await WeChatReceiveHelper.GetMessage(user.uuid.ToString().ToUpper(), touser.uuid.ToString().ToUpper(), pageIndex);

                return WebApiHelper.HttpRMtoJson(Tuple.Create(user, touser, chatList), HttpStatusCode.OK, customStatus.Success);
            }
        }

        /// <summary>
        /// 聊天单页 发出消息
        /// post api/Chat/Initialize
        /// </summary>
        /// <param name="postParameter">openid: uuid:另一人的</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Fail 发送失败
        /// Success 成功
        /// </returns>
        [Route("api/Chat/SendChat")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostSendChat([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            Guid uuid = postParameter.uuid;
            string textMsg = postParameter.textMsg;
            if(string.IsNullOrEmpty(openid) || uuid == Guid.Empty || string.IsNullOrEmpty(textMsg))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);
                var result = await WeChatSendMQHelper.SendMessage(userUuid.ToString().ToUpper(), uuid.ToString().ToUpper(), textMsg);
                if(result)
                {
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Success);
                }
                else
                {
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Fail);
                }
            }
        }

        [Route("api/Chat/IsGotUnred")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> IsThereUnredMessage([FromBody]BaseParameter postParameter)
        {
            string openid = postParameter.openID;
            if(string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson("没有传openid", HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            using(UserRepository userRepository = new UserRepository())
            {
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);
                if(userUuid.Equals(Guid.Empty))
                {
                    return WebApiHelper.HttpRMtoJson("没有取到uuid，openid=" + openid, HttpStatusCode.OK, customStatus.Fail);
                }
                else
                {
                    bool flag = await MessageRedisOp.IsGetUnredScore(userUuid.ToString().ToUpper());
                    return WebApiHelper.HttpRMtoJson(flag, HttpStatusCode.OK, customStatus.Success);
                }
            }
        }
        
    }
}
