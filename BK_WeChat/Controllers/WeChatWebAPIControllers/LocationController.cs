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
using BK.Model.Index;
using BK.CommonLib.ElasticSearch;

namespace BK.WeChat.Controllers
{
    public class LocationController: ApiController
    {
        /// <summary>
        /// 附近的人 页面初始化
        /// post api/Location/Initialize
        /// </summary>
        /// <param name="postParameter">openid: radius:距离 pageIndex: pageSize:
        /// isBusiness: 0教授 2学生 值留空只传参数名为不筛选
        /// gender: 012定义同微信取得的值 值留空只传参数名为不筛选
        /// researchFieldId: 研究领域的id 值留空只传参数名为不筛选</param>
        /// <returns>
        /// InvalidArguments 参数不正确
        /// NotFound 未发现位置信息，请开启位置服务
        /// Success 成功
        /// </returns>
        [Route("api/Location/Initialize")]
        [UserBehaviorFilter]
        public async Task<HttpResponseMessage> PostInitialize([FromBody]LocationParameter postParameter)
        {
            string openid = postParameter.openID;
            int? radius = postParameter.Radius;
            int pageIndex = postParameter.pageIndex;
            int pageSize = postParameter.pageSize;

            int? isBusiness = postParameter.IsBusiness;
            int? gender = postParameter.Gender;
            int? researchFieldId = postParameter.ResearchFieldId;

            if(string.IsNullOrEmpty(openid) || pageIndex == 0 || pageSize == 0)
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            if(radius == null)
                radius = 3;

            List<UserInfo> lcList = new List<UserInfo>();
            if(researchFieldId == 0)
            {
                using(UserRepository userRepository = new UserRepository())
                {
                    UserInfo ui = await userRepository.GetUserInfoByOpenid(openid);
                    if(ui != null)
                        researchFieldId = (int?)ui.ResearchFieldId;
                    else
                        researchFieldId = null;
                }
            }
            List<ComplexLocation> locationList = await ComplexLocationManager.GetDistanceInKmByIdAsync(openid, (int)radius, isBusiness, gender, researchFieldId);

            if(locationList == null)
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.NotFound);
            }
            if(locationList.Count > 0)
            {
                ComplexLocation myLocation = null;
                //有可能第一个值是自己
                if(locationList[0].Id == openid)
                {
                    myLocation = locationList[0];
                    locationList.RemoveAt(0);
                }
                else
                {
                    myLocation = await ComplexLocationManager.GetLocationObjectByOpenidAsync(openid);
                }
                //只有自己
                if(locationList.Count == 0)
                {
                    return WebApiHelper.HttpRMtoJson(lcList, HttpStatusCode.OK, customStatus.Success);
                }

                using(UserRepository userRepository = new UserRepository())
                {
                    //去除未绑定等
                    for(int i = 0; i < locationList.Count; i++)
                    {
                        if(!await userRepository.IsUserOpenidExist(locationList[i].Id))
                        {
                            locationList.RemoveAt(i);
                        }
                    }
                    //保留分页内的数据
                    int itemCount = locationList.Count;
                    int PageCount = PageCount = itemCount % pageSize == 0 ? itemCount / pageSize : itemCount / pageSize + 1;
                    if(pageIndex > 0)
                    {
                        locationList = locationList.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                    }
                    //取出userinfo数据 填充距离数据
                    foreach(ComplexLocation l in locationList)
                    {
                        var ui = await userRepository.GetUserInfoByOpenid(l.Id);
                        if(ui != null)
                        {
                            ui.Distance = ComplexLocationManager.GetDistanceBetween(myLocation.Coordinate, l.Coordinate);
                            lcList.Add(ui);
                        }
                    }
                    return WebApiHelper.HttpRMtoJson(Tuple.Create(itemCount, PageCount, lcList), HttpStatusCode.OK, customStatus.Success);
                }
            }
            else
                return WebApiHelper.HttpRMtoJson(lcList, HttpStatusCode.OK, customStatus.Success);
        }




    }
}
