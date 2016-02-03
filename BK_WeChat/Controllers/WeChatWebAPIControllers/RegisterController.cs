using BK.CommonLib.DB.Redis;
using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.Util;
using BK.Model.DB;
using BK.WeChat.Controllers.WeChatWebAPIControllerHelper;
using BK.WeChat.Controllers.Base;
using BK.WeChat.Controllers.WeChatWebAPIParameters;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using BK.Model.Redis.Objects;
using BK.CommonLib.ElasticSearch;

namespace BK.WeChat.Controllers
{
    public class RegisterController : ApiController
    {
        UserInfo userinfo;
        /// <summary>
        /// 获取验证码
        /// post api/Register/ValidationCode
        /// </summary>
        /// <param name="registerParam">account: openid:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// AccountExist 账号已存在
        /// Success 成功
        /// 
        /// </returns>
        [Route("api/Register/ValidationCode")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostValidationCode([FromBody]RegisterParameter registerParam)
        {
            string sNewAccount = registerParam.account;
            string validationCode = (new Random()).Next(1000, 9999).ToString();
            string openid = registerParam.openID;

            //对应openid从redis取出验证码 若没有 生成验证码 存入redis **20分钟有效
            var userinfoRedis = await UserInfoControllerHelper.GetUserInfoRedisByOpenid(openid);
            //注册测试用的白名单
            if(string.IsNullOrEmpty(userinfoRedis.PreRegisterValidationCode) || await CommonLib.Weixin.User.WXAuthHelper.IsTester(openid))
            {
                await UserInfoControllerHelper.SaveUserPreRegisterToRedis(openid, preRegisterValidationCode: validationCode);
            }
            else
            {
                validationCode = userinfoRedis.PreRegisterValidationCode;
            }

            if(string.IsNullOrEmpty(sNewAccount) || string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            if(!sNewAccount.Contains("@"))
                sNewAccount += "@phone.51science.cn";
            using(UserRepository userRepository = new UserRepository())
            {
                userinfo = await userRepository.GetUserInfoByAccount(sNewAccount);
                //注册测试用的白名单
                if(userinfo != null && !await CommonLib.Weixin.User.WXAuthHelper.IsTester(openid))
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.AccountExist);
                else
                {
                    WebApiHelper.SendValidStringSMS(validationCode, sNewAccount.Substring(0, 11));

                    //向redis里存入手机号 防止故意验证失败后又改其他手机号注册 向redis里存入验证次数
                    await UserInfoControllerHelper.SaveUserPreRegisterToRedis(openid, preRegisterAccount: sNewAccount, preRegisterTryTimes: "10");

                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Success);
                }
            }
        }

