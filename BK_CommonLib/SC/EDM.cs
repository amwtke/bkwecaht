using CodeScales.Http;
using CodeScales.Http.Entity;
using CodeScales.Http.Entity.Mime;
using CodeScales.Http.Methods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BK.CommonLib.Util;

namespace BK.CommonLib.SC
{
    public class SendCloud
    {
        public string API_User;
        public string API_Key;
        public string Template;
        public string From;
        public string FromName;
        public List<Hashtable> listVar = new List<Hashtable>();
        public List<string> listFileName = new List<string>();
        public List<string> listSubject = new List<string>();
        public string To;
        public string Label;

        public SendCloud()
        {
            Model.Configuration.PaaS.SendCloudConfig config = BK.Configuration.BK_ConfigurationManager.GetConfig<Model.Configuration.PaaS.SendCloudConfig>();
            if(config == null)
                WebApiHelper.HttpRMtoJson(null, System.Net.HttpStatusCode.OK, customStatus.Fail);
            else
            {
                API_User = config.SMS_API_User;
                API_Key = config.SMS_API_Key;
                Template = config.RegisterValidation_TempleteId;
            }
        }

        public static string GetVarJson(List<Hashtable> listVar)
        {
            StringBuilder sb = new StringBuilder();

            StringBuilder sbTo = new StringBuilder();
            StringBuilder sbSub = new StringBuilder();
            Hashtable htSub = new Hashtable();
            for (int i = 0; i < listVar.Count; i++)
            {
                foreach (DictionaryEntry de in listVar[i])
                {
                    if (de.Key as string == "to")
                    {
                        if (sbTo.Length != 0)
                            sbTo.Append(",");
                        sbTo.Append("\"").Append(de.Value).Append("\"");
                    }
                    else
                    {
                        if (!htSub.ContainsKey(de.Key))
                        {
                            htSub[de.Key] = new StringBuilder();
                        }
                        StringBuilder sbTempSub = (StringBuilder)htSub[de.Key];
                        if (sbTempSub.Length != 0)
                            sbTempSub.Append(",");
                        sbTempSub.Append("\"").Append(de.Value).Append("\"");
                    }
                }
            }
            foreach (DictionaryEntry de in htSub)
            {
                if (sbSub.Length != 0)
                    sbSub.Append(",");
                sbSub.Append("\"").Append(de.Key).Append("\":[").Append(de.Value).Append("]");
            }
            sb.Append("{\"to\":[").Append(sbTo).Append("], \"sub\":{").Append(sbSub).Append("}}");
            //"{\"to\": [\"test@163.com\", \"test@qq.com\"], \"sub\" : { \"%name%\" : [\"name1\", \"name2\"], \"%money%\" : [\"1000\", \"2000\"]}}"
            return sb.ToString();
        }

        public static string GetVarJson(Hashtable htVar)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("{");
            foreach (DictionaryEntry de in htVar)
            {
                if (sb.Length != 1)
                    sb.Append(",");
                sb.Append("\"").Append(de.Key).Append("\":\"").Append(de.Value).Append("\"");
            }
            sb.Append("}");

            return sb.ToString();
        }

