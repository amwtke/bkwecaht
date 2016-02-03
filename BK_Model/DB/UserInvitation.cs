
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class UserInvitation
    {
        [Key]
        public long Id { get; set; }
        public string Account { get; set; }
        public string Message { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public Nullable<bool> IsPass { get; set; }
        public System.Guid Invitator_uuid { get; set; }
    }
}
