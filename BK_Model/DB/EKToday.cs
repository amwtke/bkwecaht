
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class EKToday
    {
        public string AccountEmail { get; set; }
        public Nullable<bool> IsExotic { get; set; }
        public Nullable<bool> IsPublic { get; set; }
        public Nullable<System.DateTime> IsTop { get; set; }
        public string Title { get; set; }
        public string ArticleType { get; set; }
        public string BodyText { get; set; }
        public string HeadPic { get; set; }
        public Nullable<System.DateTime> PublicDate { get; set; }
        public string Keywords { get; set; }
        public string Abstract { get; set; }
        public Nullable<int> HitPoint { get; set; }
        public Nullable<int> ReadPoint { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ID { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
