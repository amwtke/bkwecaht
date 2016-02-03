using BK.Model.Index;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Model.MQ
{
    [Serializable]
    public enum LogType
    {
        biz,
        log,
    }
    [Serializable]
    public class LogMQObject
    {
        public LogType Type{ get;set; }
        public LogEventMQ Event { get; set; }
        public BizMQ BizObject { get; set; }
    }

    [Serializable]
    public class LogEventMQ
    {
        public LogEventMQ()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string TimeStamp { get; set; }

        public string Message { get; set; }

        public string MessageObject { get; set; }

        public String Exception { get; set; }

        public string LoggerName { get; set; }

        public string Domain { get; set; }

        public string Id { get; set; }

        public string Level { get; set; }

        public string ClassName { get; set; }

        public string FileName { get; set; }

        public string Name { get; set; }

        public string FullInfo { get; set; }

        public string MethodName { get; set; }

        public string OS { get; set; }
        public string Properties { get; set; }
        public string UserName { get; set; }

        public string ThreadName { get; set; }
        public string HostName { get; set; }
    }

    [Serializable]
    public class BizMQ
    {
        public BizMQ(string modelname, string openid, Guid useruuid, string message)
        {
            Message = message;
            ModelName = modelname;
            User_UUID = useruuid.ToString();
            OpenId = openid;
            Id = Guid.NewGuid().ToString();
        }
        public string TimeStamp { get; set; }

        public string UserName { get; set; }

        public string LoggerName { get; set; }

        public String UserEmail { get; set; }

        public string FromUrl { get; set; }

        public string NowUrl { get; set; }

        public string UserIP { get; set; }

        /// <summary>
        /// 功能模块的名称
        /// </summary>
        public string ModelName { get; set; }

        public string SessionId { get; set; }

        public string OpenId { get; set; }

        public string Message { get; set; }

        public string User_UUID { get; set; }

        public string Id { get; set; }

        public string Platform { get; set; }

        public string UnUsed1 { get; set; }
        public string UnUsed2 { get; set; }
        public string UnUsed3 { get; set; }

        public string HostName { get; set; }
    }
}
