using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BK.CommonLib.Util
{
    public static class CommonHelper
    {
        public static string GetLocalIp()
        {
            string hostname = Dns.GetHostName();
            IPHostEntry localhost = Dns.GetHostEntry(hostname);
            IPAddress localaddr = localhost.AddressList[1];
            return localaddr.ToString();
        }

        public static System.DateTime FromUnixTime(double d)
        {
            System.DateTime time = System.DateTime.MinValue;
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            time = startTime.AddSeconds(d);
            return time;
        }
        /// <summary>
        /// 将c# DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns>double</returns>
        public static double ToUnixTime(System.DateTime time)
        {
            double intResult = 0;
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            intResult = (time - startTime).TotalSeconds;
            return intResult;
        }

        public static double GetUnixTimeNow()
        {
            return ToUnixTime(DateTime.Now);
        }

        #region 本机信息
        public static string GetHostName()
        {
            return Environment.MachineName;
        }

        public static string GetLoggerName(Type t)
        {
            return t.ToString();
        }

        public static string GetThreadId()
        {
            return System.Threading.Thread.CurrentThread.ManagedThreadId.ToString(); 
        }

        public static string GetOSName()
        {
            return Environment.OSVersion.ToString();
        }

        public static string GetDomain()
        {
            return AppDomain.CurrentDomain.FriendlyName;
        }

        public static string GetLoggerDateTime(DateTime dt)
        {
            return dt.ToString("O");
        }
        #endregion
    }
}
