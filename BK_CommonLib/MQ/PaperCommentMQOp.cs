using BK.CommonLib.DB.Redis;
using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.Log;
using BK.Model.Configuration.MQ;
using BK.Model.Configuration.Redis;
using BK.Model.Configuration.User;
using BK.Model.MQ;
using BK.Model.Redis.Objects.Messaging;
using BK.Model.Redis.Objects.paper;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.CommonLib.MQ
{
    public static class PaperCommentMQOp
    {
        static RedisManager2<WeChatRedisConfig> _redis;
        static PaperCommentMQOp()
        {
            if (_redis == null)
                _redis = new RedisManager2<WeChatRedisConfig>();
            try
            {
                MQManager.Prepare_All_P_MQ();
            }
            catch (Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(PaperCommentMQOp), ex);
                throw ex;
            }
        }

        public static bool SendMessage(string fromUuid, long id, string content)
        {
            fromUuid = fromUuid.Trim().ToUpper();

            PCommentMQObject obj = new PCommentMQObject();
            obj.uuid = Guid.NewGuid().ToString().ToUpper();
            obj.From = fromUuid.ToUpper();
            obj.To = id;
            obj.Content = content;
            obj.Timestamp = DateTime.Now;

            return MQManager.SendMQ_TB<PaperCommentMQConfig>(obj);
        }


        static async Task<List<string>> GetContentFromRedis(long id)
        {
            var db = _redis.GetDb<PaperCommentRedis>();
            var config = BK.Configuration.BK_ConfigurationManager.GetConfig<UserBehaviorConfig>();
            int count = Convert.ToInt32(config.GetMessageCount);
            PaperCommentRedis o = new PaperCommentRedis();
            o.Id = id;
            string listName = _redis.GetKeyName<PCommentListAttribute>(o);
            RedisValue[] messages = await db.ListRangeAsync(listName, 0, count - 1);
            await db.ListTrimAsync(listName, 0, count - 1);
            return _redis.ConvertRedisValueToString(messages);
        }

        public static async Task<List<string>> GetMessage(long id, int sectionNo)
        {
            var config = BK.Configuration.BK_ConfigurationManager.GetConfig<UserBehaviorConfig>();
            int pageSize = Convert.ToInt32(config.GetMessageCount);
            int fromIndex = pageSize * sectionNo;

            if (sectionNo == 0)
            {
                return await GetContentFromRedis(id);
            }

            using (MessageRepository r = new MessageRepository())
            {
                var list = await r.GetPaperCommentAsync(id, fromIndex, pageSize);
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
