using BK.CommonLib.DB.Redis;
using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.Log;
using BK.Model.Configuration.MQ;
using BK.Model.Configuration.Redis;
using BK.Model.Configuration.User;
using BK.Model.MQ;
using BK.Model.Redis.Objects.Messaging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.CommonLib.MQ
{
    public static class EKCommentMessageMQOp
    {
        static RedisManager2<WeChatRedisConfig> _redis;
        static EKCommentMessageMQOp()
        {
            if (_redis == null)
                _redis = new RedisManager2<WeChatRedisConfig>();
            try
            {
                MQManager.Prepare_All_P_MQ();
            }
            catch (Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(WeChatSendMQHelper), ex);
                throw ex;
            }
        }

        public static bool SendMessage(string fromUuid, long toEKid, string content)
        {
            fromUuid = fromUuid.Trim().ToUpper();

            EKCommentMQObject obj = new EKCommentMQObject();
            obj.uuid = Guid.NewGuid().ToString().ToUpper();
            obj.From = fromUuid.ToUpper();
            obj.To = toEKid;
            obj.Content = content;
            obj.Timestamp = DateTime.Now;

            return MQManager.SendMQ_TB<EKCommentMQConfig>(obj);
        }


        static async Task<List<string>> GetContentFromRedis(long toEK)
        {
            var db = _redis.GetDb<EKCommentRedis>();
            var config = BK.Configuration.BK_ConfigurationManager.GetConfig<UserBehaviorConfig>();
            int count = Convert.ToInt32(config.GetMessageCount);
            EKCommentRedis o = new EKCommentRedis();
            o.Id = toEK;
            string listName = _redis.GetKeyName<EKCommentListAttribute>(o);
            RedisValue[] messages = await db.ListRangeAsync(listName, 0, count - 1);
            await db.ListTrimAsync(listName, 0, count - 1);
            return _redis.ConvertRedisValueToString(messages);
        }

        public static async Task<List<string>> GetMessage(long toEK, int sectionNo)
        {
            var config = BK.Configuration.BK_ConfigurationManager.GetConfig<UserBehaviorConfig>();
            int pageSize = Convert.ToInt32(config.GetMessageCount);
            int fromIndex = pageSize * sectionNo;

            if (sectionNo == 0)
            {
                return await GetContentFromRedis(toEK);
            }

            using (MessageRepository r = new MessageRepository())
            {
                var list = await r.GetEKCommentAsync(toEK,fromIndex, pageSize);
                if (list == null || list.Count == 0)
                    return null;
                List<string> ret = new List<string>();
                foreach (var l in list)
                {
                    double time = CommonLib.Util.CommonHelper.ToUnixTime(l.timestamp);
                    string from = l.from.ToString();
                    string message = from + ":" + time.ToString() + ":" + l.content;
                    ret.Add(message);
                }
                return ret;
            }
        }
    }
}
