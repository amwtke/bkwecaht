
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class Administrator
    {
        public string adminName { get; set; }
        public string password { get; set; }
        [Key]
        public long id { get; set; }
        public Nullable<System.DateTime> loginTime { get; set; }
    }
}
