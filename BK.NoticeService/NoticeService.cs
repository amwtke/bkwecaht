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
using System;
using System.ServiceProcess;
using System.Collections.Concurrent;
using BK.CommonLib.DB.Redis.Objects;
using BK.Model.Configuration.MQ;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BK.NoticeService
{
    public partial class NoticeService : ServiceBase
    {
        static RedisManager2<WeChatRedisConfig> _redis;
        static ConcurrentDictionary<string, string> _sidToListName = new ConcurrentDictionary<string, string>();
        public NoticeService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            LogHelper.LogInfoAsync(typeof(NoticeService), "微信通知服务开始启动...");
            init();
            LogHelper.LogInfoAsync(typeof(NoticeService), "服务通知开始启动成功！");
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
                LogHelper.LogErrorAsync(typeof(NoticeService), ex);
            }
        }

        private void init()
        {
            _redis = new RedisManager2<WeChatRedisConfig>();
            MQManager.RegisterConsumerProcessor<NoticeMQConfig>(async delegate (BasicDeliverEventArgs ar, IModel channel)
            {
                try
                {
                    var rawObject = BinarySerializationHelper.DeserializeObject(ar.Body) as NoticeMQ;


                    var iso = new IsoDateTimeConverter();
                    iso.DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

                    if(rawObject.Id == 0)
                    {
                        //放入sqlserver 
                        rawObject.Id = await NoticeHelper.SaveNoticeToSql(rawObject);
                        if(rawObject.Id != 0)
                        {
                            //收信人的未读列表加1
                            await NoticeRedisOp.AddUnreadScore(rawObject.Receiver_Uuid, 1);
                            //放入redis
                            try
                            {
                                WeChatNoticeRedis redisObject = new WeChatNoticeRedis();
                                redisObject.Uuid = rawObject.Receiver_Uuid.ToString();
                                redisObject.Message = JsonConvert.SerializeObject(rawObject, iso);
                                //redisObject.Message = BinarySerializationHelper.BytesToHexString(
                                //    BinarySerializationHelper.SerializeObject(rawObject));
                                await _redis.SaveObjectAsync(redisObject);
                            }
                            catch(Exception ex)
                            {
                                LogHelper.LogErrorAsync(typeof(NoticeService), ex);
                            }
                        }
                    }
                    else
                    {
                        //按照id修改sqlserver
                        await NoticeHelper.SaveNoticeToSql(rawObject);
                        //按照id修改redis
                        try
                        {
                            WeChatNoticeRedis redisObject = new WeChatNoticeRedis();
                            redisObject.Uuid = rawObject.Receiver_Uuid.ToString();
                            redisObject.Message = JsonConvert.SerializeObject(rawObject, iso);
                            //redisObject.Message = BinarySerializationHelper.BytesToHexString(
                            //    BinarySerializationHelper.SerializeObject(rawObject));

                            rawObject.status = 0;
                            WeChatNoticeRedis originalRedisObject = new WeChatNoticeRedis();
                            originalRedisObject.Uuid = rawObject.Receiver_Uuid.ToString();
                            originalRedisObject.Message = JsonConvert.SerializeObject(rawObject, iso);
                            //originalRedisObject.Message = BinarySerializationHelper.BytesToHexString(
                            //    BinarySerializationHelper.SerializeObject(rawObject));

                            if(!await _redis.ReplaceObjectInListAsync(redisObject, originalRedisObject))
                                LogHelper.LogInfoAsync(typeof(NoticeService), "替换失败");
                        }
                        catch(Exception ex)
                        {
                            LogHelper.LogErrorAsync(typeof(NoticeService), ex);
                        }
                    }

                }
                catch (Exception ex)
                {
                    LogHelper.LogErrorAsync(typeof(NoticeService), ex);

                    //TODO:错误处理
                }
                finally
                {
                    channel.BasicAck(ar.DeliveryTag, false);
                    System.Threading.Thread.Sleep(1);
                }
            });

            MQManager.Prepare_All_C_MQ();
        }
    }
}
