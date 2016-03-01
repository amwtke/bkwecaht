
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class T_UnivsDep
    {
        [Key]
        public int ID { get; set; }
        public int UnivsID { get; set; }
        public string DepName { get; set; }
        public string Alias { get; set; }
    }
}