        public static string SendEmail(SendCloud sc)
        {
            string result = "";
            int iPageSize = 100;
            int iPageCount = sc.listVar.Count / iPageSize + (sc.listVar.Count % iPageSize == 0 ? 0 : 1);
            for (int i = 0; i < iPageCount; i++)
            {
                try
                {
                    string sVar = GetVarJson(sc.listVar.GetRange(i * iPageSize, i == iPageCount - 1 ? sc.listVar.Count - iPageSize * i : iPageSize));
                    sVar = sVar.Replace("\r", "").Replace("\n", "");

                    HttpClient client = new HttpClient();
                    HttpPost postMethod = new HttpPost(new Uri("http://sendcloud.sohu.com/webapi/mail.send_template.json"));

                    MultipartEntity multipartEntity = new MultipartEntity();
                    postMethod.Entity = multipartEntity;

                    multipartEntity.AddBody(new StringBody(Encoding.UTF8, "api_user", sc.API_User));
                    multipartEntity.AddBody(new StringBody(Encoding.UTF8, "api_key", sc.API_Key));
                    multipartEntity.AddBody(new StringBody(Encoding.UTF8, "from", sc.From));
                    multipartEntity.AddBody(new StringBody(Encoding.UTF8, "fromname", sc.FromName));
                    multipartEntity.AddBody(new StringBody(Encoding.UTF8, "subject", string.Join(" ", sc.listSubject.ToArray())));
                    multipartEntity.AddBody(new StringBody(Encoding.UTF8, "template_invoke_name", sc.Template));
                    multipartEntity.AddBody(new StringBody(Encoding.UTF8, "label", sc.Label));
                    multipartEntity.AddBody(new StringBody(Encoding.UTF8, "substitution_vars", sVar));

                    //for (int i = 0; i < sc.listFileName.Count; i++)
                    //{
                    //    FileInfo fileInfo = new FileInfo(sc.listFileName[i]);
                    //    FileBody fileBody = new FileBody("file" + (i + 1), fileInfo.Name, fileInfo);
                    //    multipartEntity.AddBody(fileBody);
                    //}

                    HttpResponse response = client.Execute(postMethod);

                    //Console.WriteLine("Response Code: " + response.ResponseCode);
                    //Console.WriteLine("Response Content: " + EntityUtils.ToString(response.Entity));

                    //Console.ReadLine();

                    result += sVar + Environment.NewLine;

                    if (response.ResponseCode == 200)
                    {
                        result += EntityUtils.ToString(response.Entity) + Environment.NewLine + "==============" + Environment.NewLine;
                    }
                    else
                        result += response.ResponseCode.ToString() + Environment.NewLine + "==============" + Environment.NewLine;
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
            return result;
        }

        public static string SendEmailByList(SendCloud sc)
        {
            try
            {
                HttpClient client = new HttpClient();
                HttpPost postMethod = new HttpPost(new Uri("http://sendcloud.sohu.com/webapi/mail.send_template.json"));

                MultipartEntity multipartEntity = new MultipartEntity();
                postMethod.Entity = multipartEntity;

                multipartEntity.AddBody(new StringBody(Encoding.UTF8, "api_user", sc.API_User));
                multipartEntity.AddBody(new StringBody(Encoding.UTF8, "api_key", sc.API_Key));
                multipartEntity.AddBody(new StringBody(Encoding.UTF8, "from", sc.From));
                multipartEntity.AddBody(new StringBody(Encoding.UTF8, "fromname", sc.FromName));
                multipartEntity.AddBody(new StringBody(Encoding.UTF8, "subject", string.Join(" ", sc.listSubject.ToArray())));
                multipartEntity.AddBody(new StringBody(Encoding.UTF8, "template_invoke_name", sc.Template));
                multipartEntity.AddBody(new StringBody(Encoding.UTF8, "use_maillist", "true"));
                multipartEntity.AddBody(new StringBody(Encoding.UTF8, "to", sc.To));
                multipartEntity.AddBody(new StringBody(Encoding.UTF8, "label", sc.Label));

                HttpResponse response = client.Execute(postMethod);

                return response.ResponseCode.ToString();
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public static bool SendSMS(SendCloud sc)
        {
            try
            {
                string sVar = GetVarJson(sc.listVar[0]);

                StringBuilder sb = new StringBuilder();
                sb.Append(sc.API_Key).Append("&");
                sb.Append("phone=").Append(sc.To).Append("&");
                sb.Append("smsUser=").Append(sc.API_User).Append("&");
                sb.Append("templateId=").Append(sc.Template).Append("&");
                sb.Append("vars=").Append(sVar).Append("&");
                sb.Append(sc.API_Key);
                string sSignature = Encryption.UserMd5(sb.ToString());

                HttpClient client = new HttpClient();
                HttpPost postMethod = new HttpPost(new Uri("http://sendcloud.sohu.com/smsapi/send"));

                MultipartEntity multipartEntity = new MultipartEntity();
                postMethod.Entity = multipartEntity;

                multipartEntity.AddBody(new StringBody(Encoding.UTF8, "smsUser", sc.API_User));
                multipartEntity.AddBody(new StringBody(Encoding.UTF8, "signature", sSignature));
                multipartEntity.AddBody(new StringBody(Encoding.UTF8, "templateId", sc.Template));
                multipartEntity.AddBody(new StringBody(Encoding.UTF8, "phone", sc.To));
                multipartEntity.AddBody(new StringBody(Encoding.UTF8, "vars", sVar));

                HttpResponse response = client.Execute(postMethod);

                if (response.ResponseCode == 200)
                {
                    string entity = EntityUtils.ToString(response.Entity);
                    Hashtable ht = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Hashtable>(entity);
                    if ((string)ht["message"] == "请求成功" && (bool)ht["result"])
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

    }
}
