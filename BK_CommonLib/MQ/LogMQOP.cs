using BK.CommonLib.Util;
using BK.Model.Configuration.MQ;
using BK.Model.Index;
using BK.Model.MQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.CommonLib.MQ
{
    public static class LogMQOP
    {
        static LogMQOP()
        {
            try
            {
                MQManager.Prepare_All_P_MQ();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private static LogMQObject GenObject(object obj)
        {
            LogMQObject ret = null;
            if(obj is LogEventMQ)
            {
                ret = new LogMQObject();
                ret.Type = LogType.log;
                ret.Event = obj as LogEventMQ;
            }
            else if(obj is BizMQ)
            {
                ret = new LogMQObject();
                ret.Type = LogType.biz;
                ret.BizObject = obj as BizMQ;
            }
            return ret;
        }

        public static LogEventMQ GenLogEvent(Type t,string message,Exception ex=null,LogLevel level = LogLevel.INFO)
        {
            LogEventMQ ret = new LogEventMQ();
            ret.HostName = CommonHelper.GetHostName();
            ret.Level = level.ToString();
            ret.ThreadName = CommonHelper.GetThreadId();
            ret.Domain = CommonHelper.GetDomain();
            ret.OS = CommonHelper.GetOSName();
            ret.LoggerName = CommonHelper.GetLoggerName(t);
            ret.TimeStamp = CommonHelper.GetLoggerDateTime(DateTime.Now);
            if (ex != null)
            {
                ret.Exception = ex.ToString() + ex.StackTrace;
                ret.Level = LogLevel.ERROR.ToString();
            }
            ret.Message = message;
            return ret;
        }

        public static BizMQ GenBizIndex(Type t,string modelname, string openid, Guid useruuid, string message)
        {
            BizMQ index = new BizMQ(modelname, openid, useruuid, message);
            index.LoggerName = CommonHelper.GetLoggerName(t);
            index.TimeStamp = CommonHelper.GetLoggerDateTime(DateTime.Now);
            index.UserIP = CommonHelper.GetHostName();
            return index;
        }

        public static bool SendMessage(object obj)
        {
            LogMQObject log = GenObject(obj);
            if(log!=null)
                return MQManager.SendMQ_TB<LogMQConfig>(obj);
            return false;
        }

        public static void SendMessageAsync(object obj)
        {
            LogMQObject log = GenObject(obj);
            if (log != null)
                MQManager.SendMQ<LogMQConfig>(obj);
        }
    }
}
