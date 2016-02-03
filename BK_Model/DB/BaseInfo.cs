
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class BaseInfo
    {
        [Key]
        public long id { get; set; }
        public string category { get; set; }
        public string content { get; set; }
        public string keyWords { get; set; }
        public string description { get; set; }
    }
}
