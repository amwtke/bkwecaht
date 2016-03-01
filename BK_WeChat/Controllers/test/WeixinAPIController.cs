using BK.CommonLib.DB.Redis;
using BK.CommonLib.DB.Redis.Objects;
using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.ElasticSearch;
using BK.CommonLib.Log;
using BK.CommonLib.MQ;
using BK.CommonLib.Util;
using BK.CommonLib.Weixin.Message;
using BK.CommonLib.Weixin.Token;
using BK.CommonLib.Weixin.User;
using BK.Model.Configuration.Redis;
using BK.Model.Configuration.User;
using BK.Model.DB;
using BK.Model.DB.Messaging;
using BK.Model.Index;
using BK.Model.Redis;
using BK.Model.Redis.att.CustomAtts.sets;
using BK.Model.Redis.att.CustomAtts.zsets;
using BK.Model.Redis.Objects;
using BK.Model.Redis.Objects.EK;
using BK.Model.Redis.Objects.UserBehavior;
using BK.WeChat.BizHelper;
using Senparc.Weixin.MP.Helpers;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace BK.WeChat.Controllers
{
    [RoutePrefix("weixinapi")]
    public partial class WeixinAPIController: ApiController
    {

        #region test
        //http://localhost:58076/weixinapi/Append/?key=1&field=3
        [Route("Append")]
        public string GetAppendString(string key, string field)
        {
            return key + field;
        }

        [Route("GetHash/{key}/{field}")]
        public string GetHashByKey(string key, string field)
        {
            return RedisManager.GetRedisDB().HashGet(key, field);
        }

        [Route("GetKeys/{key}")]
        public async System.Threading.Tasks.Task<List<string>> GetAllHashsByKey(string key)
        {
            var hs = await RedisManager.GetRedisDB().HashGetAllAsync(key);
            List<string> _list = new List<string>();
            foreach(var h in hs)
            {
                _list.Add(h.Name);
            }
            return _list;
        }
        #endregion

        #region weixin api
        [Route("GetAT")]
        public string GetAccessToken()
        {
            return WXTokenHelper.GetSiteAccessTokenFromRedis();
        }

        [Route("GetUrl")]
        public string GetUrl(string url)
        {
            return WXAuthHelper.GetAuthUrl(url);
        }

        [Route("msg")]
        [HttpGet]
        public async System.Threading.Tasks.Task<string> TemplateMSG(string hello, string openid)
        {
            string url = WXAuthHelper.GetAuthUrl();
            string c = "#009966";
            NotifyMessageObject msg = new NotifyMessageObject(new MessageBase(hello, c), new MessageBase("点击授权登陆", c), new MessageBase("大学问网", c), new MessageBase(DateTime.Now.ToString(), c));
            var result = await TemplateMsgHelper.SendAsync(openid, TemplateMsgHelper.GetTemplateId(TemplateType.Notify), msg, url);
            return result.errmsg;
        }


        [Route("save")]
        [HttpGet]
        public async Task<UserInfoRedis> TestSave()
        {
            UserInfoRedis obj = new UserInfoRedis();
            obj.City = "city";
            obj.Country = "userinfo.country";
            obj.Province = "userinfo.province";
            obj.HeadImageUrl = "userinfo.headimgurl";
            obj.NiceName = "userinfo.nickname";
            obj.Sex = "userinfo.sex.ToString()";
            obj.Openid = "userinfo.openid";
            obj.Unionid = "userinfo.unionid";

            double now = CommonHelper.GetUnixTimeNow();
            obj.AccessToken = "result.access_token";
            obj.ExpireIn = (now + 7200).ToString();
            obj.RefreshToken = "result.refresh_token";
            await RedisManager.SaveObjectAsync(obj);

            UserInfoRedis u = await RedisManager.GetObjectFromRedis<UserInfoRedis>("oYI97wWcPgbNVXrdm7NSNjT5qZYY");
            return u;
        }

        [Route("add")]
        [HttpGet]
        public async Task<TestObjectRedis> TestSave2()
        {
            var r = new Random();
            string newid = Guid.NewGuid().ToString();
            await RedisManager.SaveObjectAsync(new TestObjectRedis() { id = newid, id2 = newid + "_id2", Country = "中国", NiceName = "amwtke", Scoreid = r.Next(), Scoreid2 = r.Next() });
            var s = await RedisManager.GetObjectFromRedis<TestObjectRedis>(newid);
            return s;
        }
        #endregion

        #region redis userinfo
        [Route("registers")]
        [HttpGet]
        public async Task<List<string>> getAllRegister()
        {
            string setName = RedisManager.GetKeyName<UserInfoRedis, UserInfoSetOpenIdSetAttribute>();
            return await RedisManager.GetAllMembers<UserInfoRedis>(setName);
        }

        [Route("getuserinfo")]
        [HttpGet]
        public async Task<UserInfoRedis> getUserInfoRedis(string openid)
        {
            return await RedisManager.GetObjectFromRedis<UserInfoRedis>(openid);
        }
        #endregion

        #region test redis object
        [Route("ranges")]
        public async Task<KeyValuePair<string, double>[]> getRangeByRankWithScore(string o = "d", long f = 0, long t = -1)
        {
            string key = RedisManager.GetKeyName<TestObjectRedis, TestObjectIdZSetAttribute>();
            if(o.Equals("d"))
            {
                return await RedisManager.GetRangeByRankWithScore<TestObjectRedis>(key, Order.Descending, f, t);
            }

            else
            {
                return await RedisManager.GetRangeByRankWithScore<TestObjectRedis>(key, Order.Ascending, f, t);
            }
        }

        [Route("range")]
        public async Task<List<string>> getRangeByRank(string o = "d", long f = 0, long t = -1)
        {
            string key = RedisManager.GetKeyName<TestObjectRedis, TestObjectIdZSetAttribute>();
            if(o.Equals("d"))
            {
                return await RedisManager.GetRangeByRank<TestObjectRedis>(key, Order.Descending, f, t);
            }

            else
            {
                return await RedisManager.GetRangeByRank<TestObjectRedis>(key, Order.Ascending, f, t);
            }
        }

        [Route("rangebyscore")]
        public async Task<KeyValuePair<string, double>[]> getRangeByScore(long o, int top, Order order = Order.Descending, double f = double.NegativeInfinity, double t = double.PositiveInfinity)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();
            return await redis.GetRangeByScoreAsync<TestObjectRedis, TestObjectIdZSetAttribute>("", o, top, order, f, t);
        }

        [Route("cs")]
        [HttpGet]
        public async Task<double> changeScore(string m, double n, string t = "d")
        {
            string key = RedisManager.GetKeyName<TestObjectRedis, TestObjectIdZSetAttribute>();
            if(t.Equals("d"))
                return await RedisManager.DecreaseScore<TestObjectRedis>(key, m, n);
            else
                return await RedisManager.IncreaseScoreAsync<TestObjectRedis>(key, m, n);
        }
        #endregion

        #region tester
        [Route("testers")]
        [HttpGet]
        public async Task<List<string>> getalltester()
        {
            return await WXAuthHelper.GetAllTesters();
        }

        [Route("addtester")]
        [HttpGet]
        public async Task<bool> addtester(string openid)
        {
            return await WXAuthHelper.AddTester(openid);
        }

        [Route("addtester2")]
        [HttpGet]
        public async Task<TesterRedis> addtester2()
        {
            TesterRedis test = new TesterRedis();
            test.Openid = "oYI97wWcPgbNVXrdm7NSNjT5qZYY";
            await RedisManager.SaveObjectAsync(test);

            TesterRedis u = await RedisManager.GetObjectFromRedis<TesterRedis>("oYI97wWcPgbNVXrdm7NSNjT5qZYY");
            return u;
        }

        [Route("istester")]
        [HttpGet]
        public async Task<bool> istester(string openid)
        {
            return await WXAuthHelper.IsTester(openid);
        }

        [Route("rmtester")]
        [HttpGet]
        public async Task<bool> removetester(string openid)
        {
            return await WXAuthHelper.RemoveTester(openid);
        }
        #endregion

        //[Route("adduserlogin")]
        //[HttpGet]
        //public async Task<bool> addUserLoginRedis(string openid)
        //{
        //    UserLoginRedis obj = new UserLoginRedis();
        //    obj.OpenId = openid;
        //    obj.LastLoginTime = CommonLib.Util.CommonHelper.GetUnixTimeNow();
        //    return await RedisManager.SaveObjectAsync(obj);
        //}
        #region login count and login time
        [Route("addlogincount")]
        [HttpGet]
        public async Task<double> addLoginCount(string openid)
        {
            return await UserLoginBehaviorOp.AddLoginCountAsync(openid);
        }

        [Route("rangelogintime")]
        [HttpGet]
        public async Task<KeyValuePair<string, double>[]> getRangeLoginTime(long f = 0, long t = -1)
        {
            return await UserLoginBehaviorOp.GetLoginLastLoginTimeRangesAsync(f, t);
        }

        [Route("rangelogincount")]
        [HttpGet]
        public async Task<KeyValuePair<string, double>[]> rangesLogin(long f = 0, long t = -1, string o = "d")
        {
            return await UserLoginBehaviorOp.GetLoginCountRangesAsync(f, t, o);
        }
        [Route("addlogintime")]
        [HttpGet]
        public async Task<bool> AddorUpdateLoginTimeAsync(string openid)
        {
            return await UserLoginBehaviorOp.AddUpdateLastLoginTimeAsync(openid);
        }

        [Route("getlogincount")]
        [HttpGet]
        public async Task<double> getLoginCount(string openid)
        {
            return await UserLoginBehaviorOp.GetLoginCountByOpenidAsync(openid);
        }

        [Route("getlogintime")]
        [HttpGet]
        public async Task<DateTime> GetLoginTimeAsync(string openid)
        {
            return await UserLoginBehaviorOp.GetLastLoginTimeAsync(openid);
        }

        [Route("islogin")]
        [HttpGet]
        public async Task<bool> IsLoggedIn(string openid)
        {
            return await UserLoginBehaviorOp.IsUserOnlineAsync(openid);
        }
        #endregion

        #region  location
        [Route("addlocation")]
        [HttpGet]
        public async Task<bool> AddLocation(string id, double lat, double lon)
        {
            return await LocationManager.AddOrUpdateLocationAsync(id, lat, lon);
        }

        [Route("glr")]
        [HttpGet]
        public async Task<List<Location>> getLocationRange(double km, string openid)
        {
            return await LocationManager.GetDistanceInKmByIdAsync(openid, km);
        }

        [Route("distance")]
        [HttpGet]
        public double Distance(double lat1, double lon1, double lat2, double lon2)
        {
            return GeoHelper.GetDistance(lat1, lon1, lat2, lon2);
        }


        [Route("addcomplexlocation")]
        [HttpGet]
        public async Task<bool> AddComplexLocation(string id, double lat, double lon)
        {
            return ComplexLocationManager.AddOrUpdateLocation(id, lat, lon);
        }

        [Route("updatecomplexlocation")]
        [HttpGet]
        public async Task<bool> UpdateComplexLocation(string id, int isbusiness, int gender, int rfid)
        {
            return await ComplexLocationManager.UpdateComplexLocationAsync(id, isbusiness, gender, rfid);
        }

        [Route("getlocation")]
        [HttpGet]
        public async Task<List<ComplexLocation>> GetLocation(double km, string openid, int? isbusiness, int? gender, int? rfid)
        {
            return await ComplexLocationManager.GetDistanceInKmByIdAsync(openid, km, isbusiness, gender, rfid);
        }
        #endregion

        #region Messaging 
        [Route("chat")]
        [HttpGet]
        public async Task<bool> chat(string u1, string u2, string msg)
        {
            return await WeChatSendMQHelper.SendMessage(u1, u2, msg);
        }


        [Route("getchat")]
        [HttpGet]
        public async Task<List<string>> getChat(string u1, string u2, int sectionNo)
        {
            return await WeChatReceiveHelper.GetMessage(u1, u2, sectionNo);
        }

        [Route("getunreadscore")]
        [HttpGet]
        public async Task<double> GetUnreadScore(string uuid, string sessionId)
        {
            return await MessageRedisOp.GetUnreadScore(uuid, sessionId);
        }

        [Route("getsessions")]
        [HttpGet]
        public async Task<KeyValuePair<string, double>[]> GetSessions(string uuid, Order orderWay = Order.Descending, long from = 0, long to = -1)
        {
            return await MessageRedisOp.GetSessionsTimeStampByUuid(uuid, orderWay, from, to);
        }

        [Route("getunreds")]
        [HttpGet]
        public async Task<KeyValuePair<string, double>[]> Getunreds(string uuid, Order orderWay = Order.Descending, long from = 0, long to = -1)
        {
            return await MessageRedisOp.GetSessionsUnredByUuid(uuid, orderWay, from, to);
        }

        [Route("getusersbysession")]
        [HttpGet]
        public async Task<List<string>> GetUUidsBySessionId(string sessionid)
        {
            return await MessageRedisOp.GetUUidsBySessionId(sessionid);
        }

        [Route("isred")]
        [HttpGet]
        public async Task<bool> IsGetUnredScore(string uuid)
        {
            return await MessageRedisOp.IsGetUnredScore(uuid);
        }
        #endregion

        #region namecard count

        [Route("addnamecard")]
        [HttpGet]
        public async Task<double> AddNameCardScore(string u, double s)
        {
            return await NameCardAccessCountOP.AddScore(u, s);
        }

        [Route("cleannamecard")]
        [HttpGet]
        public async Task<bool> cleanNameCard(string u)
        {
            return await NameCardAccessCountOP.CleanScore(u);
        }

        [Route("getnamecard")]
        [HttpGet]
        public async Task<double> getNameCard(string u)
        {
            return await NameCardAccessCountOP.GetScore(u);
        }
        [Route("rangenamecard")]
        [HttpGet]
        public async Task<KeyValuePair<string, double>[]> RangeNameCard(Order orderWay = Order.Descending, long from = 0, long to = -1)
        {
            return await NameCardAccessCountOP.GetRange(orderWay, from, to);
        }


        #endregion

        #region 大学 院系 专业
        [Route("rangebyscoreunivs")]
        public async Task<KeyValuePair<string, double>[]> getRangeByScoreUnivs(long o, int top, Order order = Order.Descending, double f = double.NegativeInfinity, double t = double.PositiveInfinity)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();
            return await redis.GetRangeByScoreAsync<NameCardRedis, UnivZsetAttribute>("", o, top, order, f, t);
        }

        [Route("rangeunivs")]
        public async Task<KeyValuePair<string, double>[]> getRangeUnivs(string type = "p", long o = 0, int top = 1000, Order order = Order.Descending, double f = double.NegativeInfinity, double t = double.PositiveInfinity)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();
            if(type == "p")
                return await redis.GetRangeByScoreAsync<NameCardRedis, UnivProfessorZsetAttribute>("", o, top, order, f, t);
            else
                return await redis.GetRangeByScoreAsync<NameCardRedis, UnivStudentZsetAttribute>("", o, top, order, f, t);
        }

        [Route("rangebyscordep")]
        public async Task<KeyValuePair<string, double>[]> getRangeByScoredept(string type = "p", long o = 0, int top = 1000, Order order = Order.Descending, double f = double.NegativeInfinity, double t = double.PositiveInfinity)
        {
            var redis = new RedisManager2<WeChatRedisConfig>();
            if(type == "p")
                return await redis.GetRangeByScoreAsync<NameCardRedis, DeptProfessorZsetAttribute>("", o, top, order, f, t);
            else
                return await redis.GetRangeByScoreAsync<NameCardRedis, DeptStudentZsetAttribute>("", o, top, order, f, t);
        }
        #endregion

        #region EK article

        [Route("AddEkArticle")]
        [HttpGet]
        public async Task<bool> AddEkArticle(long id)
        {
            using (UserRepository r = new UserRepository())
            {
                var ektoday = await r.GetEkTodayByIdAsync(id);
                ektoday.Abstract = "test";
                ektoday.Title = ektoday.Title + "--快速将东方绿卡绝色赌妃";
                ektoday.BodyText = "太子妃";
                var index = EKArticleManager.CopyFromDB(ektoday);
                return await EKArticleManager.AddOrUpdateAsync(index);
            }
        }

        [Route("GetEkArticle")]
        [HttpGet]
        public async Task<EKIndex> GetEkArticle(long id)
        {
            return await EKArticleManager.GetById(id);
        }

        [Route("SetEKZanCount")]
        [HttpGet]
        public async Task<bool> AddEkReadCount(long id,int c)
        {
            using (UserRepository r = new UserRepository())
            {
                return await r.SetZanCountAsync(id, c);
            }
        }

        [Route("SetEKReadCount")]
        [HttpGet]
        public async Task<bool> SetEkReadCount(long id,int c)
        {
            using (UserRepository r = new UserRepository())
            {
                return await r.SetReadCount(id, c);
            }
        }

        [Route("rangeek")]
        [HttpGet]
        public async Task<KeyValuePair<string, double>[]> SetEkReadCount(string z,string id="1")
        {
            var redis = new RedisManager2<WeChatRedisConfig>();

            if (z == "z")
                return await redis.GetRangeByRankAsync<EKTodayRedis, EKZanCountZsetAttribute>("");
            if (z == "r")
                return await redis.GetRangeByRankAsync<EKTodayRedis, EKReadCountZsetAttribute>("");
            if (z == "c")
                return await redis.GetRangeByRankAsync<EKTodayRedis, EKCommentCountZsetAttribute>("");
            if (z == "rr")
                return await redis.GetRangeByRankAsync<EKTodayRedis, EKReadPepleZsetAttribute>(id);
            if (z == "zz")
                return await redis.GetRangeByRankAsync<EKTodayRedis, EKZanPepleZsetAttribute>(id);
            return null;
        }

        [Route("testcopypaper")]
        [HttpGet]
        public async Task<bool> TestCopyFromUserArticle(long id)
        {
            UserArticle dba = null;
            using (UserRepository repo = new UserRepository())
            {
                dba = await repo.GetPaperById(id);
            
            return PaperManager.AddOrUpdate(PaperManager.CopyFromDB(dba));
            }
        }

        #endregion

        #region cache
        [Route("clearcache")]
        [HttpGet]
        public async Task<bool> ClearCache(string o, string u)
        {
            return await UserRepository.ClearCache(o, u);
        }
        #endregion

        #region ek paper search
        [Route("eksearch")]
        [HttpGet]
        public async Task<List<EKIndex>> EKSearch(string q,int i=0,int s=10)
        {
            return await EKArticleManager.Search(q,i,s);
        }
        [Route("papersearch")]
        [HttpGet]
        public async Task<List<PapersIndex>> PaperSearch(string q, int i = 0, int s = 10)
        {
            return await PaperManager.Search3(q, i, s);
        }
        #endregion

        #region professor
        [Route("pindex")]
        [HttpGet]
        public ProfessorIndex GetPIndex(string u)
        {
            return ProfessorManager.GenObject(Guid.Parse(u));
        }

        [Route("psearch")]
        [HttpGet]
        public async Task<List<ProfessorIndex>> GetPSearch(Guid u,string q, int i = 0, int s = 10)
        {
            return await ProfessorManager.Search(u,q, i, s);
        }

        [Route("psearch2")]
        [HttpGet]
        public async Task<List<ProfessorIndex>> GetPSearch2(long id ,string q="", int i = 0, int s = 10)
        {
            return await ProfessorManager.Search_rf(id,q, i, s);
        }

        [Route("psearch3")]
        [HttpGet]
        public async Task<List<ProfessorIndex>> GetPSearch3(Guid u, bool xy = false, string danwei = "", string diwei = "", string a = "", int index = 0, int size = 10)
        {
            int diweiscore = ProfessorManager.GetDiweiScore(diwei);
            return await ProfessorManager.Search_rf3(u, xy, danwei, diweiscore, a , index, size);
        }

        [Route("psearch4")]
        [HttpGet]
        public async Task<List<ProfessorIndex>> GetPSearch4()
        {
            List<string> uuidList = new List<string>();
            uuidList.Add("0A6DB8CE-B9A9-4B63-BE4E-F09B4A1DBF3E");
            uuidList.Add("0DF0F434-9247-4365-BB96-C7FE3F9C9D21");
            uuidList.Add("7D0DCE95-95C8-4E66-AB77-0030BCBE0D87");
            return await ProfessorManager.Search_UUid(uuidList);
        }

        [Route("searchedu")]
        [HttpGet]
        public async Task<List<ProfessorIndex>> GetSearchEdu(string q,Guid u,int i=0,int s=10)
        {
            return await ProfessorManager.SearchEducation(q, u, i, s);
        }
        #endregion
    }
}
