
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class group_article
    {
        [Key]
        public long ID { get; set; }
        public string Title { get; set; }
        public string BodyText { get; set; }
        public Nullable<System.DateTime> PublicDate { get; set; }
        public string Keywords { get; set; }
        public Nullable<int> HitPoint { get; set; }
        public Nullable<int> ReadPoint { get; set; }
        public Nullable<System.DateTime> IsTop { get; set; }
        public Nullable<System.DateTime> IsEssential { get; set; }
        public Nullable<bool> NoReply { get; set; }
        public string Abstract { get; set; }
        public string HeadPic { get; set; }
        public Nullable<long> groupID { get; set; }
        public Nullable<int> article_status { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
