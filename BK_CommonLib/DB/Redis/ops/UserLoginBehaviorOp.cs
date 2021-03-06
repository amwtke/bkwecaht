﻿using BK.Model.Configuration.Redis;
using BK.Model.Configuration.User;
using BK.Model.Redis.Objects.UserBehavior;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.CommonLib.DB.Redis.Objects
{
    public static class UserLoginBehaviorOp
    {
        /// <summary>
        /// 访问API接口次数的累加.Zset.
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        public static async Task<double> AddLoginCountAsync(string openid)
        {
            string zsetName = RedisManager.GetKeyName<UserLoginRedis, UserLoginBehaviorCountZsetAttribute>();
            var db = RedisManager.GetRedisDB<UserLoginRedis>();
            if (!await db.KeyExistsAsync(zsetName))
            {
                await db.SortedSetAddAsync(zsetName, openid, 1);
                return 1;
            }
            else
                return await RedisManager.IncreaseScoreAsync<UserLoginRedis>(zsetName, openid, 1);
        }

        public static double AddLoginCount(string openid)
        {
            string zsetName = RedisManager.GetKeyName<UserLoginRedis, UserLoginBehaviorCountZsetAttribute>();
            var db = RedisManager.GetRedisDB<UserLoginRedis>();
            if (!db.KeyExists(zsetName))
            {
                db.SortedSetAdd(zsetName, openid, 1);
                return 1;
            }
            else
                return RedisManager.IncreaseScore<UserLoginRedis>(zsetName, openid, 1);
        }

        /// <summary>
        /// 根据openid获取调用接口的次数。
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        public static async Task<double> GetLoginCountByOpenidAsync(string openid)
        {
            //string zsetName = RedisManager.GetKeyName<UserLoginRedis, UserLoginBehaviorCountZsetAttribute>();
            //var db = RedisManager.GetRedisDB<UserLoginRedis>();

            var manager =new RedisManager2<WeChatRedisConfig>();
            string zsetName = manager.GetKeyName<UserLoginRedis, UserLoginBehaviorCountZsetAttribute>();
            var db = manager.GetDb<UserLoginRedis>();
            double? value = await db.SortedSetScoreAsync(zsetName, openid);
            return value.GetValueOrDefault();
        }


        /// <summary>
        ///按照登陆次数的排序列表。zset。
        /// </summary>
        /// <param name="f"></param>
        /// <param name="t"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public static async Task<KeyValuePair<string,double>[]> GetLoginCountRangesAsync(long f,long t,string o="d")
        {
            string zsetName = RedisManager.GetKeyName<UserLoginRedis, UserLoginBehaviorCountZsetAttribute>();
            var db = RedisManager.GetRedisDB<UserLoginRedis>();
            if (o == "d")
                return await RedisManager.GetRangeByRankWithScore<UserLoginRedis>(zsetName, Order.Descending, f, t);
            else
                return await RedisManager.GetRangeByRankWithScore<UserLoginRedis>(zsetName, Order.Ascending, f, t);
        }

        /// <summary>
        /// 插入或者更新一个用户的最后访问webapi方法的时间。
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        public static async Task<bool> AddUpdateLastLoginTimeAsync(string openid)
        {
            string zsetName = RedisManager.GetKeyName<UserLoginRedis, UserLoginBehaviorLastTimeZsetAttribute>();
            var db = RedisManager.GetRedisDB<UserLoginRedis>();
            double timeNow = CommonLib.Util.CommonHelper.GetUnixTimeNow();
            return await db.SortedSetAddAsync(zsetName, openid, timeNow);
        }

        public static bool AddUpdateLastLoginTime(string openid)
        {
            string zsetName = RedisManager.GetKeyName<UserLoginRedis, UserLoginBehaviorLastTimeZsetAttribute>();
            var db = RedisManager.GetRedisDB<UserLoginRedis>();
            double timeNow = CommonLib.Util.CommonHelper.GetUnixTimeNow();
            return db.SortedSetAdd(zsetName, openid, timeNow);
        }

        /// <summary>
        /// 获取一个用户最后访问WebApi的时间，如果不存在则范围DateTime最小时间。
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        public static async Task<DateTime> GetLastLoginTimeAsync(string openid)
        {
            string zsetName = RedisManager.GetKeyName<UserLoginRedis, UserLoginBehaviorLastTimeZsetAttribute>();
            var db = RedisManager.GetRedisDB<UserLoginRedis>();
            double? timeUnix = await db.SortedSetScoreAsync(zsetName, openid);
            if(timeUnix.HasValue)
            {
                DateTime value = CommonLib.Util.CommonHelper.FromUnixTime(timeUnix.Value);
                return value;
            }
            return DateTime.MinValue;
        }


        /// <summary>
        ///按照登陆时间的排序列表。zset。
        /// </summary>
        /// <param name="f"></param>
        /// <param name="t"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public static async Task<KeyValuePair<string, double>[]> GetLoginLastLoginTimeRangesAsync(long f, long t, string o = "d")
        {
            string zsetName = RedisManager.GetKeyName<UserLoginRedis, UserLoginBehaviorLastTimeZsetAttribute>();
            var db = RedisManager.GetRedisDB<UserLoginRedis>();
            if (o == "d")
                return await RedisManager.GetRangeByRankWithScore<UserLoginRedis>(zsetName, Order.Descending, f, t);
            else
                return await RedisManager.GetRangeByRankWithScore<UserLoginRedis>(zsetName, Order.Ascending, f, t);
        }

        /// <summary>
        /// 判断用户是否在线。按照访问webapi的最后时间10分钟以内算是在线。
        /// </summary>
        /// <param name="openid"></param>
        /// <returns>r如果出现异常返回false</returns>
        public static async Task<bool> IsUserOnlineAsync(string openid)
        {
            UserBehaviorConfig config = BK.Configuration.BK_ConfigurationManager.GetConfig<UserBehaviorConfig>();
            if (config != null)
            {
                int timeSpanConfig = Convert.ToInt32(config.LoginTimeSpanMin);
                DateTime time = await GetLastLoginTimeAsync(openid);
                if(time>DateTime.MinValue)
                {
                    TimeSpan span = DateTime.Now - time;
                    return span <= TimeSpan.FromMinutes(timeSpanConfig);
                }
            }
            return false;
        }
    }
}
