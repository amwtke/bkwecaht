using BK.Model.DB;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.CommonLib.DB.Repositorys
{
    public class SystemRepository : IDisposable
    {
        private bool disposed = false;
        private BKDBContext context;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
                disposed = true;
            }
        }
        #region common
        public SystemRepository()
        {
            this.context = new BKDBContext();
        }
        public SystemRepository(BKDBContext context)
        {
            this.context = context;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region School Faculty
        public async Task<Univs> GetUnivIdByNameAsync(string univName)
        {
            List<Univs> result = await (from u in context.Univs
                            where u.UnivsName.Equals(univName)
                            select u).ToListAsync();
            if (result != null && result.Count == 1)
                return result[0];
            return null;
        }

        public Univs GetUnivIdByName(string univName)
        {
            List<Univs> result = (from u in context.Univs
                                        where u.UnivsName.Equals(univName)
                                        select u).ToList();
            if (result != null && result.Count == 1)
                return result[0];
            return null;
        }

        public async Task<T_UnivsDep> GetDepByNameAsync(string depName)
        {
            List<T_UnivsDep> result = await (from u in context.T_UnivsDep
                                        where u.DepName.Equals(depName)
                                        select u).ToListAsync();
            if (result != null && result.Count == 1)
                return result[0];
            return null;
        }

        public T_UnivsDep GetDepByName(Univs univ,string depName)
        {
            List<T_UnivsDep> result = new List<T_UnivsDep>();
            if (univ == null)
            {
                result = (from u in context.T_UnivsDep
                                           where u.DepName.Equals(depName)
                                           select u).ToList();
            }
            else
            {
                result = (from u in context.T_UnivsDep
                                           where u.DepName.Equals(depName)
                                           && univ.UnivsID.Equals(u.UnivsID)
                                           select u).ToList();
            }
            if (result != null && result.Count > 0)
                return result[0];
            return null;
        }

        public T_UnivsDep GetDepByName2(string depName)
        {
            List<T_UnivsDep> result = new List<T_UnivsDep>();

                result = (from u in context.T_UnivsDep
                          where u.DepName.Equals(depName)
                          select u).ToList();
            
            if (result != null && result.Count > 0)
                return result[0];
            return null;
        }

        public async Task<List<Univs>> GetUniversityNameByKeyword(string keyword)
        {
            List<Univs> result = null;
            if (string.IsNullOrEmpty(keyword))
                result = await context.Univs.ToListAsync();
            else
                result = await (from u in context.Univs
                                where u.UnivsName.Contains(keyword)
                                select u).ToListAsync();
            return result;
        }

        public async Task<List<T_UnivsDep>> GetFacultyNameByUnivsID(string name)
        {
            List<T_UnivsDep> result = null;
            if(!string.IsNullOrEmpty(name))
                result = await (from u in context.T_UnivsDep
                                where u.UnivsID == name
                                select u).ToListAsync();
            return result;
        }

        public async Task<List<ResearchField>> GetResearchFieldByFartherID(long fatherID)
        {
            List<ResearchField> result = null;
            result = await (from u in context.ResearchField
                            where u.FatherID == fatherID
                            select u).ToListAsync();
            return result;
        }
        public async Task<string> GetResearchFieldNameByID(long id)
        {
            string result = null;
            var rf = await (from u in context.ResearchField
                            where u.ID == id
                            select u).FirstOrDefaultAsync();
            if(rf != null)
                result = rf.FieldName;
            return result;
        }


        public async Task<IList<City>> GetCityNamesByProvinceId(int name)
        {
            List<City> result = null;
            result = await (from u in context.City
                            where u.provinceId_Province == name.ToString()
                            orderby u.cityId
                            select u).ToListAsync();
            return result;
        }
        public async Task<IList<Country>> GetCountryNames()
        {
            List<Country> result = null;
            result = await context.Countrie.OrderBy(a => a.Id).ToListAsync();
            return result;
        }
        public async Task<IList<Province>> GetProvinceNames()
        {
            List<Province> result = null;
            result = await context.Province.OrderBy(a => a.provinceId).ToListAsync();
            return result;
        }

        public async Task<string> GetShortAddress(string province,string city)
        {
            string result = null;
            if(!string.IsNullOrEmpty(province))
            {
                result = await (from u in context.Province
                                where u.provinceId == province
                                select u.provinceName).FirstOrDefaultAsync();
                if(province != "110000" && province != "120000" && province != "310000" && province != "500000")
                {
                    if(!string.IsNullOrEmpty(city))
                    {
                        result += await (from u in context.City
                                         where u.cityId == city
                                         select u.cityName).FirstOrDefaultAsync();
                    }
                }
            }
            else
            {
                return "中国";
            }            
            return result;
        }

        #endregion


    }
}
