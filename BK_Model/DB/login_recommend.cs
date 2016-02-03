
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class login_recommend
    {
        [Key]
        public long id { get; set; }
        public string name { get; set; }
        public string recommendtext { get; set; }
        public Nullable<System.DateTime> createtime { get; set; }
        public System.Guid accountemail_uuid { get; set; }
    }
}
