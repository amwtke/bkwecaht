
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class Univs
    {
        [Key]
        public int ID { get; set; }
        public int UnivsID { get; set; }
        public string ProvinceID { get; set; }
        public string UnivsName { get; set; }
        public string Alias { get; set; }
    }
}
