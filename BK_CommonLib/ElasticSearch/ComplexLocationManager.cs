using BK.CommonLib.Log;
using BK.CommonLib.Util;
using BK.Configuration;
using BK.Model.Configuration.ElasticSearch;
using BK.Model.Index;
using Nest;
using Senparc.Weixin.MP.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.CommonLib.ElasticSearch
{
    public static class ComplexLocationManager
    {
        static void LogError(Exception ex)
        {
            LogHelper.LogErrorAsync(typeof(ComplexLocationManager), ex);
        }
        static ElasticClient _client = null;
        static ComplexLocationConfig _config = null;
        static ComplexLocationManager()
        {
            _client = ESHeper.GetClient<ComplexLocationConfig>();
            if(_client == null)
            {
                var err = new Exception("_client没有正确初始化！");
                LogError(err);
                throw err;
            }

            _config = BK_ConfigurationManager.GetConfig<ComplexLocationConfig>();
            if(_config == null)
            {
                var err = new Exception("配置没有正确初始化！");
                LogError(err);
                throw err;
            }

            init();
        }

        static void init()
        {
            IGetMappingResponse mapping = _client.GetMapping<ComplexLocation>();
            if(mapping != null && (mapping.Mappings == null || mapping.Mappings.Count != 0)) return;
            _client.CreateIndex(_config.IndexName, s => s
                .AddMapping<ComplexLocation>(f => f
                .MapFromAttributes()
                .Properties(p => p
                    .GeoPoint(g => g.Name(n => n.Coordinate).IndexLatLon())
                 )
               )
            );
        }

        public static async Task<bool> AddOrUpdateLocationAsync(string id, double lat, double lon)
        {
            var result = await _client.SearchAsync<ComplexLocation>(s => s.Query(q => q.QueryString(ss => ss.Query("Id:" + id))));
            ComplexLocation l = null;
            if(result.Total >= 1)
            {
                string _id = result.Hits.First().Id;
                var r = await _client.UpdateAsync<ComplexLocation>((u) =>
                {
                    u.Id(_id);
                    l = new ComplexLocation() {
                        Id = id,
                        Coordinate = new ComplexLocationCoordinate() { Lat = lat, Lon = lon },
                        TimeStamp = DateTime.Now.ToString(),
                        IsBusiness = result.Documents.First().IsBusiness,
                        Gender = result.Documents.First().Gender,
                        ResearchFieldId = result.Documents.First().ResearchFieldId
                    };
                    u.Doc(l);
                    u.Index(_config.IndexName);
                    return u;
                });
                return r.IsValid;
            }
            else
            {
                l = new ComplexLocation() {
                    Id = id,
                    Coordinate = new ComplexLocationCoordinate() {
                        Lon = lon,
                        Lat = lat,
                    },
                    TimeStamp = DateTime.Now.ToString(),
                    IsBusiness = 1, Gender = 0, ResearchFieldId = 0
                };
                List<ComplexLocation> list = new List<ComplexLocation>() { l };
                var resoponse = await _client.IndexAsync<ComplexLocation>(l, (i) => { i.Index(_config.IndexName); return i; });
                return resoponse.Created;
            }
        }
        public static async Task<bool> UpdateComplexLocationAsync(string id, int isBusiness, int gender, long researchfieldId)
        {
            var result = await _client.SearchAsync<ComplexLocation>(s => s.Query(q => q.QueryString(ss => ss.Query("Id:" + id))));
            ComplexLocation l = null;
            if(result.Total >= 1)
            {
                if(isBusiness != result.Documents.First().IsBusiness
                    || gender != result.Documents.First().Gender
                    || researchfieldId != result.Documents.First().ResearchFieldId)
                {
                    string _id = result.Hits.First().Id;
                    var r = await _client.UpdateAsync<ComplexLocation>((u) =>
                    {
                        u.Id(_id);
                        l = new ComplexLocation() {
                            Id = id,
                            Coordinate = new ComplexLocationCoordinate() {
                                Lat = result.Documents.First().Coordinate.Lat,
                                Lon = result.Documents.First().Coordinate.Lon
                            },
                            TimeStamp = DateTime.Now.ToString(),
                            IsBusiness = isBusiness, Gender = gender, ResearchFieldId = researchfieldId
                        };
                        u.Doc(l);
                        u.Index(_config.IndexName);

                        BKLogger.LogInfoAsync(typeof(ComplexLocationManager), "更新位置复杂信息："
                            + id
                            + isBusiness
                            + gender
                            + researchfieldId);
                        return u;
                    });
                    return r.IsValid;
                }
                else
                    return true;
            }
            else
                return false;
        }

        public static bool AddOrUpdateLocation(string id, double lat, double lon)
        {
            var result = _client.Search<ComplexLocation>(s => s.Query(q => q.QueryString(ss => ss.Query("Id:" + id))));
            ComplexLocation l = null;
            if (result.Total >= 1)
            {
                string _id = result.Documents.First().Id;
                var r = _client.Update<ComplexLocation>((u) =>
                {
                    u.Id(_id);
                    result.Documents.First().Coordinate = new ComplexLocationCoordinate() { Lat = lat, Lon = lon };
                    result.Documents.First().TimeStamp = DateTime.Now.ToString();
                    //l = new ComplexLocation() { Id = id, Coordinate = new ComplexLocationCoordinate() { Lat = lat, Lon = lon }, TimeStamp = DateTime.Now.ToString() };
                    //u.Doc(l);
                    u.Doc(result.Documents.First());
                    u.Index(_config.IndexName);
                    return u;
                });
                return r.IsValid;
            }
            else
            {
                l = new ComplexLocation() {
                    Id = id,
                    Coordinate = new ComplexLocationCoordinate() {
                        Lon = lon,
                        Lat = lat,
                    },
                    TimeStamp = DateTime.Now.ToString()
                };
                List<ComplexLocation> list = new List<ComplexLocation>() { l };
                var resoponse = _client.Index(l, (i) => { i.Index(_config.IndexName); return i; });
                return resoponse.Created;
            }
        }

        public static async Task<ComplexLocation> GetLocationObjectByOpenidAsync(string openid)
        {
            var result = await _client.SearchAsync<ComplexLocation>(s => s.Query(q => q.QueryString(ss => ss.Query("Id:" + openid))));
            if(result.Total == 1)
                return result.Documents.First();
            return null;
        }

        public static async Task<double> GetDistanceBetweenAsync(string openid1, string openid2)
        {
            ComplexLocation l1 = await GetLocationObjectByOpenidAsync(openid1);
            ComplexLocation l2 = await GetLocationObjectByOpenidAsync(openid2);

            ComplexLocationCoordinate co1 = l1.Coordinate;
            ComplexLocationCoordinate co2 = l2.Coordinate;
            return GeoHelper.GetDistance(co1.Lat, co1.Lon, co2.Lat, co2.Lon);
        }

        public static ComplexLocation GetLocationObjectByOpenid(string openid)
        {
            var result = _client.Search<ComplexLocation>(s => s.Query(q => q.QueryString(ss => ss.Query("Id:" + openid))));
            if(result.Total == 1)
                return result.Documents.First();
            return null;
        }

        public static double GetDistanceBetween(ComplexLocationCoordinate c1, ComplexLocationCoordinate c2)
        {
            return GeoHelper.GetDistance(c1.Lat, c1.Lon, c2.Lat, c2.Lon);
        }

        public static double GetDistanceBetween(string openid1, string openid2)
        {
            ComplexLocation l1 = GetLocationObjectByOpenid(openid1);
            ComplexLocation l2 = GetLocationObjectByOpenid(openid2);

            ComplexLocationCoordinate co1 = l1.Coordinate;
            ComplexLocationCoordinate co2 = l2.Coordinate;
            return GeoHelper.GetDistance(co1.Lat, co1.Lon, co2.Lat, co2.Lon);
        }

        public static async Task<List<ComplexLocation>> GetDistanceInKmByIdAsync(string openid, double km)
        {
            var result = await _client.SearchAsync<ComplexLocation>(s => s.Query(q => q.QueryString(ss => ss.Query("Id:" + openid))));
            if(result.Total == 1)
            {
                try
                {
                    var co = result.Documents.First().Coordinate;
                    var results = await _client.SearchAsync<ComplexLocation>(s => s
                       .Filter(f => f.GeoDistance("Coordinate", fd => fd.Distance(km, GeoUnit.Kilometers).Location(co.Lat, co.Lon)))
                       .SortGeoDistance(sort => sort.OnField("Coordinate").PinTo(co.Lat, co.Lon).Ascending()));
                    return results.Documents.ToList();
                }
                catch(Exception ex)
                {
                    LogError(ex);
                    throw ex;
                }
            }
            return null;
        }
        public static async Task<List<ComplexLocation>> GetDistanceInKmByIdAsync(string openid, double km,int?isBusiness,int?gender,int?researchFieldId)
        {
            var result = await _client.SearchAsync<ComplexLocation>(s => s.Query(q => q.QueryString(ss => ss.Query("Id:" + openid))));
            if(result.Total == 1)
            {
                try
                {
                    var co = result.Documents.First().Coordinate;
                    List<FilterContainer> filterlist = new List<FilterContainer>();
                    filterlist.Add(new FilterDescriptor<ComplexLocation>().GeoDistance("Coordinate", fd => fd.Distance(km, GeoUnit.Kilometers).Location(co.Lat, co.Lon)));
                    if(isBusiness != null)
                        filterlist.Add(new FilterDescriptor<ComplexLocation>().Term("IsBusiness", isBusiness));
                    if(gender != null)
                        filterlist.Add(new FilterDescriptor<ComplexLocation>().Term("Gender", gender));
                    if(researchFieldId != null)
                        filterlist.Add(new FilterDescriptor<ComplexLocation>().Term("ResearchFieldId", researchFieldId));

                    var results = await _client.SearchAsync<ComplexLocation>(s => s
                       .Filter(new FilterDescriptor<ComplexLocation>().And(filterlist.ToArray()))
                       .SortGeoDistance(sort => sort.OnField("Coordinate").PinTo(co.Lat, co.Lon).Ascending()));
                    return results.Documents.ToList();
                }
                catch(Exception ex)
                {
                    LogError(ex);
                    throw ex;
                }
            }
            return null;
        }
        public static List<ComplexLocation> GetDistanceInKmById(string openid, double km)
        {
            var result = _client.Search<ComplexLocation>(s => s.Query(q => q.QueryString(ss => ss.Query("Id:" + openid))));
            if(result.Total == 1)
            {
                try
                {
                    var co = result.Documents.First().Coordinate;
                    var results = _client.Search<ComplexLocation>(s => s
       .Filter(f => f.GeoDistance("Coordinate", fd => fd.Distance(km, GeoUnit.Kilometers).Location(co.Lat, co.Lon)))
       .SortGeoDistance(sort => sort.OnField("Coordinate").PinTo(co.Lat, co.Lon).Ascending()));
                    return results.Documents.ToList();
                }
                catch(Exception ex)
                {
                    LogError(ex);
                    throw ex;
                }
            }
            return null;
        }

    }
}
