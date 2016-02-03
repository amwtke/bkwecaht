using BK.CommonLib.ElasticSearch;
using BK.CommonLib.MQ;
using BK.CommonLib.Util;
using BK.Model.Configuration.MQ;
using BK.Model.Index;
using BK.Model.MQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace BK.LogService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            MQManager.RegisterConsumerProcessor<LogMQConfig>(delegate (BasicDeliverEventArgs ar, IModel channel)
            {
                var body = ar.Body;
                object obj = BinarySerializationHelper.DeserializeObject(body);

                if (obj != null)
                {
                    if (obj is BizMQ)
                    {
                        var env = obj as BizMQ;
                        if (env != null && !string.IsNullOrEmpty(env.Id) && !Guid.Parse(env.Id).Equals(Guid.Empty))
                        {
                            BizIndex index = LogESManager.CopyFromBizMQ(env);
                            LogESManager.AddOrUpdateBiz(index);
                        }
                    }
                    if (obj is LogEventMQ)
                    {
                        var env = obj as LogEventMQ;
                        if (env != null && !string.IsNullOrEmpty(env.Id) && !Guid.Parse(env.Id).Equals(Guid.Empty))
                        {
                            var index = LogESManager.CopyFromLogEventMQ(env);
                            LogESManager.AddOrUpdateLogEvent(index);
                        }
                    }

                }
                channel.BasicAck(ar.DeliveryTag, false);
                System.Threading.Thread.Sleep(10);
            });
            MQManager.Prepare_All_C_MQ();
        }

        protected override void OnStop()
        {
            MQManager.CloseAll();
        }
    }
}
