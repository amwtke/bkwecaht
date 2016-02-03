using BK.Model.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BK.CommonLib.DB.Repositorys
{
    public static class RepositoryHelper
    {
        public static object UpdateContextItem(DbContext context,Object obj)
        {
            //将foreign key制空
            obj.GetType().GetProperties().ToList().ForEach(delegate (PropertyInfo pi)
            {
                var att = pi.GetCustomAttribute(typeof(ForeignKeyAttribute));
                if (att != null)
                {
                    pi.SetValue(obj, null);
                }

            });

            //修改操作为修改
            context.Entry(obj).State = EntityState.Modified;

            //重新加载foreign key的对象。
            obj.GetType().GetProperties().ToList().ForEach(delegate (PropertyInfo pi)
            {
                var att = pi.GetCustomAttribute(typeof(ForeignKeyAttribute));
                if (att != null)
                {
                    context.Entry(obj).Reference(pi.Name).Load();
                }
            });
            return obj;
        }

        public static List<string> ConvertUserAcademicToString(List<UserAcademic> list )
        {
            List<string> ret = new List<string>();
            foreach(var a in list)
            {
                //院士
                if (!string.IsNullOrEmpty(a.Academician))
                    ret.Add(a.Academician.Trim());
                //博士生导师
                if (!string.IsNullOrEmpty(a.Tutor) && !ret.Contains(a.Tutor.Trim()))
                    ret.Add(a.Tutor.Trim());
                //协会
                if(!string.IsNullOrEmpty(a.Association) && !string.IsNullOrEmpty(a.AssociationPost))
                {
                    ret.Add(a.Association.Trim() + "-" + a.AssociationPost.Trim());
                }

                //杂志
                if (!string.IsNullOrEmpty(a.Magazine) && !string.IsNullOrEmpty(a.MagazinePost))
                {
                    ret.Add(a.Magazine.Trim() + "-" + a.MagazinePost.Trim());
                }

                //基金
                if (!string.IsNullOrEmpty(a.Fund) && !string.IsNullOrEmpty(a.FundPost))
                {
                    ret.Add(a.Fund.Trim() + "-" + a.FundPost.Trim());
                }
            }

            return ret;
        }

        //public static List<Tuple> ConvertIDBModelWithIDToTupleList<T>(List<T> modelList,Func<T,List<string>> func) where T : IDBModelWithID
        //{
        //    List<Tuple> ret = new List<Tuple>();
        //    foreach (var a in modelList)
        //    {
        //        ret.AddRange(func(a));
        //    }
        //    return ret;
        //}
    }
}
