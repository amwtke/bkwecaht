
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class City
    {
        public string cityId { get; set; }
        public string cityName { get; set; }
        public string provinceId_Province { get; set; }
        [Key]
        public long id { get; set; }
    }
}
