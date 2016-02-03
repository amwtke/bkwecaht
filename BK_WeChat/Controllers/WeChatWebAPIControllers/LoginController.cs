using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.Util;
using BK.Model.DB;
using BK.WeChat.Controllers.Base;
using BK.WeChat.Controllers.WeChatWebAPIParameters;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using BK.WeChat.Controllers.WeChatWebAPIControllerHelper;
using BK.CommonLib.ElasticSearch;

namespace BK.WeChat.Controllers
{
    public class LoginController: ApiController
    {
        UserInfo userinfo;
        /// <summary>
        /// 通过账号密码绑定用户uuid到openid
        /// post api/Login/UserLoginAccountPassword
        /// </summary>
        /// <param name="userlogin">{account: openid:}</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Forbidden 账号被禁用
        /// Inactive 账号未激活
        /// Success 绑定成功
        /// Fail 绑定失败
        /// WrongPassowrd 密码错误
        /// NotFound 账号不存在
        /// </returns>
        [Route("api/Login/UserLoginAccountPassword")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostUserLoginAccountPassword([FromBody]LoginParameter userlogin)
        {
            string account = userlogin.account;
            string password = userlogin.password;
            string openid = userlogin.openID;
            if(string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(openid))
                return WebApiHelper.HttpRMtoJson(userlogin.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            //13163331326@phone.51science.cn
            if(!account.Contains("@"))
                account += "@phone.51science.cn";
            password = Encryption.EncryptMD5(userlogin.password);
            using(UserRepository userRepository = new UserRepository())
            {
                userinfo = await userRepository.GetUserInfoByAccountPassword(account, password);
                if(userinfo != null)
                {
                    if(userinfo.Status == 1)
                        return WebApiHelper.HttpRMtoJson(userlogin.jsonpCallback, userinfo, HttpStatusCode.OK, customStatus.Forbidden);
                    else
                    {
                        if(userinfo.IsLogin == 0)
                        {
                            userinfo.IsLogin = 1;
                            userinfo.LastLogin = DateTime.MinValue;
                        }
                        var userinfoRedis = await UserInfoControllerHelper.GetUserInfoRedisByOpenid(openid);

                        if(await UserInfoControllerHelper.CheckUserInfoPhoto(userinfo, userinfoRedis))
                        {
                            //photo已更新为微信头像
                        }
                        if(await ComplexLocationManager.UpdateComplexLocationAsync(openid, userinfo.IsBusiness ?? 0, int.Parse(userinfo.Gender), userinfo.ResearchFieldId ?? 0))
                        {
                            //位置索引添加供筛选字段
                        }
                        if(await userRepository.SaveUserOpenid(userinfo.uuid, openid, userinfoRedis.Unionid))
                            return WebApiHelper.HttpRMtoJson(userlogin.jsonpCallback, userinfo, HttpStatusCode.OK, customStatus.Success);
                        else
                            return WebApiHelper.HttpRMtoJson(userlogin.jsonpCallback, userinfo, HttpStatusCode.OK, customStatus.Fail);

                    }

                }
                else
                {

                    userinfo = await userRepository.GetUserInfoByAccount(account);
                    if(userinfo != null)
                        return WebApiHelper.HttpRMtoJson(userlogin.jsonpCallback, null, HttpStatusCode.OK, customStatus.WrongPassowrd);
                    else
                        return WebApiHelper.HttpRMtoJson(userlogin.jsonpCallback, null, HttpStatusCode.OK, customStatus.NotFound);
                }
            }
        }
        /// <summary>
        /// 获取验证码
        /// post api/Login/ValidationCode
        /// </summary>
        /// <param name="registerParam">account: openid:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 账号不存在
        /// Success 成功
        /// </returns>
        [Route("api/Login/ValidationCode")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostValidationCode([FromBody]RegisterParameter registerParam)
        {
            string sNewAccount = registerParam.account;
            string validationCode = (new Random()).Next(1000, 9999).ToString();
            string openid = registerParam.openID;

            //对应openid从redis取出验证码 若没有 生成验证码 存入redis **20分钟有效
            var userinfoRedis = await UserInfoControllerHelper.GetUserInfoRedisByOpenid(openid);
            //注册测试用的白名单
            if(string.IsNullOrEmpty(userinfoRedis.PreRegisterValidationCode))
            {
                await UserInfoControllerHelper.SaveUserPreRegisterToRedis(openid, preRegisterValidationCode: validationCode);
            }
            else
            {
                validationCode = userinfoRedis.PreRegisterValidationCode;
            }

            if(string.IsNullOrEmpty(sNewAccount) || string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(registerParam.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            if(!sNewAccount.Contains("@"))
                sNewAccount += "@phone.51science.cn";
            using(UserRepository userRepository = new UserRepository())
            {
                userinfo = await userRepository.GetUserInfoByAccount(sNewAccount);
            }
            if(userinfo == null)
                return WebApiHelper.HttpRMtoJson(registerParam.jsonpCallback, null, HttpStatusCode.OK, customStatus.NotFound);
            else
            {
                WebApiHelper.SendValidStringSMS(validationCode, sNewAccount.Substring(0, 11));

                //向redis里存入手机号 防止故意验证失败后又改其他手机号注册 向redis里存入验证次数
                await UserInfoControllerHelper.SaveUserPreRegisterToRedis(openid, preRegisterAccount: sNewAccount, preRegisterTryTimes: "10");

                return WebApiHelper.HttpRMtoJson(registerParam.jsonpCallback, null, HttpStatusCode.OK, customStatus.Success);
            }
        }

        /// <summary>
        /// 通过验证后将新密码保存
        /// post api/Login/UserLoginResetPassword
        /// </summary>
        /// <param name="userlogin">{account: password: openid: validationCode:}</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 修改成功
        /// Fail 修改失败
        /// NotFound 账号不存在
        /// NoValidationCode 验证码不存在
        /// ErrorValidationCode 验证码错误
        /// Forbidden 超过了尝试次数
        /// </returns>
        [Route("api/Login/UserLoginResetPassword")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostUserLoginResetPassword([FromBody]RegisterParameter registerParam)
        {
            string sNewAccount = registerParam.account;
            string validationCode = registerParam.validationCode;
            string password = Encryption.EncryptMD5(registerParam.password);
            string openid = registerParam.openID;

            if(string.IsNullOrEmpty(sNewAccount) || string.IsNullOrEmpty(openid) || string.IsNullOrEmpty(validationCode) || password == Encryption.EncryptMD5(""))
            {
                return WebApiHelper.HttpRMtoJson(registerParam.jsonpCallback, null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            if(!sNewAccount.Contains("@"))
                sNewAccount += "@phone.51science.cn";

            using(UserRepository userRepository = new UserRepository())
            {
                userinfo = await userRepository.GetUserInfoByAccount(sNewAccount);
            }
            if(userinfo == null)
                return WebApiHelper.HttpRMtoJson(registerParam.jsonpCallback, null, HttpStatusCode.OK, customStatus.NotFound);
            else
            {
                var preRegisterRedis = await UserInfoControllerHelper.GetUserInfoRedisByOpenid(openid);

                string preRegisterAccount = preRegisterRedis.PreRegisterAccount;
                string preRegisterValidationCode = preRegisterRedis.PreRegisterValidationCode;

                int preRegisterTryTimes = 0;
                int.TryParse(preRegisterRedis.PreRegisterTryTimes, out preRegisterTryTimes);

                if(string.IsNullOrEmpty(preRegisterAccount) || sNewAccount != preRegisterAccount)
                {
                    await UserInfoControllerHelper.SaveUserPreRegisterToRedis(openid, preRegisterValidationCode: "");
                    preRegisterValidationCode = "";
                }

                if(!string.IsNullOrEmpty(preRegisterValidationCode))
                {
                    if(preRegisterTryTimes > 0)
                    {
                        await UserInfoControllerHelper.SaveUserPreRegisterToRedis(openid, preRegisterTryTimes: (preRegisterTryTimes - 1).ToString());
                    }
                    else
                    {
                        await UserInfoControllerHelper.SaveUserPreRegisterToRedis(openid, "", "", "");
                        return WebApiHelper.HttpRMtoJson(registerParam.jsonpCallback, null, HttpStatusCode.OK, customStatus.Forbidden);
                    }

                    if(validationCode != preRegisterValidationCode)
                    {
                        return WebApiHelper.HttpRMtoJson(registerParam.jsonpCallback, null, HttpStatusCode.OK, customStatus.ErrorValidationCode);
                    }
                }
                else
                {
                    return WebApiHelper.HttpRMtoJson(registerParam.jsonpCallback, null, HttpStatusCode.OK, customStatus.NoValidationCode);
                }
                bool result = false;
                using(UserRepository userRepository = new UserRepository())
                {
                    result = await userRepository.UpdateUserinfoPassword(sNewAccount, password);
                }
                if(result)
                    return WebApiHelper.HttpRMtoJson(registerParam.jsonpCallback, null, HttpStatusCode.OK, customStatus.Success);
                else
                    return WebApiHelper.HttpRMtoJson(registerParam.jsonpCallback, null, HttpStatusCode.OK, customStatus.Fail);
            }
        }
    }
}
