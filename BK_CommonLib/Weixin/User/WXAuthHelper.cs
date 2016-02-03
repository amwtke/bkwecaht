using BK.CommonLib.DB.Redis;
using BK.CommonLib.Log;
using BK.CommonLib.Util;
using BK.Model.Configuration;
using BK.Model.Redis.att.CustomAtts.sets;
using BK.Model.Redis.Att.CustomAtts.Sets;
using BK.Model.Redis.Objects;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.CommonLib.Weixin.User
{
    public static class WXAuthHelper
    {
        static object lockObject = new object();
        /// <summary>
        /// 获取Auth相关db在redis中的Db号。
        /// </summary>
        /// <returns></returns>

        /// <summary>
        /// 获取第三方认证的登陆链接.约束：必须在wechat.51science.cn这个域名底下的url才行。
        /// 默认是http://wechat.51science.cn/wxcallback。
        /// </summary>
        /// <returns></returns>
        public static string GetAuthUrl(string url = "http://wechat.51science.cn/wxcallback")
        {
            var config = BK.Configuration.BK_ConfigurationManager.GetConfig<WeixinConfig>();
            return OAuthApi.GetAuthorizeUrl(config.WeixinAppId, url, config.WeixinToken, OAuthScope.snsapi_userinfo);
        }

        /// <summary>
        /// 当页面授权回调时获取openid时使用。
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="secretCode"></param>
        /// <param name="callBackCode"></param>
        /// <param name="saveUserInfo"></param>
        /// <returns></returns>
        public static async Task<string> GetOpenIdWXCallBackAsync(string appid, string secretCode, string callBackCode, Func<OAuthAccessTokenResult, Task<bool>> isNeedGetUserInfoFunc,Func<OAuthUserInfo,Task<bool>> saveUserinfo)
        {
            if (isNeedGetUserInfoFunc == null || saveUserinfo == null)
                throw new Exception("GetOpenIdWXCallBackAsync->judgeFunc与SaveUseinfo必须补全！");
            try
            {
                OAuthAccessTokenResult userAt = await OAuthApi.GetAccessTokenAsync(appid, secretCode, callBackCode);

                if (!string.IsNullOrEmpty(userAt.openid))
                {
                    if (await isNeedGetUserInfoFunc(userAt))
                    {
                        OAuthAccessTokenResult token = await OAuthApi.RefreshTokenAsync(appid, userAt.refresh_token);
                        var userinfo = await OAuthApi.GetUserInfoAsync(token.access_token, userAt.openid);
                        if (await saveUserinfo(userinfo))
                            return userAt.openid;
                        else
                            throw new Exception("Userinfo保存失败！");
                    }
                }
                return userAt.openid;
            }
            catch (Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(WXAuthHelper), ex);
                throw ex;
            }
        }

        #region user tester related
        public static async Task<UserInfoRedis> GetUserInfoByOPenId(string openid)
        {
            return await RedisManager.GetObjectFromRedis<UserInfoRedis>(openid);
        }

        public static UserInfoRedis GetUserInfoByOPenId_TongBu(string openid)
        {
            return RedisManager.GetObjectFromRedis_TongBu<UserInfoRedis>(openid);
        }

        public static string GetUserinfoOpenIdSetName()
        {
            return RedisManager.GetKeyName<UserInfoRedis, UserInfoSetOpenIdSetAttribute>();
        }

        public static IDatabase GetUserinfoDB()
        {
            return RedisManager.GetRedisDB<UserInfoRedis>();
        }
        /// <summary>
        /// 移除测试用户
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        public static async Task<bool> RemoveTester(string openid)
        {
            string setname = RedisManager.GetKeyName<TesterRedis, TesterOpenIdSetAttribute>();//"tester." + GetUserinfoSetName();
            var db = GetUserinfoDB();
            return await db.SetRemoveAsync(setname, openid);
        }

        /// <summary>
        /// 添加测试用户
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        public static async Task<bool> AddTester(string openid)
        {
            string setname = RedisManager.GetKeyName<TesterRedis, TesterOpenIdSetAttribute>();//"tester." + GetUserinfoSetName();
            var db = GetUserinfoDB();
            return await db.SetAddAsync(setname, openid);
        }
        /// <summary>
        /// 是否为测试用户
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        public static async Task<bool> IsTester(string openid)
        {
            var setname = RedisManager.GetKeyName<TesterRedis, TesterOpenIdSetAttribute>();//"tester." + GetUserinfoSetName();
            var db = GetUserinfoDB();
            return await db.SetContainsAsync(setname, openid);
        }

       /// <summary>
       /// 获取所有测试用户
       /// </summary>
       /// <returns></returns>
        public static async Task<List<string>> GetAllTesters()
        {
            var setkey = RedisManager.GetKeyName<TesterRedis, TesterOpenIdSetAttribute>();//"tester." + GetUserinfoSetName();
            var db = GetUserinfoDB();
            return RedisManager.ConvertRedisValueToString(await db.SetMembersAsync(setkey));
        }
        #endregion
    }
}
