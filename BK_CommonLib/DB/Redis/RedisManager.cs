using BK.CommonLib.Log;
using BK.Model.Configuration;
using BK.Model.Configuration.Redis;
using BK.Model.DB;
using BK.Model.Redis;
using BK.Model.Redis.Objects;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BK.CommonLib.DB.Redis
{
    [Obsolete("使用RedisManager2代替！")]
    public static class RedisManager
    {
        static object syncobject = new object();
        static ConnectionMultiplexer _con = null;
        static RedisManager()
        {
            if (_con == null)
            {
                lock(syncobject)
                {
                    if (_con == null)
                    {
                        _con = InitDb();
                    }
                }
            }
        }

        public static void Close()
        {
            if (_con != null && _con.IsConnected)
            {
                lock (syncobject)
                {
                    if (_con != null && _con.IsConnected)
                    {
                        //LogHelper.LogInfoAsync(typeof(RedisManager), "Redis连接回收！");
                        _con.Close();
                    }
                }
            }
        }

        public static IDatabase GetRedisDB(int dbNumber, object asyncObject)
        {
            return _con.GetDatabase(dbNumber, asyncObject);
        }


        public static IDatabase GetRedisDB()
        {
            return _con.GetDatabase(0);
        }

        private static ConnectionMultiplexer InitDb()
        {
            WeChatRedisConfig config = BK.Configuration.BK_ConfigurationManager.GetConfig<WeChatRedisConfig>();
            ConfigurationOptions op = new ConfigurationOptions();
            if (config == null)
                throw new Exception("Redis配置读取失败，请检查数据库！");
            //add master
            op.EndPoints.Add(config.MasterHostAndPort);
            string[] slaves = config.SlaveHostsAndPorts.Split(config.StringSeperator[0]);
            foreach (var s in slaves)
            {
                if(!string.IsNullOrEmpty(s))
                    op.EndPoints.Add(s);
            }
            op.Password = config.Password;op.AllowAdmin = true;
            op.ClientName = "WeChat-"+"ip:"+ BK.CommonLib.Util .CommonHelper.GetLocalIp() +"-"+DateTime.Now.ToString();
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(op);
            return redis;
        }


        #region ORMapping
        
        #region save object
        /// <summary>
        /// 注意，Object一定要带上RedisDB特有的标签，否则会爆出异常！而且[RedisKeyAttribute]标签的属性必须要带上值，不然会save失败。
        /// </summary>
        /// <param name="o">注意使用标签没</param>
        /// <returns></returns>
        public static async Task<bool> SaveObjectAsync(Object o)
        {
            int dbNumber; string HashName = null;

            //获取number属性
            RedisDBNumberAttribute numberAtt = o.GetType().GetCustomAttribute(typeof(RedisDBNumberAttribute), false) as RedisDBNumberAttribute;
            if (numberAtt == null)
            {
                LogHelper.LogErrorAsync(typeof(RedisManager), new Exception("Redis的存储对象没有指定dbNumber属性！"));
                throw new Exception("Redis的存储对象没有指定dbNumber属性！");
            }
            dbNumber = Convert.ToInt32(numberAtt.DBNumber);

            //获取hushname
            RedisHashAttribute hashatt = o.GetType().GetCustomAttribute(typeof(RedisHashAttribute), false) as RedisHashAttribute;
            //如果有则赋值，没有则传入空值
            if(hashatt!=null)
                HashName = hashatt.Name;

            //遍历每个property
            bool ret = await saveByAttAsync(dbNumber, HashName, o);
            return ret;
        }
        /// <summary>
        /// 可以没有hashname，但是不能没有ReidsKeyAttribute的属性！
        /// </summary>
        /// <param name="dbno"></param>
        /// <param name="hashName"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        static async Task<bool> saveByAttAsync(int dbno, string hashName, object o)
        {
            Type t = o.GetType(); string KeyValue = null;
            bool isNeedHash = !string.IsNullOrEmpty(hashName);

            //不能没有ReidsKeyAttribute
            KeyValue = getKeyAttributeValue(o);
            if (string.IsNullOrEmpty(KeyValue))
                return false;

            var db = GetRedisDB(dbno, null);
            List<HashEntry> _pairs = new List<HashEntry>();

            foreach (var p in t.GetProperties())
            {
                //获取属性值
                var vo = p.GetValue(o);
                if (vo == null)
                    continue;
                string value = vo.ToString();

                foreach (var att in p.GetCustomAttributes())
                {
                    //将属性值添加到set表
                    if (att != null && att is RedisSetAttribute)
                    {
                        var setAtt = att as RedisSetAttribute;
                        string setKey = setAtt.Name;

                        //如果为Every_key,则每个Key值创建一个set.格式为type.id.set。
                        if (setKey.Equals(RedisSpecialStrings.EVERY_KEY.ToString()))
                            await db.SetAddAsync(ObjectHelper.MakeKeyIfEveryKey_Set(t,KeyValue), value);
                        else
                            await db.SetAddAsync(setKey, value);
                    }

                    //将属性值与分数添加到zset表
                    if (att != null && att is RedisZSetAttribute)
                    {
                        var zsetAtt = att as RedisZSetAttribute;
                        string zsetName = zsetAtt.Name;
                        if (!zsetName.Equals(RedisSpecialStrings.EVERY_KEY.ToString()))
                        {
                            string scoreField = zsetAtt.ScoreFieldName;

                            //如果不需要存储值，scoreField字段留空。
                            if (string.IsNullOrEmpty(scoreField)) continue;

                            //没有找到property
                            PropertyInfo pp = t.GetProperty(scoreField);
                            if (pp == null) continue;

                            //property value是空或者不是double
                            object scoreValue = pp.GetValue(o);
                            if (scoreValue == null || !(scoreValue is double)) continue;

                            double score = Convert.ToDouble(scoreValue);
                            await db.SortedSetAddAsync(zsetName, value, score);
                        }
                        else
                        {
                            //如果没有指定zsetname则为每个key一个zset，为对象外部自己写操作方法。
                        }
                    }

                    //将属性值添加到list中
                    if (att != null && att is RedisListAttribute)
                    {
                        var listAtt = att as RedisListAttribute;
                        string listkey = listAtt.Name;
                        //如果listkey名字指定为EveryKey
                        if (listkey.Equals(RedisSpecialStrings.EVERY_KEY.ToString()))
                        {
                            //生成Key
                            listkey = ObjectHelper.MakeKeyIfEveryKey_List(t, KeyValue);
                        }

                        if (listAtt.Push.Equals(ListPush.Left))
                            await db.ListLeftPushAsync(listkey, value);
                        else
                            await db.ListRightPushAsync(listkey, value);

                    }

                    if (att != null && att is RedisStringAttribute)
                    {
                        var stringAtt = att as RedisStringAttribute;
                        string stringKey = stringAtt.Name;
                        if (stringKey.Equals(RedisSpecialStrings.EVERY_KEY.ToString()))
                            await db.StringSetAsync(ObjectHelper.MakeKeyIfEveryKey_String(t, KeyValue), value);
                        else
                            await db.StringSetAsync(stringKey, value);
                    }

                    if (isNeedHash && att != null && att is RedisHashEntryAttribute && !string.IsNullOrEmpty(value))
                    {
                        var entryAtt = att as RedisHashEntryAttribute;
                        string entryName = entryAtt.Name;
                        HashEntry en = new KeyValuePair<RedisValue, RedisValue>(ObjectHelper.MakeField(KeyValue, entryName), value);
                        _pairs.Add(en);
                    }
                }
            }
            try
            {
                if (isNeedHash && _pairs.Count > 0)
                    await db.HashSetAsync(hashName, _pairs.ToArray());
            }
            catch (Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(RedisManager), ex);
                return false;
            }

            return true;
        }

        #endregion

        #region get object
        /// <summary>
        /// 获取reids对象的Hash数据结构中的对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task<T> GetObjectFromRedis<T>(string key) where T : new()
        {
            try
            {
                string hashName = GetHashName<T>();
                string keyValue = key;
                if (string.IsNullOrEmpty(keyValue) || string.IsNullOrEmpty(hashName))
                    return default(T);
                T obj = new T();

                var db = GetRedisDB<T>();//RedisManager.GetRedisDB(nubmerDB, null);
                List<RedisValue> _fields = new List<RedisValue>();
                Dictionary<string, string> _dic = new Dictionary<string, string>();

                foreach (PropertyInfo p in obj.GetType().GetProperties())
                {
                    var att = p.GetCustomAttribute(typeof(RedisHashEntryAttribute), false);
                    if (att == null)
                        continue;
                    var entryAtt = att as RedisHashEntryAttribute;
                    string field = ObjectHelper.MakeField(keyValue, entryAtt.Name);

                    _dic[field] = entryAtt.Name;
                    _fields.Add(field);
                }

                //一次性获取
                if (_fields.Count > 0)
                {
                    RedisValue[] values = await db.HashGetAsync(hashName, _fields.ToArray());

                    if (values != null && values.Length > 0)
                    {
                        for (int i = 0; i < values.Length; i++)
                        {
                            var p = obj.GetType().GetProperty(_dic[_fields[i]]);
                            if (values[i].IsNull)
                                continue;

                            if (p.PropertyType.Equals(typeof(string)))
                                p.SetValue(obj, values[i].ToString());
                            else
                            if (p.PropertyType.Equals(typeof(int)))
                                p.SetValue(obj, Convert.ToInt32(values[i].ToString()));
                            else
                            if (p.PropertyType.Equals(typeof(double)))
                                p.SetValue(obj, Convert.ToDouble(values[i].ToString()));
                            else
                            if (p.PropertyType.Equals(typeof(decimal)))
                                p.SetValue(obj, Convert.ToDecimal(values[i].ToString()));
                            else
                            if (p.PropertyType.Equals(typeof(DateTime)))
                                p.SetValue(obj, Convert.ToDateTime(values[i].ToString()));
                        }
                    }
                }
                //最后别忘了给Key字段赋值
                setKeyPropertyValue<T>(obj, keyValue);
                return obj;
            }
            catch (Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(RedisManager), ex);
                throw ex;
            }
        }

        public static T GetObjectFromRedis_TongBu<T>(string key) where T : new()
        {
            try
            {
                string hashName = GetHashName<T>();
                string keyValue = key;
                if (string.IsNullOrEmpty(keyValue) || string.IsNullOrEmpty(hashName))
                    return default(T);
                T obj = new T();

                var db = GetRedisDB<T>();//RedisManager.GetRedisDB(nubmerDB, null);
                List<RedisValue> _fields = new List<RedisValue>();
                Dictionary<string, string> _dic = new Dictionary<string, string>();

                foreach (PropertyInfo p in obj.GetType().GetProperties())
                {
                    var att = p.GetCustomAttribute(typeof(RedisHashEntryAttribute), false);
                    if (att == null)
                        continue;
                    var entryAtt = att as RedisHashEntryAttribute;
                    string field = ObjectHelper.MakeField(keyValue, entryAtt.Name);

                    _dic[field] = entryAtt.Name;
                    _fields.Add(field);
                }

                //一次性获取
                if (_fields.Count > 0)
                {
                    RedisValue[] values = db.HashGet(hashName, _fields.ToArray());

                    if (values != null && values.Length > 0)
                    {
                        for (int i = 0; i < values.Length; i++)
                        {
                            var p = obj.GetType().GetProperty(_dic[_fields[i]]);
                            if (values[i].IsNull)
                                continue;

                            if (p.PropertyType.Equals(typeof(string)))
                                p.SetValue(obj, values[i].ToString());
                            else
                            if (p.PropertyType.Equals(typeof(int)))
                                p.SetValue(obj, Convert.ToInt32(values[i].ToString()));
                            else
                            if (p.PropertyType.Equals(typeof(double)))
                                p.SetValue(obj, Convert.ToDouble(values[i].ToString()));
                            else
                            if (p.PropertyType.Equals(typeof(decimal)))
                                p.SetValue(obj, Convert.ToDecimal(values[i].ToString()));
                            else
                            if (p.PropertyType.Equals(typeof(DateTime)))
                                p.SetValue(obj, Convert.ToDateTime(values[i].ToString()));
                        }
                    }
                }
                //最后别忘了给Key字段赋值
                setKeyPropertyValue<T>(obj, keyValue);
                return obj;
            }
            catch (Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(RedisManager), ex);
                throw ex;
            }
        }
        #endregion


        #region op redis attribute

        #region get set keyattribute
        static T setKeyPropertyValue<T>(T o, string value) where T : new()
        {
            Type t = typeof(T);
            t.GetProperties().ToList().ForEach(
                delegate (PropertyInfo p)
                {
                    Attribute a = p.GetCustomAttribute(typeof(RedisKeyAttribute), false);
                    if (a != null)
                    {
                        if (!string.IsNullOrEmpty(value))
                            p.SetValue(o, value);
                    }
                });
            return o;
        }
        static string getKeyAttributeValue(object o)
        {
            foreach (var p in o.GetType().GetProperties())
            {
                var att = p.GetCustomAttribute(typeof(RedisKeyAttribute), false);
                if (att != null)
                {
                    object value = p.GetValue(o);
                    if(value==null)
                    {
                        LogHelper.LogErrorAsync(typeof(RedisManager), new Exception("type=" + o.GetType() + "在saveByAttAsync时，没有指定Key！"));
                        throw new Exception("type=" + o.GetType() + "在saveByAttAsync时，没有指定Key！");
                    }
                        
                    return value.ToString();
                }
            }
            return null;
        }
        #endregion

        public static IDatabase GetRedisDB<T>()
        {
            try
            {
                Type t = typeof(T);
                RedisDBNumberAttribute number = t.GetCustomAttribute(typeof(RedisDBNumberAttribute), false) as RedisDBNumberAttribute;
                int numberdb = Convert.ToInt32(number.DBNumber);

                var db = RedisManager.GetRedisDB(numberdb, null);
                return db;
            }
            catch (Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(RedisManager), ex);
                return null;
            }
        }

        /// <summary>
        /// 获取一个对象上Redis中Hash结构的key值。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>没有则返回空</returns>
        public static string GetHashName<T>()
        {
            try
            {
                Attribute hashatt = typeof(T).GetCustomAttribute(typeof(RedisHashAttribute), false);
                if (hashatt == null)
                    return null;
                var hashAt = hashatt as RedisHashAttribute;
                return hashAt.Name;
            }
            catch (Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(RedisManager), ex);
                throw ex;
            }
        }

        [Obsolete("使用GetKeyName<OType,AttType>代替!")]
        /// <summary>
        /// 获取对象某个属性上的set名称。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string GetSetNameByPropertyName<T>(string propertyName)
        {
            return GetNameByProperty<RedisSetAttribute>(typeof(T), propertyName);
        }

        [Obsolete("使用GetKeyName<OType,AttType>代替!")]
        /// <summary>
        /// 获取对象某个属性上的zset名称。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string GetZSetNameByPropertyName<T>(string propertyName)
        {
            return GetNameByProperty<RedisZSetAttribute>(typeof(T), propertyName);
        }

        [Obsolete("使用GetKeyName<OType,AttType>代替!")]
        /// <summary>
        /// 根据property名称获取T属性的数据库名称。
        /// </summary>
        /// <typeparam name="T">hash,set,zset,list</typeparam>
        /// <param name="objectType"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static string GetNameByProperty<T>(Type objectType,string propertyName) where T : RedisBaseAttribute
        {
            try
            {
                Type t = typeof(T);
                PropertyInfo p = objectType.GetProperty(propertyName);
                if (p == null)
                    return null;
                T att = p.GetCustomAttribute(typeof(T), false) as T;
                if (att != null)
                    return att.Name;
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(RedisManager), ex);
                return null;
            }
        }

        /// <summary>
        /// 根据对象以及对象上的标签来获取对应Redis对象中Property上的特定数据结构的key名称。
        /// </summary>
        /// <typeparam name="OType">Redis对象的type</typeparam>
        /// <typeparam name="AttType">Redis对象中某个数据结构的Att。在BK.Model.Redis.Att.CustomAtts*命名空间下.</typeparam>
        /// <returns></returns>
        public static string GetKeyName<OType, AttType>() where AttType : RedisBaseAttribute
        {
            Type oT = typeof(OType);
            Type aT = typeof(AttType);
            string ret = null;
            oT.GetProperties().ToList().ForEach(delegate (PropertyInfo p)
            {
                var a = p.GetCustomAttribute(aT);
                if (a != null)
                {
                    var at = a as RedisBaseAttribute;
                    ret= at.Name;
                }
            });
            return ret;
        }
        #endregion

        #region operation db
        public static async Task<bool> IsContainedInSetByPropertyName<T>(string KeyName,string value)
        {
            var db = GetRedisDB<T>();
            return await db.SetContainsAsync(KeyName, value);
        }

        /// <summary>
        /// 对T类型propertyName属性上的Zset数据库中Member元素的Score增加increaseby。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyName"></param>
        /// <param name="member"></param>
        /// <param name="increaseBy"></param>
        /// <returns></returns>
        public static async Task<double> IncreaseScoreAsync<T>(string keyName, string member,double increaseBy)
        {
            try
            {
                var db = GetRedisDB<T>();
                return await db.SortedSetIncrementAsync(keyName, member, increaseBy);
            }
            catch (Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(RedisManager), ex);
                return Double.NaN;
            }
        }

        public static double IncreaseScore<T>(string keyName, string member, double increaseBy)
        {
            try
            {
                var db = GetRedisDB<T>();
                return db.SortedSetIncrement(keyName, member, increaseBy);
            }
            catch (Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(RedisManager), ex);
                return Double.NaN;
            }
        }

        /// <summary>
        /// 减小分数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Name"></param>
        /// <param name="member"></param>
        /// <param name="decreaseBy"></param>
        /// <returns></returns>
        public static async Task<double> DecreaseScore<T>(string KeyName, string member, double decreaseBy)
        {
            try
            {
                var db = GetRedisDB<T>();
                return await db.SortedSetDecrementAsync(KeyName, member, decreaseBy);
            }
            catch (Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(RedisManager), ex);
                return Double.NaN;
            }
        }

        /// <summary>
        /// 按照Score的打分区间来取数据
        /// </summary>
        /// <typeparam name="T">对象的tpye</typeparam>
        /// <param name="keyName">zset定义在那个字段上</param>
        /// <param name="offSet">从第几个元素开始</param>
        /// <param name="top">共取多少个元素</param>
        /// <param name="orderWay">升序或者逆序</param>
        /// <param name="Scorefrom">最小的score</param>
        /// <param name="Scoreto">最大的score</param>
        /// <returns></returns>
        public static async Task<KeyValuePair<string,double>[]> GetRangeByScoreWithScore<T>(string keyName, long offSet, int top, Order orderWay = Order.Descending, double Scorefrom = double.NegativeInfinity, double Scoreto = double.PositiveInfinity)
        {
            try
            {
                var db = GetRedisDB<T>();
                return ConvertSortedSetEntryToKeyValuePair(await db.SortedSetRangeByScoreWithScoresAsync(keyName, order: orderWay, take: top, start: Scorefrom, stop: Scoreto, skip: offSet));
            }
            catch (Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(RedisManager), ex);
                return null;
            }
        }

        /// <summary>
        /// 按照Score的打分区间来取数据.不带score的方法。
        /// </summary>
        /// <typeparam name="T">对象的tpye</typeparam>
        /// <param name="keyName">zset定义在那个字段上</param>
        /// <param name="offSet">从第几个元素开始</param>
        /// <param name="top">共取多少个元素</param>
        /// <param name="orderWay">升序或者逆序</param>
        /// <param name="Scorefrom">最小的score</param>
        /// <param name="Scoreto">最大的score</param>
        /// <returns></returns>
        public static async Task<List<string>> GetRangeByScore<T>(string keyName, long offSet, int top, Order orderWay = Order.Descending, double Scorefrom = double.NegativeInfinity, double Scoreto = double.PositiveInfinity)
        {
            try
            {
                var db = GetRedisDB<T>();
                return ConvertRedisValueToString(await db.SortedSetRangeByScoreAsync(keyName, order: orderWay, take: top, start: Scorefrom, stop: Scoreto, skip: offSet));
            }
            catch (Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(RedisManager), ex);
                return null;
            }
        }

        /// <summary>
        /// 按照打分的排名来取。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyName"></param>
        /// <param name="orderWay"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static async Task<List<string>> GetRangeByRank<T>(string keyName, Order orderWay = Order.Descending, long from = 0, long to = -1)
        {
            try
            {
                var db = GetRedisDB<T>();
                return ConvertRedisValueToString(await db.SortedSetRangeByRankAsync(keyName, order: orderWay, start: from, stop: to));
            }
            catch (Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(RedisManager), ex);
                return null;
            }
        }

        /// <summary>
        /// 按照打分的排名来取。带Score值。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyName"></param>
        /// <param name="orderWay"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static async Task<KeyValuePair<string, double>[]> GetRangeByRankWithScore<T>(string keyName, Order orderWay = Order.Descending, long from = 0, long to = -1)
        {
            try
            {
                var db = GetRedisDB<T>();
                return ConvertSortedSetEntryToKeyValuePair(await db.SortedSetRangeByRankWithScoresAsync(keyName, order: orderWay, start: from, stop: to));
            }
            catch (Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(RedisManager), ex);
                return null;
            }
        }

        public static async Task<List<string>> GetAllMembers<T>(string keyName)
        {
            try
            {
                var db = RedisManager.GetRedisDB<T>();
                return ConvertRedisValueToString(await db.SetMembersAsync(keyName));
            }
            catch (Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(RedisManager), ex);
                return null;
            }
        }
        #region converter
        public static KeyValuePair<string, double>[] ConvertSortedSetEntryToKeyValuePair(SortedSetEntry[] source)
        {
            SortedSetEntry[] es = source;
            if (es != null && es.Length > 0)
            {
                List<KeyValuePair<string, double>> pair = new List<KeyValuePair<string, double>>();
                foreach (var e in es)
                {
                    pair.Add(new KeyValuePair<string, double>(e.Element, e.Score));
                }
                return pair.ToArray();
            }
            return null;
        }

        public static List<string> ConvertRedisValueToString(RedisValue[] source)
        {
            RedisValue[] es = source;
            if (es != null && es.Length > 0)
            {
                List<string> list = new List<string>();
                foreach (var e in es)
                {
                    if (e.IsNullOrEmpty)
                        continue;
                    list.Add(e.ToString());
                }
                return list;
            }
            return null;
        }
        #endregion


        #endregion

        #endregion
    }
}
