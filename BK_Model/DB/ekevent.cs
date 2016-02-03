
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class ekevent
    {
        [Key]
        public long ID { get; set; }
        public string AccountEmail { get; set; }
        public Nullable<bool> IsPublic { get; set; }
        public string Title { get; set; }
        public string BodyText { get; set; }
        public Nullable<System.DateTime> PublicDate { get; set; }
        public Nullable<int> HitPoint { get; set; }
        public Nullable<int> ReadPoint { get; set; }
    }
}
