
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class pre_register
    {
        [Key]
        public int id { get; set; }
        public string accountemail { get; set; }
        public string name { get; set; }
        public string password { get; set; }
        public string html { get; set; }
        public int validate { get; set; }
        public DateTime createtime { get; set; }
    }
}
