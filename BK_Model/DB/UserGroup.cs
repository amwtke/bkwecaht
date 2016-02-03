
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class UserGroup
    {
        public long GroupID { get; set; }
        public System.DateTime JoinTime { get; set; }
        [Key]
        public long Id { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<bool> IsOwner { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
