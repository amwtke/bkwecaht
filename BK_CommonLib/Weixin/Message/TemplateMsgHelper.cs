using BK.CommonLib.Weixin.Token;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.CommonLib.Weixin.Message
{

    public static class TemplateMsgHelper
    {
        static Dictionary<TemplateType, string> _dic = new Dictionary<TemplateType, string>();
        static TemplateMsgHelper()
        {
            _dic.Add(TemplateType.Notify, "ka2R_eIXtkfkuXpDJIFcLJyBKPCWVoyWzei93WxjfKA");
        }

        /// <summary>
        /// 获取模板消息的id
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTemplateId(TemplateType type)
        {
            string id;
            return _dic.TryGetValue(type, out id) ? id : null;
        }

        public static SendTemplateMessageResult Send(string toOpenId,string tempId,object data, string url, string topColor= "#0066CC")
        {
            var result = TemplateApi.SendTemplateMessage(WXTokenHelper.GetSiteAccessTokenFromRedis(), toOpenId, tempId, topColor, url, data);
            return result;
        }

        public static async Task<SendTemplateMessageResult> SendAsync(string toOpenId, string tempId, object data, string url, string topColor = "#0066CC")
        {
            var result = await TemplateApi.SendTemplateMessageAsync(WXTokenHelper.GetSiteAccessTokenFromRedis(), toOpenId, tempId, topColor, url, data);
            return result;
        }
    }
}
