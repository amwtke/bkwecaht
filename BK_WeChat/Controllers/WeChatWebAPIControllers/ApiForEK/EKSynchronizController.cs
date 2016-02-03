﻿using BK.CommonLib.DB.Redis;
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
using BK.CommonLib.MQ;
using BK.CommonLib.DB.Redis.Objects;

namespace BK.WeChat.Controllers
{
    public class EKSynchronizController: ApiController
    {
        [Route("apiForEK/Chat/SendChat")]
        public async Task<HttpResponseMessage> PostSendChatForEK([FromBody]DualParameter postParameter)
        {
            Guid openid = Guid.Empty;
            Guid.TryParse(postParameter.openID, out openid);
            Guid uuid = postParameter.uuid;
            string textMsg = postParameter.textMsg;
            if(openid == Guid.Empty || uuid == Guid.Empty || string.IsNullOrEmpty(textMsg))
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }

            using(UserRepository userRepository = new UserRepository())
            {
                var from = userRepository.GetUserInfoByUuid(openid);
                var to = userRepository.GetUserInfoByUuid(uuid);
                if(await from == null || await to == null)
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Fail);
                var result = await WeChatSendMQHelper.SendMessage(openid.ToString().ToUpper(), uuid.ToString().ToUpper(), textMsg);
                if(result)
                {
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Success);
                }
                else
                {
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Fail);
                }
            }
        }
        [Route("apiForEK/EKToday/InsertArticle")]
        public async Task<HttpResponseMessage> PostInsertEKTodayArticleForEK([FromBody]EKToday postParameter)
        {            
            using(UserRepository userRepository = new UserRepository())
            {
                bool result = await userRepository.InsertEKToday(postParameter);
                if(result)
                {
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Success);
                }
                else
                {
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Fail);
                }
            }
        }

        [Route("apiForEK/EKToday/UpdateArticle")]
        public async Task<HttpResponseMessage> PostUpdateEKTodayArticleForEK([FromBody]EKToday postParameter)
        {
            using(UserRepository userRepository = new UserRepository())
            {
                bool result = await userRepository.UpdateEKToday(postParameter);
                if(result)
                {
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Success);
                }
                else
                {
                    return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.Fail);
                }
            }
        }
    }
}