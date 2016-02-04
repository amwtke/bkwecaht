using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using BK.CommonLib.DB.Repositorys;
using BK.DataSyncFromEK.BKModel;

namespace BK.DataSyncFromEK.BKRepo
{
    public class UserRepository: IDisposable
    {
        private bool disposed = false;
        private BKDBContext context;
        protected virtual void Dispose(bool disposing)
        {
            if(!disposed)
            {
                if(disposing)
                {
                    context.Dispose();
                }
                disposed = true;
            }
        }
        #region common
        public UserRepository()
        {
            this.context = new BKDBContext();
        }
        public UserRepository(BKDBContext context)
        {
            this.context = context;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region userinfo
        public IEnumerable<UserInfo> GetAllUserInfo()
        {
            return context.UserInfo.AsEnumerable();
        }
        
        public void AddUserInfoTemp(UserInfo userinfo)
        {
            context.UserInfo.Add(userinfo);
        }
        public void UpdateUserInfoTemp(UserInfo userinfo)
        {
            context.Entry(userinfo).State = EntityState.Modified;
        }
        public async Task<int> AddUserRecordsTemp<T>(List<T> input) where T :class,IDBModelWithID
        {
            context.Set<T>().AddRange(input);
            return await context.SaveChangesAsync();
        }

        public void SaveContext()
        {
            context.SaveChanges();
        }

        //TODO 数据库层的cache用cacheLRU来做
        //static ConcurrentDictionary<Guid, UserInfo> _userinfoCache = new ConcurrentDictionary<Guid, UserInfo>();

        public async Task<UserInfo> GetUserInfoByUuid(Guid uuid)
        {
            UserInfo u = null;
            //if(_userinfoCache.TryGetValue(uuid, out u))
            //    return u;
            u = await context.UserInfo.FindAsync(uuid);
            //await context.Entry(u).Reference(r => r.ResearchField).LoadAsync();
            //_userinfoCache[uuid] = u;
            return u;
        }

        public UserInfo GetUserInfoByUuid_TB(Guid uuid)
        {
            UserInfo u = null;
            //if (_userinfoCache.TryGetValue(uuid, out u))
            //    return u;
            u = context.UserInfo.Find(uuid);
            //context.Entry(u).Reference(r => r.ResearchField).Load();
            //_userinfoCache[uuid] = u;
            return u;
        }

        public async Task<UserInfo> GetUserInfoByAccountPassword(string account, string password)
        {
            UserInfo result = await (from ui in context.UserInfo
                                     where ui.AccountEmail == account && ui.Password == password
                                     select ui).FirstOrDefaultAsync();
            return result;
        }


        public async Task<UserInfo> GetUserInfoByAccount(string account)
        {
            UserInfo result = await (from ui in context.UserInfo
                                     where ui.AccountEmail == account
                                     select ui).FirstOrDefaultAsync();
            return result;
        }
        /// <summary>
        /// 根据用户的openid是否存在
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        //public async Task<UserInfo> GetUserInfoByOpenid(string wechat_openid)
        //{
        //    //从缓存中读取
        //    Guid uuid = await GetUserUuidByOpenid(wechat_openid);
        //    UserInfo u = await GetUserInfoByUuid(uuid);
        //    if(u != null)
        //        return u;

        //    //没读到则在数据库中读取。
        //    UserInfo result = null;
        //    List<UserInfo> oalist = await (from ui in context.UserInfo
        //                                   join oa in context.wechat_oa on ui.uuid equals oa.uuid
        //                                   where oa.wechat_openid == wechat_openid
        //                                   select ui).ToListAsync();
        //    if(oalist.Count > 0)
        //        result = oalist[0];
        //    return result;
        //}
        public async Task<bool> UpdateUserinfoPassword(string sNewAccount, string password)
        {
            UserInfo userinfo = await (from ui in context.UserInfo
                                       where ui.AccountEmail == sNewAccount
                                       select ui).FirstOrDefaultAsync();
            if(userinfo == null)
            {
                return false;
            }
            else
            {
                userinfo.Password = password;
                if(await context.SaveChangesAsync() > 0)
                    return true;
                else
                    return false;
            }
        }
        public async Task<bool> SaveUserInfo(UserInfo userinfo)
        {
            //新加入
            if(userinfo.uuid == Guid.Empty || GetUserInfoByUuid(userinfo.uuid) == null)
            {
                userinfo.uuid = Guid.NewGuid();
                context.UserInfo.Add(userinfo);
            }
            else if(context.Entry(userinfo).State == EntityState.Detached)
            {
                //userinfo.ResearchField = null;
                //context.Entry(userinfo).State = EntityState.Modified;
                //await context.Entry(userinfo).Reference("ResearchField").LoadAsync();
                RepositoryHelper.UpdateContextItem(context, userinfo);
            }
            try
            {
                //存入数据库
                await context.SaveChangesAsync();

                //刷新缓存
                //_userinfoCache[userinfo.uuid] = userinfo;

                return true;
            }
            catch(Exception ex)
            {
                BK.CommonLib.Log.LogHelper.LogErrorAsync(typeof(UserRepository), ex);
                return false;
            }
        }
        #endregion

        //#region UserLogin
        ///// <summary>
        ///// 返回用户的openid是否存在
        ///// </summary>
        ///// <param name="openid"></param>
        ///// <returns></returns>
        //public async Task<bool> IsUserOpenidExist(string wechat_openid)
        //{
        //    if(_openidUUidDic.ContainsKey(wechat_openid))
        //        return true;
        //    //todo 存入redis 作为缓存 SaveUserOpenid时更新
        //    bool result = false;
        //    List<wechat_oa> oalist = await (from oa in context.wechat_oa
        //                                    where oa.wechat_openid == wechat_openid
        //                                    select oa).ToListAsync();
        //    if(oalist.Count > 0)
        //        result = true;
        //    return result;
        //}


        //static ConcurrentDictionary<string, Guid> _openidUUidDic = new ConcurrentDictionary<string, Guid>();
        ///// <summary>
        ///// 根据用户的openid是否存在
        ///// </summary>
        ///// <param name="openid"></param>
        ///// <returns></returns>
        //public async Task<Guid> GetUserUuidByOpenid(string wechat_openid)
        //{
        //    Guid ret = Guid.Empty;
        //    //先检查本进程缓存
        //    if(_openidUUidDic.TryGetValue(wechat_openid, out ret))
        //        return ret;

        //    //todo 存入redis 作为缓存 SaveUserOpenid时更新
        //    var v = await OpenIdToUserUUIDOp.GetUuidByOpenIdAsync(wechat_openid);
        //    if(!v.Equals(Guid.Empty))
        //    {
        //        _openidUUidDic[wechat_openid] = v;
        //        return v;
        //    }


        //    Guid result = Guid.Empty;
        //    List<wechat_oa> oalist = await (from oa in context.wechat_oa
        //                                    where oa.wechat_openid == wechat_openid
        //                                    select oa).ToListAsync();
        //    if(oalist.Count > 0)
        //    {
        //        result = oalist[0].uuid;
        //        await OpenIdToUserUUIDOp.SaveAsync(wechat_openid, result);
        //    }

        //    return result;
        //}
        ///// <summary>
        ///// 绑定用户的openID、UnionID
        ///// </summary>
        ///// <param name="openid"></param>
        ///// <returns></returns>
        //public async Task<bool> SaveUserOpenid(Guid uuid, string wechat_openid = "", string wechat_unionid = "")
        //{
        //    bool result = false;
        //    wechat_oa oa = await context.wechat_oa.Where(a => a.uuid == uuid).FirstOrDefaultAsync();
        //    if(oa == null)
        //    {
        //        context.wechat_oa.Add(new wechat_oa() {
        //            uuid = uuid,
        //            wechat_openid = wechat_openid,
        //            wechat_unionid = wechat_unionid,
        //            oadate = DateTime.Now
        //        });
        //    }
        //    else
        //    {
        //        oa.wechat_openid = wechat_openid;
        //        oa.wechat_unionid = wechat_unionid;
        //        oa.oadate = DateTime.Now;
        //    }

        //    _openidUUidDic[wechat_openid] = uuid;
        //    if(await context.SaveChangesAsync() > 0)
        //        result = true;
        //    return result;
        //}
        ///// <summary>
        ///// 存入预注册信息
        ///// </summary>
        ///// <param name="openid"></param>
        ///// <returns></returns>
        //public async Task<bool> SavePreRegister(string account, string name, string password,int validate, string wechat_openid)
        //{
        //    bool result = false;
        //    pre_register preg = await (from pr in context.pre_register
        //                               where pr.html == wechat_openid
        //                               select pr).FirstOrDefaultAsync();
        //    if(preg == null)
        //    {
        //        context.pre_register.Add(new pre_register() {
        //            accountemail = account,
        //            name = name,
        //            password = password,
        //            html = wechat_openid,
        //            validate = validate,
        //            createtime = DateTime.Now
        //        });
        //    }
        //    else
        //    {
        //        preg.accountemail = account;
        //        preg.name = name;
        //        preg.password = password;
        //        preg.html = wechat_openid;
        //        preg.validate = validate;
        //        preg.createtime = DateTime.Now;

        //    }
        //    if(await context.SaveChangesAsync() > 0)
        //        result = true;
        //    return result;
        //}
        ///// <summary>
        ///// 获取预注册信息
        ///// </summary>
        ///// <param name="openid"></param>
        ///// <returns></returns>
        //public async Task<pre_register> GetPreRegisterByOpenid(string wechat_openid)
        //{
        //    pre_register preg = await (from pr in context.pre_register
        //                               where pr.html == wechat_openid
        //                               select pr).FirstOrDefaultAsync();
        //    return preg;
        //}
        //#endregion

        //#region userContact UserBeenTo UserFavorite

        //#region 我的好友
        ///// <summary>
        ///// 获取用户的好友
        ///// </summary>
        ///// <param name="openid"></param>
        ///// <returns></returns>
        ///// 
        //public async Task<string> IsUserContact(Guid uuid1, Guid uuid2)
        //{
        //    if(uuid1 != Guid.Empty && uuid2 != Guid.Empty)
        //    {
        //        var uclist = await (from uc in context.UserContacts
        //                            where uc.ConAccount_uuid == uuid1 && uc.AccountEmail_uuid == uuid2
        //                            select uc).FirstOrDefaultAsync();
        //        if(uclist == null)
        //            return "none";
        //        else if(uclist.Status == true)
        //            return "true";
        //        return "false";
        //    }
        //    else
        //        return "none";
        //}

        //public async Task<Tuple<int, int, List<UserInfo>>> GetUserContact(Guid uuid, int pageIndex, int pageSize)
        //{
        //    List<UserInfo> uclist = null;
        //    int itemCount = 0;
        //    int PageCount = 0;
        //    if(uuid != Guid.Empty)
        //    {
        //        itemCount = await (from uc in context.UserContacts
        //                           where uc.AccountEmail_uuid == uuid && (uc.Status ?? false)
        //                           select uc).CountAsync();
        //        if(itemCount > 0)
        //        {
        //            PageCount = itemCount % pageSize == 0 ? itemCount / pageSize : itemCount / pageSize + 1;
        //            if(pageIndex > 0)
        //            {
        //                uclist = await (from uc in context.UserContacts
        //                                where uc.AccountEmail_uuid == uuid && (uc.Status ?? false)
        //                                orderby uc.AddTime descending
        //                                select uc.ConAccount_userinfo).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        //            }
        //        }
        //    }
        //    return Tuple.Create(itemCount, PageCount, uclist);
        //}

        //public async Task<int> GetUserContactNumber(Guid uuid)
        //{
        //    int result = 0;
        //    if(uuid != Guid.Empty)
        //        result = await (from uc in context.UserContacts
        //                        where uc.AccountEmail_uuid == uuid && (uc.Status ?? false)
        //                        select uc).CountAsync();
        //    return result;
        //}

        //public async Task<bool> AddUserContact(UserContacts ucs1, UserContacts ucs2)
        //{
        //    var uclist = await (from uc in context.UserContacts
        //                        where uc.ConAccount_uuid == ucs1.ConAccount_uuid && uc.AccountEmail_uuid == ucs1.AccountEmail_uuid
        //                        select uc).FirstOrDefaultAsync();
        //    if(uclist != null)
        //        return false;
        //    else
        //    {
        //        context.UserContacts.Add(ucs1);
        //        context.UserContacts.Add(ucs2);
        //        if(await context.SaveChangesAsync() > 0)
        //            return true;
        //        else
        //            return false;
        //    }

        //}
        //public async Task<bool> ActiveUserContact(Guid u1, Guid u2)
        //{
        //    var uc1 = await (from uc in context.UserContacts
        //                     where uc.ConAccount_uuid == u1 && uc.AccountEmail_uuid == u2
        //                     select uc).FirstOrDefaultAsync();
        //    var uc2 = await (from uc in context.UserContacts
        //                     where uc.ConAccount_uuid == u2 && uc.AccountEmail_uuid == u1
        //                     select uc).FirstOrDefaultAsync();
        //    if(uc1 == null || uc2 == null)
        //        return false;
        //    else
        //    {
        //        uc1.Status = true;
        //        uc2.Status = true;
        //        if(await context.SaveChangesAsync() > 0)
        //            return true;
        //        else
        //            return false;
        //    }

        //}

        //public async Task<bool> DeleteUserContact(Guid ucs1, Guid ucs2)
        //{
        //    var uc1 = await (from uc in context.UserContacts
        //                     where uc.ConAccount_uuid == ucs1 && uc.AccountEmail_uuid == ucs2
        //                     select uc).FirstOrDefaultAsync();
        //    var uc2 = await (from uc in context.UserContacts
        //                     where uc.ConAccount_uuid == ucs2 && uc.AccountEmail_uuid == ucs1
        //                     select uc).FirstOrDefaultAsync();
        //    if(uc1 == null)
        //        return false;
        //    else
        //    {
        //        try
        //        {
        //            context.UserContacts.Remove(uc1);
        //            context.UserContacts.Remove(uc2);
        //            await context.SaveChangesAsync();
        //            return true;
        //        }
        //        catch
        //        {
        //            return false;
        //        }
        //    }

        //}
        //#endregion

        //#region beento 我访问过的人
        ///// <summary>
        ///// 获取用户的访问踪迹
        ///// </summary>
        ///// <param name="openid"></param>
        ///// <returns></returns>
        //public async Task<List<UserInfo>> GetUserBeenTo(Guid uuid)
        //{
        //    List<UserInfo> uclist = null;
        //    if(uuid != Guid.Empty)
        //        uclist = await (from uc in context.VisitBetweenUser
        //                        where uc.UserGuest_uuid == uuid
        //                        orderby uc.VisitTime descending
        //                        select uc.UserHost_userinfo).Distinct().ToListAsync();
        //    return uclist;
        //}

        //public async Task<Tuple<int, int, List<UserInfo>>> GetUserBeenTo(Guid uuid, int pageIndex, int pageSize)
        //{
        //    List<UserInfo> uclist = null;
        //    int itemCount = 0;
        //    int PageCount = 0;

        //    if(uuid != Guid.Empty)
        //    {
        //        itemCount = await (from uc in context.VisitBetweenUser
        //                           where uc.UserGuest_uuid == uuid
        //                           select uc.UserHost_userinfo).Distinct().CountAsync();
        //        if(itemCount > 0)
        //        {
        //            PageCount = itemCount % pageSize == 0 ? itemCount / pageSize : itemCount / pageSize + 1;
        //            if(pageIndex > 0)
        //            {
        //                uclist = await (from uc in context.VisitBetweenUser
        //                                where uc.UserGuest_uuid == uuid
        //                                orderby uc.VisitTime descending
        //                                select uc.UserHost_userinfo).Distinct().ToListAsync();
        //                uclist = uclist.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        //            }
        //        }
        //    }
        //    return Tuple.Create(itemCount, PageCount, uclist);
        //}

        //public async Task<int> GetUserBeenToNumber(Guid uuid)
        //{
        //    int result = 0;
        //    if(uuid != Guid.Empty)
        //        result = await (from uc in context.VisitBetweenUser
        //                        where uc.UserGuest_uuid == uuid
        //                        select uc.UserHost_uuid).Distinct().CountAsync();
        //    return result;
        //}

        ///// <summary>
        ///// 获取用户的访问踪迹
        ///// </summary>
        ///// <param name="openid"></param>
        ///// <returns></returns>
        //public async Task<Tuple<int, int, List<UserInfo>>> GetUserVisitor(Guid uuid, int pageIndex, int pageSize)
        //{
        //    List<UserInfo> uclist = null;
        //    int itemCount = 0;
        //    int PageCount = 0;
        //    if(uuid != Guid.Empty)
        //    {
        //        itemCount = await (from uc in context.VisitBetweenUser
        //                           where uc.UserHost_uuid == uuid
        //                           select uc).CountAsync();
        //        if(itemCount > 0)
        //        {
        //            PageCount = itemCount % pageSize == 0 ? itemCount / pageSize : itemCount / pageSize + 1;
        //            if(pageIndex > 0)
        //            {
        //                uclist = await (from uc in context.VisitBetweenUser
        //                                where uc.UserHost_uuid == uuid
        //                                orderby uc.VisitTime descending
        //                                select uc.UserGuest_userinfo).Distinct().ToListAsync();
        //                uclist = uclist.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        //            }
        //        }
        //    }
        //    return Tuple.Create(itemCount, PageCount, uclist);
        //}

        //public async Task<int> GetUserVisitorNumber(Guid uuid)
        //{
        //    int result = 0;
        //    if(uuid != Guid.Empty)
        //        result = await (from uc in context.VisitBetweenUser
        //                        where uc.UserHost_uuid == uuid
        //                        select uc.UserGuest_uuid).Distinct().CountAsync();
        //    return result;
        //}

        ///// <summary>
        ///// 获取用户的访客的访问踪迹
        ///// </summary>
        ///// <param name="openid"></param>
        ///// <returns></returns>
        //public async Task<List<UserInfo>> GetVisitorBeenTo(Guid uuid, int top = 100)
        //{
        //    List<UserInfo> uclist = null;
        //    if(uuid != Guid.Empty)
        //    {
        //        List<Guid> Vlist = await (from uc in context.VisitBetweenUser
        //                                  where uc.UserHost_uuid == uuid
        //                                  select uc.UserGuest_uuid).Distinct().ToListAsync();
        //        if(Vlist.Count > 0)
        //            uclist = await (from uc in context.VisitBetweenUser
        //                            where Vlist.Contains(uc.UserGuest_uuid) && uc.UserHost_uuid != uuid
        //                            orderby uc.VisitTime descending
        //                            select uc.UserHost_userinfo).Distinct().Take(top).ToListAsync();
        //    }
        //    return uclist;
        //}

        //public async Task<Tuple<int, int, List<UserInfo>>> GetVisitorBeenTo(Guid uuid, int pageIndex, int pageSize)
        //{
        //    List<UserInfo> uclist = null;
        //    int itemCount = 0;
        //    int PageCount = 0;
        //    if(uuid != Guid.Empty)
        //    {
        //        List<Guid> Vlist = await (from uc in context.VisitBetweenUser
        //                                  where uc.UserHost_uuid == uuid
        //                                  select uc.UserGuest_uuid).Distinct().ToListAsync();
        //        if(Vlist.Count > 0)
        //        {
        //            uclist = await (from uc in context.VisitBetweenUser
        //                            where Vlist.Contains(uc.UserGuest_uuid) && uc.UserHost_uuid != uuid
        //                            orderby uc.VisitTime descending
        //                            select uc.UserHost_userinfo).Distinct().ToListAsync();
        //            itemCount = uclist.Count;
        //        }
        //        if(itemCount > 0)
        //        {
        //            PageCount = itemCount % pageSize == 0 ? itemCount / pageSize : itemCount / pageSize + 1;
        //            if(pageIndex > 0)
        //            {
        //                uclist = uclist.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        //            }
        //        }
        //    }
        //    return Tuple.Create(itemCount, PageCount, uclist);
        //}

        //public async Task<int> GetVisitorBeenToNumber(Guid uuid)
        //{
        //    int result = 0;
        //    if(uuid != Guid.Empty)
        //    {
        //        List<Guid> Vlist = await (from uc in context.VisitBetweenUser
        //                                  where uc.UserHost_uuid == uuid
        //                                  select uc.UserGuest_uuid).Distinct().ToListAsync();
        //        if(Vlist.Count > 0)
        //            result = await (from uc in context.VisitBetweenUser
        //                            where Vlist.Contains(uc.UserGuest_uuid) && uc.UserHost_uuid != uuid
        //                            select uc.UserHost_userinfo).Distinct().CountAsync();
        //    }
        //    return result;
        //}

        //public async Task<bool> AddVisitBetweenUser(VisitBetweenUser bt)
        //{
        //    context.VisitBetweenUser.Add(bt);
        //    if(await context.SaveChangesAsync() > 0)
        //        return true;
        //    else
        //        return false;
        //}

        //#endregion

        //#region 点赞信息
        ///// <summary>
        ///// 获取用户的点赞信息
        ///// </summary>
        ///// <param name="openid"></param>
        ///// <returns></returns>
        //public async Task<bool> IsUserFavorite(Guid uuid1, Guid uuid2)
        //{
        //    if(uuid1 != Guid.Empty && uuid2 != Guid.Empty)
        //    {
        //        var uclist = await (from uc in context.user_favorite
        //                            where uc.user_account_uuid == uuid1 && uc.user_fav_account_uuid == uuid2
        //                            select uc).FirstOrDefaultAsync();
        //        if(uclist == null)
        //            return false;
        //        else
        //            return true;
        //    }
        //    else
        //        return false;
        //}

        //public async Task<List<UserInfo>> GetuserFavorite(Guid uuid)
        //{
        //    List<UserInfo> uclist = null;

        //    if(uuid != Guid.Empty)
        //        uclist = await (from uc in context.user_favorite
        //                        where uc.user_fav_account_uuid == uuid
        //                        orderby uc.add_time descending
        //                        select uc.user_account_userinfo).ToListAsync();
        //    return uclist;
        //}

        //public async Task<Tuple<int, int, List<UserInfo>>> GetuserFavorite(Guid uuid, int pageIndex, int pageSize)
        //{
        //    List<UserInfo> uclist = null;
        //    int itemCount = 0;
        //    int PageCount = 0;

        //    if(uuid != Guid.Empty)
        //    {
        //        itemCount = await (from uc in context.user_favorite
        //                           where uc.user_fav_account_uuid == uuid
        //                           select uc).CountAsync();
        //        if(itemCount > 0)
        //        {
        //            PageCount = itemCount % pageSize == 0 ? itemCount / pageSize : itemCount / pageSize + 1;
        //            if(pageIndex > 0)
        //            {
        //                uclist = await (from uc in context.user_favorite
        //                                where uc.user_fav_account_uuid == uuid
        //                                orderby uc.add_time descending
        //                                select uc.user_account_userinfo).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        //            }
        //        }
        //    }
        //    return Tuple.Create(itemCount, PageCount, uclist);
        //}

        //public async Task<int> GetuserFavoriteNumber(Guid uuid)
        //{
        //    int result = 0;
        //    if(uuid != Guid.Empty)
        //        result = await (from uc in context.user_favorite
        //                        where uc.user_fav_account_uuid == uuid
        //                        select uc).CountAsync();
        //    return result;
        //}

        //public async Task<bool> AddUserFavorite(user_favorite uf)
        //{
        //    var uclist = await (from uc in context.user_favorite
        //                        where uc.user_fav_account_uuid == uf.user_fav_account_uuid && uc.user_account_uuid == uf.user_account_uuid
        //                        select uc).FirstOrDefaultAsync();
        //    if(uclist != null)
        //        return false;
        //    else
        //    {
        //        context.user_favorite.Add(uf);
        //        if(await context.SaveChangesAsync() > 0)
        //            return true;
        //        else
        //            return false;
        //    }

        //}

        //public async Task<bool> DeleteUserFavorite(user_favorite uf)
        //{
        //    var uclist = await (from uc in context.user_favorite
        //                        where uc.user_fav_account_uuid == uf.user_fav_account_uuid && uc.user_account_uuid == uf.user_account_uuid
        //                        select uc).FirstOrDefaultAsync();
        //    if(uclist == null)
        //        return false;
        //    else
        //    {
        //        try
        //        {
        //            context.user_favorite.Remove(uclist);
        //            await context.SaveChangesAsync();
        //            return true;
        //        }
        //        catch
        //        {
        //            return false;
        //        }
        //    }

        //}
        //#endregion

        //#endregion

        #region 8项资料通用方法
        public async Task<T> GetUserRecordsById<T>(T input) where T : class, IDBModelWithID
        {
            T result;
            result = await (from uc in context.Set<T>()
                            where uc.Id == input.Id
                            select uc).FirstOrDefaultAsync();
            return result;
        }
        public async Task<bool> SaveUserRecordsById<T>(T input) where T : class, IDBModelWithID
        {
            if(input.Id == 0)
            {
                context.Set<T>().Add(input);
            }
            else if(context.Entry(input).State == EntityState.Detached)
            {
                RepositoryHelper.UpdateContextItem(context, input);
            }

            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> DeleteUserRecordsById<T>(T input) where T : class, IDBModelWithID
        {
            T get = await (from up in context.Set<T>()
                           where up.Id == input.Id
                           select up).FirstOrDefaultAsync();
            if(get == null)
                return false;
            else
            {
                try
                {
                    context.Set<T>().Remove(get);
                    await context.SaveChangesAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public async Task<List<T>> GetUserRecords<T>(T input) where T : class, IDBModelWithID
        {
            List<T> uclist = null;
            if(input.AccountEmail_uuid != Guid.Empty)
                uclist = await (from uc in context.Set<T>()
                                where uc.AccountEmail_uuid == input.AccountEmail_uuid
                                orderby uc.Id descending
                                select uc).ToListAsync();
            return uclist;
        }

        public async Task<List<T>> GetUserRecordsByUuid<T>(Guid uuid) where T : class, IDBModelWithID
        {
            List<T> uclist = null;
            if (!uuid.Equals(Guid.Empty))
            {
                uclist = await (from uc in context.Set<T>()
                                where uc.AccountEmail_uuid == uuid
                                orderby uc.Id descending
                                select uc).ToListAsync();
            }
            return uclist;
        }

        public List<T> GetUserRecords_TB<T>(T input) where T : class, IDBModelWithID
        {
            List<T> uclist = null;
            if (input.AccountEmail_uuid != Guid.Empty)
                uclist = (from uc in context.Set<T>()
                                where uc.AccountEmail_uuid == input.AccountEmail_uuid
                                orderby uc.Id descending
                                select uc).ToList();
            return uclist;
        }

        public async Task<Tuple<int, int, List<T>>> GetUserRecords<T>(T input, int pageIndex, int pageSize) where T : class, IDBModelWithID
        {
            List<T> uclist = null;
            int itemCount = 0;
            int PageCount = 0;
            if(input.AccountEmail_uuid != Guid.Empty)
            {
                itemCount = await (from uc in context.Set<T>()
                                   where uc.AccountEmail_uuid == input.AccountEmail_uuid
                                   select uc).CountAsync();
                if(itemCount > 0)
                {
                    PageCount = itemCount % pageSize == 0 ? itemCount / pageSize : itemCount / pageSize + 1;
                    if(pageIndex > 0)
                    {
                        uclist = await (from uc in context.Set<T>()
                                        where uc.AccountEmail_uuid == input.AccountEmail_uuid
                                        orderby uc.Id descending
                                        select uc).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
                    }
                }
            }
            return Tuple.Create(itemCount, PageCount, uclist);
        }
        public async Task<int> GetUserRecordsNumber<T>(T input) where T : class, IDBModelWithID
        {
            int result = 0;

            if(input.AccountEmail_uuid != Guid.Empty)
                result = await (from uc in context.Set<T>()
                                where uc.AccountEmail_uuid == input.AccountEmail_uuid
                                select uc).CountAsync();
            return result;
        }
        #endregion

        //#region EK Today

        //public bool AddEKReader(long id, Guid uuid,double time)
        //{
        //    List<article_reader> list = (from reader in context.article_reader
        //                                 where reader.articleid == id && reader.useraccount_uuid == uuid
        //                                 select reader).ToList();
        //    if(list!=null && list.Count()==1)
        //    {
        //        article_reader r = list[0];
        //        r.readtime = CommonLib.Util.CommonHelper.FromUnixTime(time);
        //    }
        //    else
        //    {
        //        DateTime t = BK.CommonLib.Util.CommonHelper.FromUnixTime(time);
        //        article_reader r = new article_reader();
        //        r.articleid = id; r.readtime = t; r.useraccount_uuid = uuid;
        //        context.article_reader.Add(r);
        //    }
        //    return context.SaveChanges() == 1;
        //}
        //public bool AddEKZaner(long id, Guid uuid,double time)
        //{
        //    List<article_praise> list = (from reader in context.article_praise
        //                                 where reader.articleid == id && reader.useraccount_uuid == uuid
        //                                 select reader).ToList();
        //    if (list != null && list.Count() == 1)
        //    {
        //        article_praise r = list[0];
        //        r.praisetime= CommonLib.Util.CommonHelper.FromUnixTime(time);
        //    }
        //    else
        //    {
        //        DateTime t = BK.CommonLib.Util.CommonHelper.FromUnixTime(time);
        //        article_praise r = new article_praise();
        //        r.articleid = id; r.praisetime = t; r.useraccount_uuid = uuid;
        //        context.article_praise.Add(r);
        //    }
        //    return context.SaveChanges() == 1;
        //}

        //public List<article_reader> GetEKReaders(long id)
        //{
        //    List<article_reader> list = (from r in context.article_reader
        //                                 where r.articleid == id
        //                                 select r).ToList();
        //    return list;
        //}

        //public bool IsEKReader(long id,Guid uuid)
        //{
        //    article_reader reader =  (from r in context.article_reader
        //    where r.articleid == id && r.useraccount_uuid == uuid
        //    select r).FirstOrDefault();
        //    if (reader != null)
        //        return true;
        //    return false;
        //}

        //public List<article_praise> GetEKZaners(long id)
        //{
        //    List<article_praise> list = (from r in context.article_praise
        //                                 where r.articleid == id
        //                                 select r).ToList();
        //    return list;
        //}

        //public bool IsEKZaner(long id,Guid uuid)
        //{
        //    article_praise zaner = (from r in context.article_praise
        //                             where r.articleid == id && r.useraccount_uuid == uuid
        //                             select r).FirstOrDefault();
        //    if (zaner != null)
        //        return true;
        //    return false;
        //}

        //public IEnumerable<EKToday> GetAllEKToday()
        //{
        //    return context.EKToday.AsEnumerable();
        //}

        //public async Task<EKToday> GetEkTodayByIdAsync(long id)
        //{
        //    var value = await context.EKToday.FindAsync(id);
        //    return value;
        //}

        //public EKToday GetEkTodayById(long id)
        //{
        //    var value = context.EKToday.Find(id);
        //    return value;
        //}

        //public async Task<bool> AddReadCount(long id)
        //{
        //    var value = await GetEkTodayByIdAsync(id);
        //    if(value!=null)
        //    {
        //        int readCount = value.ReadPoint != null ? value.ReadPoint.Value : 0;
        //        readCount++;
        //        value.ReadPoint = readCount;
        //        return await context.SaveChangesAsync() == 1;
        //    }
        //    return false;
        //}

        //public async Task<bool> SetReadCount(long id,int count)
        //{
        //    var value = await GetEkTodayByIdAsync(id);
        //    if (value != null)
        //    {
        //        value.ReadPoint = count;
        //        return await context.SaveChangesAsync() == 1;
        //    }
        //    return false;
        //}

        //public bool SetReadCount_TB(long id, int count)
        //{
        //    var value = GetEkTodayById(id);
        //    if (value != null)
        //    {
        //        value.ReadPoint = count;
        //        return context.SaveChanges() == 1;
        //    }
        //    return false;
        //}

        //public async Task<bool> AddZanCount(long id)
        //{
        //    var value = await GetEkTodayByIdAsync(id);
        //    if (value != null)
        //    {
        //        int zanCount = value.HitPoint != null ? value.HitPoint.Value : 0;
        //        zanCount++;
        //        value.HitPoint = zanCount;
        //        return await context.SaveChangesAsync() == 1;
        //    }
        //    return false;
        //}

        //public async Task<bool> SetZanCountAsync(long id, int count)
        //{
        //    var value = await GetEkTodayByIdAsync(id);
        //    if (value != null)
        //    {
        //        value.HitPoint = count;
        //        return await context.SaveChangesAsync() == 1;
        //    }
        //    return false;
        //}
        //public bool SetZanCount(long id, int count)
        //{
        //    var value = GetEkTodayById(id);
        //    if (value != null)
        //    {
        //        value.HitPoint = count;
        //        return context.SaveChanges() == 1;
        //    }
        //    return false;
        //}


        //public async Task<bool> UpdateEKToday(EKToday input)
        //{
        //    try
        //    {
        //        context.Entry(input).State = EntityState.Modified;
        //        await context.SaveChangesAsync();
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}
        //public async Task<bool> InsertEKToday(EKToday input)
        //{
        //    try
        //    {
        //        context.EKToday.Add(input);
        //        await context.SaveChangesAsync();
        //        return true;
        //    }
        //    catch(Exception ex)
        //    {
        //        return false;
        //    }
        //}
        //#endregion
        #region paper
        public IEnumerable<UserArticle> GetAllPapers()
        {
            return context.UserArticle.AsEnumerable();
        }

        public async Task<bool> DeletePaperAsync(long id)
        {
            UserArticle obj = await context.UserArticle.FindAsync(id);
            if(obj!=null)
            {
                obj.Status = ((int)PaperStatus.Deleted).ToString();
                int count = await context.SaveChangesAsync();
                return count == 1;
            }
            return false;
        }

        public async Task<bool> AddPaperAsync(UserArticle article)
        {
            List<UserArticle> list = await GetPaperByObjectAsync(article);
            if (list == null || list.Count()==0)
            {
                context.UserArticle.Add(article);
                return await context.SaveChangesAsync() == 1;
            }
            return false;
        }
        public async Task<UserArticle> GetPaperById(long id)
        {
            return await context.UserArticle.FindAsync(id);
        }

        public async Task<bool> UpdatePaperAsync(UserArticle article)
        {
            if (article.Id == 0)
                return false;
            UserArticle db = await GetPaperById(article.Id);
            if (db == null || db.Id == 0)
                return false;
            db.PostMagazine = article.PostMagazine;
            db.PublishTime = article.PublishTime;
            db.Title = article.Title;
            db.Author = article.Author;
            db.AccountEmail_uuid = article.AccountEmail_uuid;

            context.Entry(db).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<List<UserArticle>> GetPaperByObjectAsync(UserArticle u)
        {
            if (u == null || string.IsNullOrEmpty(u.Author) || string.IsNullOrEmpty(u.Title)
                || string.IsNullOrEmpty(u.PostMagazine) || Guid.Empty.Equals(u.AccountEmail_uuid))
                throw new Exception("userarticle必须的四个项不全！");

            List<UserArticle> list = await (from a in context.UserArticle where a.AccountEmail_uuid.Equals(u.AccountEmail_uuid) && a.Author.Trim().Equals(u.Author.Trim()) && a.PostMagazine.Trim().Equals(u.PostMagazine.Trim()) && u.Title.Trim().Equals(a.Title.Trim()) select a).ToListAsync();
            return list;
        }
        #endregion

        //#region 清空cache

        //public static async Task<bool> ClearCache(string openid,string uuid)
        //{
        //    Guid temp;
        //    bool flag = _openidUUidDic.TryRemove(openid, out temp);
        //    flag = await OpenIdToUserUUIDOp.DeleteItemAsync(openid);
        //    UserInfo u = null;
        //    flag = _userinfoCache.TryRemove(Guid.Parse(uuid), out u);
        //    return flag;
        //}

        //#endregion

        #region UserAcadmic

        public IEnumerable<UserAcademic> GetAllUserAcadmic()
        {
            return context.UserAcademic.AsEnumerable();
        }

        public async Task<UserAcademic> GetUserAcadmicById(long id)
        {
            List<UserAcademic> list = await (from ua in context.UserAcademic
                                             where ua.Id == id select ua).ToListAsync();
            if (list!=null && list.Count == 1)
                return list[0];
            return null;
        }

        public async Task<bool> SaveOrUpdateUserAcadmic(UserAcademic input) 
        {
            if (input.Id == 0)
            {
                context.Set<UserAcademic>().Add(input);
            }
            else 
            {
                UserAcademic tmp = await GetUserAcadmicById(input.Id);
                tmp.Association = input.Association;
                tmp.AssociationPost = input.AssociationPost;
                tmp.Fund = input.Fund;
                tmp.FundPost = input.FundPost;
                tmp.Magazine = input.Magazine;
                tmp.MagazinePost = input.MagazinePost;
            }
            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}
