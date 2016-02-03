using BK.CommonLib.DB.Redis;
using BK.CommonLib.DB.Redis.Objects;
using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.Log;
using BK.CommonLib.Util;
using BK.Model.Configuration.MQ;
using BK.Model.Configuration.Redis;
using BK.Model.Configuration.User;
using BK.Model.DB;
using BK.Model.MQ;
using BK.Model.Redis.Objects.Messaging;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BK.CommonLib.MQ
{
    public static class WeChatNoticeSendMQHelper
    {
        //static RedisManager2<WeChatRedisConfig> _redis;
        static WeChatNoticeSendMQHelper()
        {
            //if (_redis == null)
            //    _redis = new RedisManager2<WeChatRedisConfig>();
            try
            {
                MQManager.Prepare_All_P_MQ();
            }
            catch (Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(WeChatNoticeSendMQHelper), ex);
                throw ex;
            }
        }

        //static ConcurrentDictionary<string, string> _userToSessionIdDIc = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// 获取如果没有则创建一对uuid的session值。
        /// </summary>
        /// <param name="from">user uuid</param>
        /// <param name="to">user uuid</param>
        /// <returns></returns>
        //public static async Task<string> GetOrCreateSessionId(string from, string to)
        //{
        //    from = from.Trim().ToUpper();
        //    to = to.Trim().ToUpper();
        //    string key = from + "_" + to;

        //    //先看缓存中是否存在
        //    string ret = null;
        //    if (_userToSessionIdDIc.TryGetValue(key, out ret))
        //        return ret;

        //    //redis中是否存在
        //    var sessionObject = await _redis.GetObjectFromRedis<WeChatUserToSessionHash>(key);
        //    if (sessionObject != null && !String.IsNullOrEmpty(sessionObject.SessionId))
        //    {
        //        _userToSessionIdDIc[key] = sessionObject.SessionId;
        //        return sessionObject.SessionId;
        //    }

        //    //双向绑定到一个sessionid,
        //    //初始化user.message.user_to_session.hash这个redishash，
        //    //因为sessionid是在发送消息的时候才会第一次调用。
        //    string key2 = to + "_" + from;
        //    WeChatUserToSessionHash o1 = new WeChatUserToSessionHash();
        //    WeChatUserToSessionHash o2 = new WeChatUserToSessionHash();
        //    string uuid = Guid.NewGuid().ToString();
        //    o1.Key = key; o1.SessionId = uuid;
        //    o2.Key = key2; o2.SessionId = uuid;
        //    bool flag = await _redis.SaveObjectAsync(o1);
        //    flag = await _redis.SaveObjectAsync(o2);


        //    //然后反向初始化sessionid取user的redis hash
        //    //user.message.session_to_user.hash
        //    //users按照','分割。
        //    WeChatSessoinToUsersHash h = new WeChatSessoinToUsersHash();
        //    h.SessionId = uuid;
        //    h.Users = to + "," + from;
        //    flag = await _redis.SaveObjectAsync(h);

        //    if (!flag)
        //    {
        //        LogHelper.LogErrorAsync(typeof(WeChatSendMQHelper), new Exception("怎么回事？sessionid创建失败！"));
        //    }

        //    return uuid;
        //}

        public static async Task<bool> SendNotice(NoticeMQ obj)
        {
            //obj.TimeStamp = Util.CommonHelper.GetUnixTimeNow();
            obj.TimeStamp = DateTime.Now;
            return MQManager.SendMQ_TB<NoticeMQConfig>(obj);
        }

        #region 发送时调用的相关接口
        public static async Task<bool> SendNoticeAddContact(Guid receiverUuid, Guid relationUuid, string msg = "")
        {
            NoticeMQ obj = new NoticeMQ();
            obj.MsgType = NoticeType.Contact_Request;
            obj.Relation_Uuid = relationUuid;
            obj.Receiver_Uuid = receiverUuid;
            obj.PayLoad = msg;
            return await SendNotice(obj);
        }
        public static async Task<bool> UpdateNoticeAddContact(Message m)
        {
            NoticeMQ obj = new NoticeMQ();
            obj.Id = m.ID;
            obj.MsgType = NoticeType.Contact_Request;
            obj.Relation_Uuid = m.RelationID_uuid;
            obj.Receiver_Uuid = m.Receiver_uuid;
            obj.PayLoad = m.MessageInfo;
            obj.status = m.Status;
            //obj.TimeStamp = CommonHelper.ToUnixTime(m.SendTime);
            obj.TimeStamp = m.SendTime;
            return MQManager.SendMQ_TB<NoticeMQConfig>(obj);
        }
        public static async Task<bool> SendNoticeVisit(Guid receiverUuid, Guid relationUuid)
        {
            NoticeMQ obj = new NoticeMQ();
            obj.MsgType = NoticeType.Visitor_Add;
            obj.Relation_Uuid = relationUuid;
            obj.Receiver_Uuid = receiverUuid;
            return await SendNotice(obj);
        }
        public static async Task<bool> SendNoticeFavorite(Guid receiverUuid, Guid relationUuid)
        {
            NoticeMQ obj = new NoticeMQ();
            obj.MsgType = NoticeType.Favorite_Add;
            obj.Relation_Uuid = relationUuid;
            obj.Receiver_Uuid = receiverUuid;
            return await SendNotice(obj);
        }
        #endregion
    }

    public static class WeChatNoticeReceiveHelper
    {
        static RedisManager2<WeChatRedisConfig> _redis;
        static WeChatNoticeReceiveHelper()
        {
            if (_redis == null)
                _redis = new RedisManager2<WeChatRedisConfig>();
        }


        /// <summary>
        /// 从redis中获取谁发的这条消息的集合。按照10步长取。
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns>uuid:message的格式</returns>
        static async Task<List<Message>> GetNoticeFromRedis(Guid uuid)
        {
            var db = _redis.GetDb<WeChatNoticeRedis>();
            var config = Configuration.BK_ConfigurationManager.GetConfig<UserBehaviorConfig>();
            int count = Convert.ToInt32(config.GetMessageCount);
            WeChatNoticeRedis o = new WeChatNoticeRedis();
            o.Uuid = uuid.ToString();
            string listName = _redis.GetKeyName<NoticeListAttribute>(o);
            RedisValue[] messages = await db.ListRangeAsync(listName, 0, count - 1);
            await db.ListTrimAsync(listName, 0, count - 1);
            List<string> resultStrings = _redis.ConvertRedisValueToString(messages);
            if (resultStrings == null)
                return null;
            else
            {
                List<Message> result = new List<Message>();
                foreach (var strings in resultStrings)
                {
                    result.Add(
                        WeChatNoticeHelper.GetMessageType(
                            JsonConvert.DeserializeObject<NoticeMQ>(strings)));
                    //result.Add(
                    //    WeChatNoticeHelper.GetMessageType(
                    //        BinarySerializationHelper.DeserializeObject(
                    //            BinarySerializationHelper.HexStringToBytes(strings)) as NoticeMQ));
                }
                return result;
            }
        }

        /// <summary>
        /// 获取会话中最近的一个消息
        /// </summary>
        /// <param name="fromUuid"></param>
        /// <param name="toUUid"></param>
        /// <returns></returns>
        public static async Task<Message> GetFirstNoticeFromRedis(Guid uuid)
        {
            var db = _redis.GetDb<WeChatNoticeRedis>();
            var config = Configuration.BK_ConfigurationManager.GetConfig<UserBehaviorConfig>();
            int count = Convert.ToInt32(config.GetMessageCount);
            WeChatNoticeRedis o = new WeChatNoticeRedis();
            o.Uuid = uuid.ToString();
            string listName = _redis.GetKeyName<NoticeListAttribute>(o);
            RedisValue[] messages = await db.ListRangeAsync(listName, 0, 0);
            List<string> resultStrings = _redis.ConvertRedisValueToString(messages);
            if (resultStrings != null && !string.IsNullOrEmpty(resultStrings[0]))
            {
                return WeChatNoticeHelper.GetMessageType(
                        JsonConvert.DeserializeObject<NoticeMQ>(resultStrings[0]));
                //return WeChatNoticeHelper.GetMessageType(
                //            BinarySerializationHelper.DeserializeObject(
                //                BinarySerializationHelper.HexStringToBytes(resultStrings[0])) as NoticeMQ);
            }
            else
                return null;
        }

        /// <summary>
        /// 按照一定的步长去消息。
        /// </summary>
        /// <param name="fromUuid"></param>
        /// <param name="toUuid"></param>
        /// <param name="sectionNo"></param>
        /// <returns></returns>
        public static async Task<List<Message>> GetNotice(Guid uuid, int sectionNo, bool isNeedCleanUnred = true)
        {
            var config = Configuration.BK_ConfigurationManager.GetConfig<UserBehaviorConfig>();
            int pageSize = Convert.ToInt32(config.GetMessageCount);
            int fromIndex = pageSize * sectionNo;

            if (isNeedCleanUnred)
                await NoticeRedisOp.CleanUnreadScore(uuid);

            if (sectionNo == 0)
            {
                return await GetNoticeFromRedis(uuid);
            }

            using (NoticeRepository noticeRepository = new NoticeRepository())
            {
                var list = await noticeRepository.GetNotice(fromIndex, pageSize, uuid);
                if (list == null || list.Count == 0)
                    return null;
                else
                    return list;
            }
        }
    }

    public static class WeChatNoticeHelper
    {

        public static Message GetMessageType(NoticeMQ notice)
        {
            Message msg = new Message();
            msg.ID = notice.Id;
            msg.Receiver_uuid = notice.Receiver_Uuid;
            //msg.SendTime = CommonHelper.FromUnixTime(Convert.ToDouble(notice.TimeStamp));
            msg.SendTime = notice.TimeStamp;
            msg.Status = notice.status;
            msg.MsgType = (int)notice.MsgType;
            msg.MessageInfo = notice.PayLoad as string;
            msg.RelationID_uuid = notice.Relation_Uuid;
            msg.RelationID = notice.Relation_Id;
            return msg;
        }
    }
}
