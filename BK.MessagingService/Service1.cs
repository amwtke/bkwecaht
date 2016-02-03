using BK.CommonLib.DB.Redis;
using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.Log;
using BK.CommonLib.MQ;
using BK.CommonLib.Util;
using BK.Model.Configuration.Redis;
using BK.Model.MQ;
using BK.Model.Redis.Objects.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using BK.CommonLib.DB.Redis.Objects;

namespace BK.MessagingService
{
    public partial class Service1 : ServiceBase
    {
        static RedisManager2<WeChatRedisConfig> _redis;
        static ConcurrentDictionary<string, string> _sidToListName = new ConcurrentDictionary<string, string>();
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            LogHelper.LogInfoAsync(typeof(Service1), "BK.Messaging服务开始启动...");
            init();
            LogHelper.LogInfoAsync(typeof(Service1), "BK.Messaging服务开始启动成功！");
        }

        protected override void OnStop()
        {
            try
            {
                _redis.Close();
                MQManager.CloseAll();
            }
            catch (Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(Service1), ex);
            }
        }

        private void init()
        {
            _redis = new RedisManager2<WeChatRedisConfig>();
            MQManager.RegisterConsumerProcessor<BK.Model.Configuration.MQ.WeChatMessageMQConfig>(async delegate (BasicDeliverEventArgs ar, IModel channel)
            {
                try
                {
                    var body = ar.Body;
                    var rawObject = BinarySerializationHelper.DeserializeObject(body) as ChatMessageMQ;
                    //log
                    //StringBuilder sb = new StringBuilder();
                    //sb.AppendLine("msg from:" + rawObject.From);
                    //sb.AppendLine("msg to:" + rawObject.To);
                    //sb.AppendLine("msg uuid:" + rawObject.Uuid);
                    //sb.AppendLine("msg sessionid:" + rawObject.SessionId);
                    //sb.AppendLine("msg time:" + CommonHelper.FromUnixTime(Convert.ToDouble(rawObject.TimeStamp)).ToString());
                    //sb.AppendLine("msg message:" + rawObject.PayLoad.ToString());
                    //LogHelper.LogInfoAsync(typeof(Service1), sb.ToString());

                    //记录日志

                    //更新收信方的未读列表数，更新发送方的时间排序
                    //收信人的未读列表加1
                    await MessageRedisOp.AddUnreadScore(rawObject.To, rawObject.SessionId, 1);
                    //LogHelper.LogInfoAsync(typeof(Service1), "数字是：" + d.ToString());

                    //更新双方的session timestamp
                    await MessageRedisOp.SetOrUpdateTimestampToNow(rawObject.From, rawObject.SessionId);

                    await MessageRedisOp.SetOrUpdateTimestampToNow(rawObject.To, rawObject.SessionId);

                    using (MessageRepository r = new MessageRepository())
                    {
                        try
                        {
                            Guid uid = Guid.Parse(rawObject.Uuid);
                            DateTime t = CommonHelper.FromUnixTime(Convert.ToDouble(rawObject.TimeStamp));
                            await r.AddChatLogAsync(uid, rawObject.From, rawObject.To, rawObject.PayLoad.ToString(), rawObject.SessionId, t);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.LogErrorAsync(typeof(Service1), ex);
                        }

                    }

                    //放入redis
                    //uuid:message
                    try
                    {
                        WeChatMessageRedis redisObject = new WeChatMessageRedis();
                        redisObject.SessionId = rawObject.SessionId;
                        redisObject.Message = makeRedisListMessage(rawObject.From, rawObject.TimeStamp, rawObject.PayLoad.ToString());
                        //rawObject.From + ":" + rawObject.PayLoad.ToString();
                        //LogHelper.LogInfoAsync(typeof(Service1), redisObject.Message);
                        await _redis.SaveObjectAsync(redisObject);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogErrorAsync(typeof(Service1), ex);
                    }

                }
                catch (Exception ex)
                {
                    LogHelper.LogErrorAsync(typeof(Service1), ex);

                    //TODO:错误处理
                }
                finally
                {
                    channel.BasicAck(ar.DeliveryTag, false);
                    System.Threading.Thread.Sleep(1);
                }
            });

            //EK Comments
            MQManager.RegisterConsumerProcessor<BK.Model.Configuration.MQ.EKCommentMQConfig>(async delegate (BasicDeliverEventArgs ar, IModel channel)
            {
                try
                {
                    var body = ar.Body;
                    var rawObject = BinarySerializationHelper.DeserializeObject(body) as EKCommentMQObject;
                    

                    //TODO Reis update???


                    using (MessageRepository r = new MessageRepository())
                    {
                        try
                        {
                            Guid uid = Guid.Parse(rawObject.uuid);
                            await r.AddEKCommentAsync(uid,Guid.Parse(rawObject.From),rawObject.To,rawObject.Content,rawObject.Timestamp);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.LogErrorAsync(typeof(Service1), ex);
                        }

                    }

                    //放入redis
                    //uuid:message
                    try
                    {
                        EKCommentRedis redisObject = new EKCommentRedis();
                        redisObject.Id = rawObject.To;
                        redisObject.Comment = makeRedisListMessage(rawObject.From, CommonLib.Util.CommonHelper.ToUnixTime(rawObject.Timestamp), rawObject.Content.ToString());
                       
                        await _redis.SaveObjectAsync(redisObject);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogErrorAsync(typeof(Service1), ex);
                    }

                }
                catch (Exception ex)
                {
                    LogHelper.LogErrorAsync(typeof(Service1), ex);

                    //TODO:错误处理
                }
                finally
                {
                    channel.BasicAck(ar.DeliveryTag, false);
                    System.Threading.Thread.Sleep(1);
                }
            });

            //Paper Comments
            MQManager.RegisterConsumerProcessor<BK.Model.Configuration.MQ.PaperCommentMQConfig>(async delegate (BasicDeliverEventArgs ar, IModel channel)
            {
                try
                {
                    var body = ar.Body;
                    var rawObject = BinarySerializationHelper.DeserializeObject(body) as PCommentMQObject;

                    //存入数据库
                    using (MessageRepository r = new MessageRepository())
                    {
                        try
                        {
                            Guid uid = Guid.Parse(rawObject.uuid);
                            await r.AddPaperCommentAsync(uid, Guid.Parse(rawObject.From), rawObject.To, rawObject.Content, rawObject.Timestamp);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.LogErrorAsync(typeof(Service1), ex);
                        }

                    }

                    //放入redis
                    //uuid:message
                    try
                    {
                        PaperCommentRedis redisObject = new PaperCommentRedis();
                        redisObject.Id = rawObject.To;
                        redisObject.Comment = makeRedisListMessage(rawObject.From, CommonLib.Util.CommonHelper.ToUnixTime(rawObject.Timestamp), rawObject.Content.ToString());

                        await _redis.SaveObjectAsync(redisObject);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogErrorAsync(typeof(Service1), ex);
                    }

                }
                catch (Exception ex)
                {
                    LogHelper.LogErrorAsync(typeof(Service1), ex);
                }
                finally
                {
                    channel.BasicAck(ar.DeliveryTag, false);
                    System.Threading.Thread.Sleep(1);
                }
            });

            MQManager.Prepare_All_C_MQ();
        }

        string getListName(string sessionId)
        {
            string listName = null;
            if (_sidToListName.TryGetValue(sessionId, out listName))
                return listName;
            var db = _redis.GetDb<WeChatMessageRedis>();
            WeChatMessageRedis o = new WeChatMessageRedis();
            o.SessionId = sessionId;
            listName = _redis.GetKeyName<WeChatMessageListAttribute>(o);
            _sidToListName[sessionId] = listName;
            return listName;
        }

        string makeRedisListMessage(string from,double timeStamp,string message)
        {
            string ret = from + ":" + timeStamp.ToString() + ":" + message;
            return ret;
        }
    }
}
