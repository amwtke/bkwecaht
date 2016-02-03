using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.ElasticSearch;
using BK.CommonLib.Log;
using BK.CommonLib.MQ;
using BK.CommonLib.Util;
using BK.Model.DB;
using BK.Model.MQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace BK.WeChat.Controllers
{
    public partial class WeixinAPIController : ApiController
    {
        #region 学术地位
        [Route("acerro")]
        [HttpGet]
        public List<Guid> GetUCError()
        {
            List<Guid> ret = new List<Guid>();
            using (UserRepository repo = new UserRepository())
            {
                foreach(var v in repo.GetAllUserAcadmic())
                {
                    if(!string.IsNullOrEmpty(v.Association) && string.IsNullOrEmpty(v.AssociationPost))
                    {
                        if (!ret.Contains(v.AccountEmail_uuid))
                            ret.Add(v.AccountEmail_uuid);
                    }

                    if (!string.IsNullOrEmpty(v.Magazine) && string.IsNullOrEmpty(v.MagazinePost))
                    {
                        if (!ret.Contains(v.AccountEmail_uuid))
                            ret.Add(v.AccountEmail_uuid);
                    }


                    if (!string.IsNullOrEmpty(v.Fund) && string.IsNullOrEmpty(v.FundPost))
                    {
                        if (!ret.Contains(v.AccountEmail_uuid))
                            ret.Add(v.AccountEmail_uuid);
                    }
                }
            }
            return ret;
        }
        [Route("GetErrorUserAcademicById")]
        [HttpGet]
        public async Task<List<UserAcademic>> GetUCErrorUserAcademic(Guid uuid)
        {
            List<UserAcademic> ret = new List<UserAcademic>();

            using (UserRepository repo = new UserRepository())
            {
                ret = await repo.GetUserRecordsByUuid<UserAcademic>(uuid);
            }

            return ret;
        }

        [Route("SaveOrUpdateUserAcademic")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostSaveUserAcademic([FromBody]PostUserAcademicParameter parameter)
        {
            if(parameter.UserAcademic == null)
            {
                return WebApiHelper.HttpRMtoJson(null, HttpStatusCode.OK, customStatus.InvalidArguments);
            }
            using (UserRepository repo = new UserRepository())
            {
                UserAcademic tmp = parameter.UserAcademic;
                bool flag = await repo.SaveOrUpdateUserAcadmic(tmp);
                return WebApiHelper.HttpRMtoJson(flag, HttpStatusCode.OK, customStatus.Success);
            }
        }

        #endregion

        #region logservice

        [Route("testex")]
        [HttpGet]
        public bool TestException()
        {
            BKLogger.LogErrorAsync(typeof(WeixinAPIController), new Exception("测试错误！" + DateTime.Now.ToString()));
            BizMQ obj = new BizMQ("log", "oYI97wWcPgbNVXrdm7NSNjT5qZYY", Guid.Parse("C51F3107-D823-493F-8103-6206DD41586F"), "天气不错");
            BKLogger.LogBizAsync(typeof(WeixinAPIController), obj);
            return true;
        }

        #endregion
    }

    public class PostUserAcademicParameter
    {
        public UserAcademic UserAcademic { get; set; }
    }
}