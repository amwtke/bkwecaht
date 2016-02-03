
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class GroupNews
    {
        [Key]
        public long Id { get; set; }
        public long GroupID { get; set; }
        public string NewsInfo { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public System.Guid Releaser_uuid { get; set; }
    }
}
