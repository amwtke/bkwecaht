
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class ArticleNews
    {
        [Key]
        public long ArticleID { get; set; }
        public string NewsInfo { get; set; }
        public long ID { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public Nullable<bool> IsAnonymity { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
