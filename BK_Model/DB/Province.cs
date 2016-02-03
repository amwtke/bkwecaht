
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class Province
    {
        public string provinceId { get; set; }
        public string provinceName { get; set; }
        [Key]
        public long id { get; set; }
    }
}
