
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class News
    {
        [Key]
        public long Id { get; set; }
        public Nullable<System.DateTime> ReleaseTime { get; set; }
        public long RelationId { get; set; }
        public int NewsType { get; set; }
        public string ReplayInfo { get; set; }
        public Nullable<bool> IsAnonymity { get; set; }
        public Nullable<long> ParentId { get; set; }
        public System.Guid ReplyUser_uuid { get; set; }
        public System.Guid AnswerUser_uuid { get; set; }
    }
}
