
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class VisitBetweenUser
    {
        [Key]
        public long Id
        { get; set; }
        public Nullable<System.DateTime> VisitTime
        { get; set; }
        public System.Guid UserGuest_uuid
        { get; set; }
        public System.Guid UserHost_uuid
        { get; set; }
        [ForeignKey("UserHost_uuid")]
        public virtual UserInfo UserHost_userinfo
        { get; set; }
        [ForeignKey("UserGuest_uuid")]
        public virtual UserInfo UserGuest_userinfo
        { get; set; }
    }
}