        /// <summary>
        /// 提交新用户数据
        /// post api/Register/Register
        /// </summary>
        /// <param name="registerParam">account: openid: validationCode: name: password: </param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// AccountExist 账号已存在
        /// Success 成功 跳至注册信息完善页面
        /// NotFound 验证码不存在
        /// Fail 验证码错误
        /// Forbidden 超过了尝试次数
        /// </returns>
        [Route("api/Register/Register")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostRegister([FromBody]RegisterParameter registerParam)
        {
            string sNewAccount = registerParam.account;
            string validationCode = registerParam.validationCode;
            string name = registerParam.name;
            string password = Encryption.EncryptMD5(registerParam.password);
            string openid = registerParam.openID;

            if(string.IsNullOrEmpty(sNewAccount) || string.IsNullOrEmpty(openid) || string.IsNullOrEmpty(validationCode) || string.IsNullOrEmpty(name) || password == Encryption.EncryptMD5(""))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            if(!sNewAccount.Contains("@"))
                sNewAccount += "@phone.51science.cn";

            using(UserRepository userRepository = new UserRepository())
            {
                userinfo = await userRepository.GetUserInfoByAccount(sNewAccount);
                //注册测试用的白名单
                if(userinfo != null && !await CommonLib.Weixin.User.WXAuthHelper.IsTester(openid))
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.AccountExist);
                else
                {
                    // 从redis里取出手机号、验证码 防止故意验证失败后又改其他手机号注册 从redis里取出验证次数
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
                            return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Forbidden);
                        }

                        if(validationCode != preRegisterValidationCode)
                        {
                            return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Fail);
                        }
                    }
                    else
                    {
                        return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);
                    }
                    await userRepository.SavePreRegister(sNewAccount, name, password,2, openid);
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Success);
                }
            }

        }


        /// <summary>
        /// 提交新用户数据 教授
        /// post api/Register/RegisterPro
        /// </summary>
        /// <param name="registerParam">account: openid: validationCode: name: password: </param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// AccountExist 账号已存在
        /// Success 成功 跳至注册信息完善页面
        /// NotFound 验证码不存在
        /// Fail 验证码错误
        /// Forbidden 超过了尝试次数
        /// </returns>
        [Route("api/Register/RegisterPro")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostRegisterPro([FromBody]RegisterParameter registerParam)
        {
            string sNewAccount = registerParam.account;
            string validationCode = registerParam.validationCode;
            string name = registerParam.name;
            string password = Encryption.EncryptMD5(registerParam.password);
            string openid = registerParam.openID;

            if(string.IsNullOrEmpty(sNewAccount) || string.IsNullOrEmpty(openid) || string.IsNullOrEmpty(validationCode) || string.IsNullOrEmpty(name) || password == Encryption.EncryptMD5(""))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            if(!sNewAccount.Contains("@"))
                sNewAccount += "@phone.51science.cn";

            using(UserRepository userRepository = new UserRepository())
            {
                userinfo = await userRepository.GetUserInfoByAccount(sNewAccount);
                //注册测试用的白名单
                if(userinfo != null && !await CommonLib.Weixin.User.WXAuthHelper.IsTester(openid))
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.AccountExist);
                else
                {
                    // 从redis里取出手机号、验证码 防止故意验证失败后又改其他手机号注册 从redis里取出验证次数
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
                            return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Forbidden);
                        }

                        if(validationCode != preRegisterValidationCode)
                        {
                            return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Fail);
                        }
                    }
                    else
                    {
                        return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);
                    }
                    //教授validate值为0
                    await userRepository.SavePreRegister(sNewAccount, name, password, 0, openid);
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Success);
                }
            }

        }

        /// <summary>
        /// 完善新用户数据 页面初始化
        /// post api/Register/Initialize
        /// </summary>
        /// <param name="registerParam">openid:</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 账号不存在
        /// Success 成功 跳至注册信息完善页面
        /// </returns>
        [Route("api/Register/Initialize")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitialize([FromBody]RegisterParameter registerParam, [FromUri]string type)
        {
            string openid = registerParam.openID;
            if(string.IsNullOrEmpty(openid))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            pre_register prereg = null;
            using(UserRepository userRepository = new UserRepository())
            {
                var userinfoRedis = await BK.CommonLib.Weixin.User.WXAuthHelper.GetUserInfoByOPenId(openid);

                if(type != "update")
                {
                    prereg = await userRepository.GetPreRegisterByOpenid(openid);

                    if(prereg == null)
                    {
                        return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);
                    }
                    userinfo = new UserInfo() {
                        Photo = userinfoRedis.HeadImageUrl.Substring(0, userinfoRedis.HeadImageUrl.LastIndexOf("/0")) + "/96",
                        Name = prereg.name,
                        Gender = userinfoRedis.Sex,
                    };
                }
                else
                {
                    userinfo = await userRepository.GetUserInfoByOpenid(openid);
                    userinfo.Gender = userinfoRedis.Sex;
                }

                return WebApiHelper.HttpRMtoJson(userinfo, HttpStatusCode.OK, customStatus.Success);

            }
        }

        /// <summary>
        /// 完善新用户数据 提交
        /// post api/Register/Complement
        /// </summary>
        /// <param name="registerParam">openid: university: faculty: researchFieldId: degree: enrollment:
        /// province: city: [birthday:] [hometownProvince ] [hometownCity]
        /// </param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// AccountExist 账号已存在
        /// NotFound 预注册信息不存在
        /// Success 成功 跳至个人页面
        /// Fail 保存信息失败
        /// </returns>
        [Route("api/Register/Complement")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostComplement([FromBody]ComplementParameter registerParam, [FromUri]string type)
        {
            string openid = registerParam.openID;
            string university = registerParam.university;
            string faculty = registerParam.faculty;
            int researchFieldId = registerParam.researchFieldId;
            short degree = registerParam.degree;
            short enrollment = registerParam.enrollment;
            string gender = registerParam.Gender;
            string name = registerParam.name;
            string position = registerParam.position;
            short isBusiness = registerParam.isBusiness;
            if (string.IsNullOrEmpty(openid) || string.IsNullOrEmpty(university) || string.IsNullOrEmpty(faculty) || researchFieldId == 0 || string.IsNullOrEmpty(gender) || string.IsNullOrEmpty(name))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            if (string.IsNullOrEmpty(position) && isBusiness == 0)
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            if (isBusiness == 2 && enrollment == 0)
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            var preRegisterRedis = await UserInfoControllerHelper.GetUserInfoRedisByOpenid(openid);
            string preRegisterAccount = preRegisterRedis.PreRegisterAccount;
            pre_register preRegister = null;

            if (type != "update")
            {
                using (UserRepository userRepository = new UserRepository())
                {
                    userinfo = await userRepository.GetUserInfoByAccount(preRegisterAccount);
                    preRegister = await userRepository.GetPreRegisterByOpenid(openid);
                }
                if (userinfo != null)
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.AccountExist);
                if (preRegister == null)
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);
            }

            //更新数据库的userinfo
            bool result = await UserInfoControllerHelper.SaveUserInfo(registerParam, preRegister);
            if (result)
            {
                //更新redis的性别
                UserInfoRedis redisUser = new UserInfoRedis();
                redisUser.Openid = openid;
                redisUser.Sex = gender;
                await RedisManager.SaveObjectAsync(redisUser);

                using (UserRepository userRepository = new UserRepository())
                {
                    userinfo = await userRepository.GetUserInfoByOpenid(openid);
                }

                if (await ComplexLocationManager.UpdateComplexLocationAsync(openid, userinfo.IsBusiness ?? 0, int.Parse(userinfo.Gender), userinfo.ResearchFieldId ?? 0))
                {
                    //位置索引添加供筛选字段
                }

                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Success);
            }
            else
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Fail);
            }
        }

        /// <summary>
        /// 根据关键字匹配学校名
        /// post api/Register/SchoolName
        /// </summary>
        /// <param name="registerParam">name: </param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// NotFound 未找到
        /// </returns>
        [Route("api/Register/SchoolName")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostSchoolName([FromBody]RegisterParameter registerParam)
        {
            string name = registerParam.name;
            List<Univs> univsList = null;
            using(SystemRepository systemRepository = new SystemRepository())
            {
                univsList = await systemRepository.GetUniversityNameByKeyword(name);
                if(univsList.Count == 0)
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);
                else
                    return WebApiHelper.HttpRMtoJson(univsList, HttpStatusCode.OK, customStatus.Success);
            }

        }

        /// <summary>
        /// 根据学校列出学院
        /// post api/Register/Faculty
        /// </summary>
        /// <param name="registerParam">name: 上面获取到的UnivsID</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// NotFound 未找到
        /// </returns>
        [Route("api/Register/Faculty")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostFacultyName([FromBody]RegisterParameter registerParam)
        {
            string name = registerParam.name;
            List<T_UnivsDep> udList = null;
            using(SystemRepository systemRepository = new SystemRepository())
            {
                udList = await systemRepository.GetFacultyNameByUnivsID(name);
                if(udList.Count == 0)
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);
                else
                    return WebApiHelper.HttpRMtoJson(udList, HttpStatusCode.OK, customStatus.Success);
            }

        }

        /// <summary>
        /// 根据专业分类号查询子专业 一级：0 二级：一级编号 三级：二级编号
        /// post api/Register/Faculty
        /// </summary>
        /// <param name="registerParam">name: 编号</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// NotFound 未找到
        /// </returns>
        [Route("api/Register/ResearchField")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostResearchFieldName([FromBody]RegisterParameter registerParam)
        {
            int name = 0;
            if(!int.TryParse(registerParam.name, out name))
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);

            List<ResearchField> rfList = null;
            using(SystemRepository systemRepository = new SystemRepository())
            {
                rfList = await systemRepository.GetResearchFieldByFartherID(name);
                if(rfList.Count == 0)
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);
                else
                    return WebApiHelper.HttpRMtoJson(rfList, HttpStatusCode.OK, customStatus.Success);
            }

        }

        /// <summary>
        /// 查询国家数据
        /// post api/Register/CountryName
        /// </summary>
        /// <param name="registerParam"></param>
        /// <returns>
        /// Success 成功
        /// NotFound 未找到
        /// </returns>
        [Route("api/Register/CountryName")]
        public async Task<HttpResponseMessage> PostCountryName()
        {
            using(SystemRepository systemRepository = new SystemRepository())
            {
                IList<Country> rfList = await systemRepository.GetCountryNames();
                if(rfList.Count == 0)
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);
                else
                    return WebApiHelper.HttpRMtoJson(rfList, HttpStatusCode.OK, customStatus.Success);
            }
        }
        /// <summary>
        /// 查询省数据
        /// post api/Register/ProvinceName
        /// </summary>
        /// <param name="registerParam"></param>
        /// <returns>
        /// Success 成功
        /// NotFound 未找到
        /// </returns>
        [Route("api/Register/ProvinceName")]
        public async Task<HttpResponseMessage> PostProvinceName()
        {
            using(SystemRepository systemRepository = new SystemRepository())
            {
                IList<Province> rfList = await systemRepository.GetProvinceNames();
                if(rfList.Count == 0)
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);
                else
                    return WebApiHelper.HttpRMtoJson(rfList, HttpStatusCode.OK, customStatus.Success);
            }
        }
        /// <summary>
        /// 查询市数据
        /// post api/Register/Faculty
        /// </summary>
        /// <param name="registerParam">name: 编号</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// Success 成功
        /// NotFound 未找到
        /// </returns>
        [Route("api/Register/CityName")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostCityName([FromBody]RegisterParameter registerParam)
        {
            int name = 0;
            if(!int.TryParse(registerParam.name, out name))
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);

            using(SystemRepository systemRepository = new SystemRepository())
            {
                IList<City> rfList = await systemRepository.GetCityNamesByProvinceId(name);
                if(rfList.Count == 0)
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);
                else
                    return WebApiHelper.HttpRMtoJson(rfList, HttpStatusCode.OK, customStatus.Success);
            }


        }
    }
}
