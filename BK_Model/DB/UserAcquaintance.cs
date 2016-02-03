
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class UserAcquaintance
    {
        [Key]
        public long Id { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<System.DateTime> AddTime { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
        public System.Guid ConAccount_uuid { get; set; }
    }
}
