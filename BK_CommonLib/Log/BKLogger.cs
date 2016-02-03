using BK.CommonLib.MQ;
using BK.CommonLib.Util;
using BK.Model.Index;
using BK.Model.MQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.CommonLib.Log
{
    public static class BKLogger
    {
        public static void LogInfoAsync(Type t, object message)
        {
            var obj = LogMQOP.GenLogEvent(t, message.ToString());
            LogMQOP.SendMessageAsync(obj);
        }

        public static void LogInfo(Type t, object message)
        {
            var obj = LogMQOP.GenLogEvent(t, message.ToString());
            LogMQOP.SendMessage(obj);
        }

        public static void LogBizAsync(Type t,BizMQ bo)
        {
            bo.TimeStamp = CommonHelper.GetLoggerDateTime(DateTime.Now);
            bo.LoggerName = CommonHelper.GetLoggerName(t);
            bo.HostName = CommonHelper.GetHostName();
            LogMQOP.SendMessageAsync(bo);
        }

        public static void LogBiz(Type t,BizMQ bo)
        {
            bo.TimeStamp = CommonHelper.GetLoggerDateTime(DateTime.Now);
            bo.LoggerName = CommonHelper.GetLoggerName(t);
            bo.HostName = CommonHelper.GetHostName();
            LogMQOP.SendMessage(bo);
        }

        public static void LogErrorAsync(Type t, Exception ex)
        {
            var obj = LogMQOP.GenLogEvent(t,"ERROR",ex);
            LogMQOP.SendMessageAsync(obj);
        }

        public static void LogError(Type t, Exception ex)
        {
            var obj = LogMQOP.GenLogEvent(t, "ERROR", ex);
            LogMQOP.SendMessage(obj);
        }
    }
}
