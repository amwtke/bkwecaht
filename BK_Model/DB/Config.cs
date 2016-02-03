
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class Config
    {
        [Key]
        public long id { get; set; }
        public string key { get; set; }
        public string value { get; set; }
        public Nullable<System.DateTime> createTime { get; set; }
        public string description { get; set; }
    }
}
