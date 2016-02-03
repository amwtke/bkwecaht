
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class user_favorite
    {
        //ÔÞµÄÊý¾Ý
        [Key]
        public long id { get; set; }
        public DateTime? add_time { get; set; }
        public int? type { get; set; }
        public Guid user_account_uuid { get; set; }
        public Guid user_fav_account_uuid { get; set; }
        [ForeignKey("user_account_uuid")]
        public virtual UserInfo user_account_userinfo
        { get; set; }
        [ForeignKey("user_fav_account_uuid")]
        public virtual UserInfo user_fav_account_userinfo
        { get; set; }
    }
}
