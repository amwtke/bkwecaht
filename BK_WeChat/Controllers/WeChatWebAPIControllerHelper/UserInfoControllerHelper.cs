using System.Threading.Tasks;
using BK.CommonLib.DB.Redis;
using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.Util;
using BK.CommonLib.Weixin.User;
using BK.Model.DB;
using BK.Model.Redis.Objects;
using BK.WeChat.Controllers.WeChatWebAPIParameters;

namespace BK.WeChat.Controllers.WeChatWebAPIControllerHelper
{
    public static class UserInfoControllerHelper
    {
        public static async Task<bool> SaveUserPreRegisterToRedis(string preRegisterOpenid, string preRegisterAccount = "", string preRegisterValidationCode = "", string preRegisterTryTimes = "")
        {
            UserInfoRedis obj = new UserInfoRedis();
            obj.PreRegisterAccount = preRegisterAccount;
            obj.PreRegisterValidationCode = preRegisterValidationCode;
            obj.PreRegisterTryTimes = preRegisterTryTimes;
            obj.Openid = preRegisterOpenid;

            return await RedisManager.SaveObjectAsync(obj);
        }

        public static async Task<UserInfoRedis> GetUserInfoRedisByOpenid(string openid)
        {
            return await RedisManager.GetObjectFromRedis<UserInfoRedis>(openid);
        }

        public static async Task<bool> SaveUserInfo(ComplementParameter complementParameter, pre_register preRegister)
        {
            bool result = false;
            UserInfoRedis userinfoRedis = await GetUserInfoRedisByOpenid(complementParameter.openID);
            UserInfo userinfo = null;
            using (UserRepository userRepository = new UserRepository())
            {
                userinfo = await userRepository.GetUserInfoByOpenid(complementParameter.openID);
                if (userinfo == null)
                {
                    userinfo = new UserInfo()
                    {
                        AccountEmail = preRegister.accountemail,
                        Password = preRegister.password,
                        CreateTime = preRegister.createtime,
                        Name = preRegister.name,
                        Gender = userinfoRedis.Sex,
                        Phone = preRegister.accountemail.Substring(0, 11),
                        IsBusiness = preRegister.validate
                    };
                }
                else
                {
                    userinfo.Name = complementParameter.name;
                    userinfo.Gender = complementParameter.Gender;
                }
                await CheckUserInfoPhoto(userinfo, userinfoRedis);

                userinfo.Unit = complementParameter.university;
                userinfo.Faculty = complementParameter.faculty;
                userinfo.ResearchFieldId = complementParameter.researchFieldId;
                userinfo.Degree = complementParameter.degree;
                if (userinfo.IsBusiness==2)
                    userinfo.Enrollment = complementParameter.enrollment;
                if (userinfo.IsBusiness == 0 && !string.IsNullOrEmpty(complementParameter.position))
                    userinfo.Position = complementParameter.position;
                userinfo.Province = complementParameter.province;
                userinfo.City = complementParameter.city;
                userinfo.Birthday = complementParameter.birthday;
                userinfo.HometownProvince = complementParameter.hometownProvince;
                userinfo.HometownCity = complementParameter.hometownCity;
                userinfo.Position = complementParameter.position;
                result = await userRepository.SaveUserInfo(userinfo);
                if (result)
                    await userRepository.SaveUserOpenid(userinfo.uuid, userinfoRedis.Openid, userinfoRedis.Unionid);
            }
            return result;
        }
        public static async Task<bool> CheckUserInfoPhoto(UserInfo userinfo, UserInfoRedis userinfoRedis)
        {
            bool result = false;
            if(string.IsNullOrEmpty(userinfo.Photo) || userinfo.Photo == "pic/header/HeaderDefault.jpg")
            {
                userinfo.Photo = WebApiHelper.UploadHeadPic(userinfoRedis.HeadImageUrl.Substring(0, userinfoRedis.HeadImageUrl.LastIndexOf("/0")));
                result = true;
            }
            if(!result && await WXAuthHelper.IsTester(userinfoRedis.Openid))
            {
                userinfo.Photo = WebApiHelper.UploadHeadPic(userinfoRedis.HeadImageUrl.Substring(0, userinfoRedis.HeadImageUrl.LastIndexOf("/0")));
                result = true;
            }
            return result;
        }

    }

}