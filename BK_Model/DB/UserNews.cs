
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class UserNews
    {
        public string NewsInfo { get; set; }
        [Key]
        public long Id { get; set; }
        public System.DateTime CreateTime { get; set; }
        public Nullable<bool> IsRecommend { get; set; }
        public string NewsType { get; set; }
        public string additional { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
