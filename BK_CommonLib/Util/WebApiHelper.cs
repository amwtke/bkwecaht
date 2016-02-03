using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace BK.CommonLib.Util
{
    public class WebApiHelper
    {

        public static HttpResponseMessage HttpRMtoJson(object obj, HttpStatusCode statusCode, customStatus customStatus)
        {
            string str;
            ResponseJsonMessage rjm = new ResponseJsonMessage(customStatus.ToString(), obj);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            str = serializer.Serialize(rjm);
            HttpResponseMessage result = new HttpResponseMessage() { StatusCode = statusCode, Content = new StringContent(str, Encoding.GetEncoding("UTF-8"), "application/json") };
            return result;
        }
        public static HttpResponseMessage HttpRMtoJson(string jsonpCallback, object obj, HttpStatusCode statusCode, customStatus customStatus)
        {
            string str;
            ResponseJsonMessage rjm = new ResponseJsonMessage(customStatus.ToString(), obj);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if(string.IsNullOrEmpty(jsonpCallback))
                str = serializer.Serialize(rjm);
            else
                str = jsonpCallback + "(" + serializer.Serialize(rjm) + ");";
            HttpResponseMessage result = new HttpResponseMessage() { StatusCode = statusCode, Content = new StringContent(str, Encoding.GetEncoding("UTF-8"), "application/json") };
            return result;
        }
        [Serializable]
        public class ResponseJsonMessage
        {
            public string CustomStatus = "";
            public string QiniuHeadPicDomain = GetHeadPicUrl("");
            public object Message = null;
            public ResponseJsonMessage(string customStatus, object message)
            {
                CustomStatus = customStatus;
                Message = message;
            }
        }

        public static bool SendValidStringSMS(string sValidString, string sTo)
        {
            Hashtable ht = new Hashtable();
            ht["%code%"] = sValidString;

            SC.SendCloud sc = new SC.SendCloud();
            sc.To = sTo;
            sc.listVar = new List<Hashtable>() { ht };

            bool result = SC.SendCloud.SendSMS(sc);

            return result;
        }
        public static string UploadHeadPic(string url)
        {
            string prefix = "Pic/Header";
            string fileName = url.Substring(url.LastIndexOf("/"));
            try
            {
                WebClient wc = new WebClient();
                Qiniu.QiniuHelper qnhelper = new Qiniu.QiniuHelper();
                if(qnhelper.PutFile(prefix + fileName, wc.OpenRead(url + "/96")))
                    return prefix + fileName;
                else
                    return "";
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }
        public static string GetHeadPicUrl(string fileName)
        {
            Qiniu.QiniuHelper qnhelper = new Qiniu.QiniuHelper();
            return qnhelper.GetFileUrl(fileName);
        }
        public static string GetEKArticlePicUrl(string fileName)
        {
            Qiniu.QiniuHelper qnhelper = new Qiniu.QiniuHelper(Qiniu.QiniuBucket.EKArticle);
            return qnhelper.GetFileUrl(fileName);
        }

        /// <summary>
        /// 将文章插图的地址转义 固定转义字串为"[QiniuEKIllustrationDomain]"
        /// </summary>
        /// <param name="eki"></param>
        /// <returns></returns>
        public static string GetEscapedBodyText(Model.Index.EKIndex eki)
        {
            return eki.BodyText.Replace("[QiniuEKIllustrationDomain]", GetEKArticlePicUrl("Pic/EKIllustration/"));
        }
        public static string GetQiniuEKArticleHeadPic(Model.Index.EKIndex eki)
        {
            return GetEKArticlePicUrl("") + eki.HeadPic;
        }

    }

    public enum customStatus
    {
        InvalidArguments,
        Forbidden,
        Inactive,
        Success,
        WrongPassowrd,
        NotFound,
        AccountExist,
        Fail,
        ErrorValidationCode,
        NoValidationCode,
    }

}
