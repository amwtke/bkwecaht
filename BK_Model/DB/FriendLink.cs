
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class FriendLink
    {
        public string link { get; set; }
        public string linkName { get; set; }
        public Nullable<System.DateTime> createTime { get; set; }
        [Key]
        public long id { get; set; }
    }
}
