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
using BK.Model.Redis.Objects.Messaging;
using Newtonsoft.Json;
using BK.Model.Configuration.Redis;
using BK.Model.MQ;
using Newtonsoft.Json.Converters;

namespace BK.WeChat.Controllers
{
    public class NoticeController: ApiController
    {
        /// <summary>
        /// 消息中心 通知项初始化
        /// post api/Notice/InitializeCenter
        /// </summary>
        /// <param name="postParameter">openid:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// </returns>
        [Route("api/Notice/InitializeCenter")]
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

                double unreadNum = await NoticeRedisOp.GetUnreadScore(uuid);
                var latestNotice = await WeChatNoticeReceiveHelper.GetFirstNoticeFromRedis(uuid);
                if(latestNotice != null)
                    latestNotice.RelationObj = await userRepository.GetUserInfoByUuidAsync(latestNotice.RelationID_uuid);
                return WebApiHelper.HttpRMtoJson(Tuple.Create(unreadNum, latestNotice), HttpStatusCode.OK, customStatus.Success);
            }
        }
        
        /// <summary>
        /// 通知页 页面初始化
        /// post api/Notice/Initialize
        /// </summary>
        /// <param name="postParameter">openid: pageIndex:一页10条 页码从0开始</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 未发现对象
        /// Success 成功
        /// </returns>
        [Route("api/Notice/Initialize")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostChatInitialize([FromBody]BaseParameter postParameter)
        {
            string openid = postParameter.openID;
            int pageIndex = postParameter.pageIndex;
            if(string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                var user = await userRepository.GetUserUuidByOpenid(openid);
                if(user == null)
                {
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);
                }
                var noticeList = await WeChatNoticeReceiveHelper.GetNotice(user, pageIndex);

                if(noticeList != null)
                    foreach(var n in noticeList)
                    {
                        if(n != null)
                            n.RelationObj = await userRepository.GetUserInfoByUuidAsync(n.RelationID_uuid);
                    }

                return WebApiHelper.HttpRMtoJson(Tuple.Create(user, noticeList), HttpStatusCode.OK, customStatus.Success);
            }
        }

        /// <summary>
        /// 是否有未读消息
        /// post api/Notice/IsThereUnreadNotice
        /// </summary>
        /// <param name="postParameter">openid:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 未发现对象
        /// Success 成功
        /// </returns>
        [Route("api/Notice/IsThereUnreadNotice")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostIsThereUnreadNotice([FromBody]BaseParameter postParameter)
        {
            string openid = postParameter.openID;
            if (string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            using (UserRepository userRepository = new UserRepository())
            {
                var userUuid = await userRepository.GetUserUuidByOpenid(openid);
                if (userUuid.Equals(Guid.Empty))
                {
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);
                }
                else
                {
                    bool flag = await NoticeRedisOp.GetUnreadScore(userUuid) > 0;
                    return WebApiHelper.HttpRMtoJson(flag, HttpStatusCode.OK, customStatus.Success);
                }
            }
        }

        /// <summary>
        /// 加联系人请求的操作 同意
        /// post api/Notice/ContactAgreeOrDiscard
        /// </summary>
        /// <param name="postParameter">openid: itemId:该条通知的id textMsg: Agree同意 Discard拒绝</param>
        /// <returns></returns>
        [Route("api/Notice/ContactAgreeOrDiscard")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostContactAgreeOrDiscard([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            string textMsg = postParameter.textMsg;
            long itemId= postParameter.itemId;

            if(string.IsNullOrEmpty(openid) || itemId==0 || !(textMsg == "Agree" || textMsg == "Discard"))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                using(NoticeRepository noticeRepository = new NoticeRepository())
                {
                    var msg = await noticeRepository.GetNotice(itemId);
                    string back = "";
                    bool result = false;
                    if(textMsg == "Agree")
                    {
                        //好友记录改为1
                        result = await userRepository.ActiveUserContact(msg.Receiver_uuid,msg.RelationID_uuid);
                        //该条要把状态变化成已同意
                        msg.Status = 1;
                        //需要发回的消息
                        back = "我已经同意加您为好友，打个招呼吧";
                    }
                    else if(textMsg == "Discard")
                    {
                        //好友记录删除
                        result = await userRepository.DeleteUserContact(msg.Receiver_uuid, msg.RelationID_uuid);
                        //该条要把状态变化成已拒绝
                        msg.Status = 2;
                        //需要发回的消息
                        back = "我拒绝了您的好友请求。";
                    }

                    //通过队列修改redis和sql中的状态
                    if(result)
                    {
                        await WeChatNoticeSendMQHelper.UpdateNoticeAddContact(msg);
                        await WeChatSendMQHelper.SendMessage(msg.Receiver_uuid.ToString().ToUpper(), msg.RelationID_uuid.ToString().ToUpper(), back);

                        return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Success);
                    }
                    else
                    {
                        return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Fail);
                    }
                }
            }
        }

        [Route("api/Notice/TestAddContact")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostAddContact([FromBody]DualParameter postParameter)
        {
            string openid = postParameter.openID;
            long itemId = postParameter.itemId;

            using(NoticeRepository noticeRepository = new NoticeRepository())
            {
                Message m = await noticeRepository.GetNotice(itemId);

                NoticeMQ rawObject = new NoticeMQ();
                rawObject.Id = m.ID;
                rawObject.MsgType = NoticeType.Contact_Request;
                rawObject.Relation_Uuid = m.RelationID_uuid;
                rawObject.Receiver_Uuid = m.Receiver_uuid;
                rawObject.PayLoad = m.MessageInfo;
                //改了
                rawObject.status = 1;
                rawObject.TimeStamp = m.SendTime;

                var iso = new IsoDateTimeConverter();
                iso.DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

                RedisManager2<WeChatRedisConfig> _redis = new RedisManager2<WeChatRedisConfig>();
                WeChatNoticeRedis redisObject = new WeChatNoticeRedis();
                redisObject.Uuid = rawObject.Receiver_Uuid.ToString();
                redisObject.Message = JsonConvert.SerializeObject(rawObject, iso);
                //改回
                rawObject.status = 0;
                rawObject.TimeStamp = DateTime.Now;
                WeChatNoticeRedis originalRedisObject = new WeChatNoticeRedis();
                originalRedisObject.Uuid = rawObject.Receiver_Uuid.ToString();
                originalRedisObject.Message = JsonConvert.SerializeObject(rawObject, iso);

                await _redis.ReplaceObjectInListAsync(redisObject, originalRedisObject);
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Fail);
            }
        }

    }
}
