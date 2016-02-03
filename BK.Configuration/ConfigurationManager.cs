using BK.Configuration.DB;
using BK.Model.Configuration;
using BK.Model.Configuration.Att;
using BK.Model.DB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Configuration
{
    public static class BK_ConfigurationManager
    {
        static string APP_DOMAIN;
        static Dictionary<Type, Object> _dic;
        static ConfigDBContext _db;
        static object _syncObject = new object();
        static BK_ConfigurationManager()
        {
            lock (_syncObject)
            {
                init();
            }
        }
        static void init()
        {
            //_innerList.Add(new LogConfig());
            //_innerList.Add(new WeixinConfig());
            //foreach (var m in _innerList)
            //{
            //    m.init();
            //}
            _dic = new Dictionary<Type, object>();
            string s = System.Configuration.ConfigurationManager.AppSettings["LocalDBName"];
            if (!string.IsNullOrEmpty(s))
                _db = new ConfigDBContext(s);
            else
                _db = new ConfigDBContext();

            APP_DOMAIN = ConfigurationManager.AppSettings["ConfigDomain"];
        }
        /// <summary>
        /// 返回单个配置信息，适用于在对象标签中同时声明了“Module”与“Function”属性的配置对象。
        /// 对于在对象标签中无法指定Function的对象请使用：public static IQueryable<BKConfigItem> GetByObject(BKConfigItem item)函数。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetConfig<T>() where T : class, new()
        {
            lock (_syncObject)
            {
                try
                {
                    Type t = typeof(T);
                    if (_dic.ContainsKey(t))
                        return _dic[t] as T;

                    return (T)GetConfigByType(t);
                }
                catch(Exception e)
                {
                    throw e;
                }
            }
            //return default(T);
        }

        /// <summary>
        /// 返回单个配置信息，适用于在对象标签中同时声明了“Module”与“Function”属性的配置对象。
        /// 对于在对象标签中无法指定Function的对象请使用：public static IQueryable<BKConfigItem> GetByObject(BKConfigItem item)函数。
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static object GetConfigByType(Type t)
        {
            if (_dic.ContainsKey(t))
                return _dic[t];

            object[] bkatts = t.GetCustomAttributes(typeof(BKConfigAttribute), false);
            if (bkatts == null || bkatts.Length == 0)
                return null;
            BKConfigAttribute att = bkatts[0] as BKConfigAttribute;
            BKConfigItem item = new BKConfigItem();
            item.Module = att.Module; item.Function = att.Function;
            var query = _db.BKConfigs.Where(c=> c.Domain==APP_DOMAIN).Where(a => a.Module == item.Module).Where(b => b.Function == item.Function);
            if (query != null && query.Count() > 0)
            {
                object ret = t.Assembly.CreateInstance(t.ToString());
                foreach (var i in query)
                {
                    var property = ret.GetType().GetProperty(i.Key);
                    if (property != null)
                        property.SetValue(ret, i.Value);
                }
                _dic[t] = ret;
                return ret;
            }
            return null;
        }

        /// <summary>
        /// 检索modul-function-key组合。
        /// 非线程安全,初始化配置建议在单线程状态完成。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IQueryable<BKConfigItem> GetByObject(BKConfigItem item)
        {
            if (item == null) return null;
            IQueryable<BKConfigItem> query = null;
            if (!string.IsNullOrEmpty(item.Domain))
                query = query.Where(a => a.Domain == item.Domain);
            if (!string.IsNullOrEmpty(item.Module))
                query = query.Where(a => a.Module == item.Module);
            if (!string.IsNullOrEmpty(item.Function))
                query = query.Where(a => a.Function == item.Function);
            if (!string.IsNullOrEmpty(item.Key))
                query = query.Where(a => a.Key==item.Key);
            return query;
        }
    }
}
