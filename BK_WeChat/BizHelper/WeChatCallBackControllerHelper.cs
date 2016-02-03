using BK.CommonLib.DB.Redis;
using BK.CommonLib.Log;
using BK.CommonLib.Util;
using BK.CommonLib.Weixin.User;
using BK.Model.Redis;
using BK.Model.Redis.Objects;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace BK.WeChat.BizHelper
{
    public static class WeChatCallBackControllerHelper
    {
        /// <summary>
        /// 在微信页面认证后如果需要绑定用户的OPenid，则需要存储必要的用户信息。
        /// 头像，昵称等。
        /// </summary>
        /// <param name="userinfo"></param>
        /// <returns></returns>
        public static async Task<bool> SaveOAuthUserInfoToRedis(OAuthUserInfo userinfo)
        {
            LogHelper.LogInfoAsync(typeof(object), "nick name is:" + userinfo.nickname);
            UserInfoRedis obj = new UserInfoRedis();
            obj.City = userinfo.city;
            obj.Country = userinfo.country;
            obj.Province = userinfo.province;
            obj.HeadImageUrl = userinfo.headimgurl;
            obj.NiceName = userinfo.nickname;
            obj.Sex = userinfo.sex.ToString();
            obj.Openid = userinfo.openid;
            obj.Unionid = userinfo.unionid;

            return await RedisManager.SaveObjectAsync(obj);
        }

        public static async Task<bool> SaveOAuthUserTokenAsync(OAuthAccessTokenResult result)
        {
            double now = CommonHelper.GetUnixTimeNow();

            UserInfoRedis u = new UserInfoRedis();
            u.Openid = result.openid;
            u.Unionid = result.unionid;
            u.AccessToken = result.access_token;
            u.ExpireIn = (now + result.expires_in).ToString();
            u.RefreshToken = result.refresh_token;
            return await RedisManager.SaveObjectAsync(u);
        }
    }
    
}