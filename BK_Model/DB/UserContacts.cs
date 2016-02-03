
namespace BK.Model.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class UserContacts
    {
        public Nullable<bool> Status { get; set; }
        public Nullable<System.DateTime> AddTime { get; set; }
        [Key]
        public long Id { get; set; }
        public string Additional { get; set; }        
        public System.Guid AccountEmail_uuid { get; set; }
        public System.Guid ConAccount_uuid { get; set; }
        public System.Guid RequestUser_uuid { get; set; }
        //[ForeignKey("AccountEmail_uuid")]
        //public virtual UserInfo AccountEmail_userinfo { get; set; }
        [ForeignKey("ConAccount_uuid")]
        public virtual UserInfo ConAccount_userinfo { get; set; }
    }
}
